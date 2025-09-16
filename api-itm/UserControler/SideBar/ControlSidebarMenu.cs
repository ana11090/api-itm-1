using api_itm.UserControler.SideBar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace api_itm.UserControler
{
    /// <summary>
    /// Sidebar menu on top of a styled TreeView.
    /// - Keeps your 2-level API (sections → items) and ItemClicked(section,item)
    /// - Adds hierarchical API for 3+ levels + ItemClickedPath(string[] fullPath)
    /// - Ctrl+MouseWheel zoom preserved
    /// </summary>
    public partial class ControlSidebarMenu : UserControl
    {
        // Main UI element
        private readonly StyledTreeView _tree = new StyledTreeView();

        // Zoom percent
        private int _zoom = 100;

        /// <summary>Legacy event (still supported): fires for 2-level menus.</summary>
        public event EventHandler<(string section, string item)> ItemClicked;

        /// <summary>New event: fires for ANY depth, sends full path from root to leaf.</summary>
        public event EventHandler<string[]> ItemClickedPath;

        public ImageList MenuImageList
        {
            get => _tree.ImageList;
            set => _tree.ImageList = value;
        }

        public Color AccentColor
        {
            get => _tree.AccentColor;
            set { _tree.AccentColor = value; _tree.Invalidate(); }
        }

        public ControlSidebarMenu()
        {
            InitializeComponent();

            BackColor = Color.White;

            // Basic look
            _tree.BackColor = Color.White;
            _tree.ForeColor = Color.Black;
            _tree.HideSelection = false;
            _tree.FullRowSelect = true;
            _tree.BorderStyle = BorderStyle.None;

            // Keep standard drawing for clarity
            _tree.DrawMode = TreeViewDrawMode.Normal;
            _tree.ShowLines = false;
            _tree.ShowRootLines = false;
            _tree.ShowPlusMinus = false;

            _tree.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _tree.ItemHeight = Math.Max(28, (int)Math.Ceiling(_tree.Font.Height * 1.6));

            _tree.Dock = DockStyle.Fill;
            Controls.Add(_tree);

            // SINGLE click handler (replaces the two duplicates)
            _tree.NodeMouseClick += (s, e) => HandleNodeClick(e.Node);

            // Ctrl + MouseWheel zoom
            _tree.MouseWheel += (s, e) =>
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    _zoom = e.Delta > 0 ? Math.Min(200, _zoom + 10) : Math.Max(70, _zoom - 10);
                    ApplyZoom();
                }
            };

            ApplyZoom();
        }

        private void ApplyZoom()
        {
            float basePt = 10f;
            float size = basePt * _zoom / 100f;

            _tree.Font = new Font("Segoe UI", size, FontStyle.Regular);
            _tree.ItemHeight = Math.Max(28, (int)Math.Ceiling(_tree.Font.Height * 1.8));
            _tree.Invalidate();
        }

        // -------------------- OLD API (2 LEVELS) – kept as-is --------------------

        /// <summary>
        /// Build a simple 2-level menu: sections → items.
        /// </summary>
        public void BuildMenu((string section, string[] items)[] data, bool expandAll = false)
        {
            _tree.BeginUpdate();
            _tree.Nodes.Clear();

            foreach (var (section, items) in data)
            {
                var root = _tree.Nodes.Add(section);
                foreach (var it in items ?? Array.Empty<string>())
                    root.Nodes.Add(it);

                if (expandAll) root.Expand();
                else root.Collapse();
            }

            if (expandAll) _tree.ExpandAll();
            _tree.EndUpdate();
        }

        /// <summary>Add a single item to a section (creates section if missing).</summary>
        public void AddMenuItem(string section, string item)
        {
            var root = _tree.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == section)
                       ?? _tree.Nodes.Add(section);

            root.Nodes.Add(item);
        }

        public void ClearMenu() => _tree.Nodes.Clear();

        public void SelectItem(string section, string item, bool triggerClick = false)
        {
            var root = _tree.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == section);
            var child = root?.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == item);

            if (child != null)
            {
                _tree.SelectedNode = child;
                if (triggerClick) ItemClicked?.Invoke(this, (section, item));
            }
        }

        // -------------------- NEW API (N LEVELS) --------------------

        public record MenuNode(string Title, MenuNode[] Children = null);

        /// <summary>Helper to declare a group (folder) with children.</summary>
        public static MenuNode Group(string title, params MenuNode[] children) => new(title, children);

        /// <summary>Helper to declare a leaf (clickable item).</summary>
        public static MenuNode Leaf(string title) => new(title, null);

        /// <summary>
        /// Build a hierarchical menu (3+ levels supported).
        /// </summary>
        public void BuildMenu(MenuNode[] roots, bool expandAll = false)
        {
            _tree.BeginUpdate();
            _tree.Nodes.Clear();

            foreach (var root in roots)
                _tree.Nodes.Add(Build(root));

            if (expandAll) _tree.ExpandAll();
            _tree.EndUpdate();

            static TreeNode Build(MenuNode m)
            {
                var tn = new TreeNode(m.Title);
                if (m.Children != null && m.Children.Length > 0)
                    foreach (var c in m.Children) tn.Nodes.Add(Build(c));
                return tn;
            }
        }

        // -------------------- Click logic (shared) --------------------

        private void HandleNodeClick(TreeNode node)
        {
            if (node == null) return;

            // If it's a group (has children): toggle expand. If it has NO children, treat as leaf.
            if (node.Nodes.Count > 0)
            {
                node.Toggle();
                return;
            }

            // Leaf → raise events
            var path = GetPath(node);
            ItemClickedPath?.Invoke(this, path);

            // Keep legacy 2-level event alive for your existing code
            string section = path.Length > 0 ? path[0] : node.Text;
            string item = path.Length > 1 ? path[^1] : node.Text; // last segment as item
            ItemClicked?.Invoke(this, (section, item));
        }

        private static string[] GetPath(TreeNode node)
        {
            var stack = new Stack<string>();
            var cur = node;
            while (cur != null)
            {
                stack.Push(cur.Text);
                cur = cur.Parent;
            }
            return stack.ToArray();
        }

        // Designer stub
        private void ControlSidebarMenu_Load(object sender, EventArgs e) { }
    }
}
