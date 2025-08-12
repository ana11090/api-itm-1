using api_itm.UserControler.SideBar;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace api_itm.UserControler
{
    /// <summary>
    /// A custom sidebar menu control built on top of a styled TreeView.
    /// Supports sections, items, click events, and zooming with Ctrl+MouseWheel.
    /// </summary>
    public partial class ControlSidebarMenu : UserControl
    {
        // The main menu UI element — a custom-styled TreeView
        private readonly StyledTreeView _tree = new StyledTreeView();

        // Current zoom level percentage (default 100%)
        private int _zoom = 100;

        /// <summary>
        /// Event raised when a menu item (not a section) is clicked.
        /// Passes the section and item text as a tuple.
        /// </summary>
        public event EventHandler<(string section, string item)> ItemClicked;

        /// <summary>
        /// Gets or sets the image list used for tree nodes.
        /// </summary>
        public ImageList MenuImageList
        {
            get => _tree.ImageList;
            set => _tree.ImageList = value;
        }

        /// <summary>
        /// Gets or sets the accent color for the menu.
        /// </summary>
        public Color AccentColor
        {
            get => _tree.AccentColor;
            set
            {
                _tree.AccentColor = value;
                _tree.Invalidate(); // Redraw to apply the new color
            }
        }

        /// <summary>
        /// Constructor — sets up the menu appearance, layout, and events.
        /// </summary>
        public ControlSidebarMenu()
        {
            InitializeComponent();

            // Make the hosting area obvious (optional, for debugging)
            this.BackColor = Color.White;

            // BASIC, VISIBLE SETTINGS (standard TreeView drawing)
            _tree.BackColor = Color.White;
            _tree.ForeColor = Color.Black;
            _tree.HideSelection = false;             // keep selection visible when focus changes
            _tree.FullRowSelect = true;              // highlight full width
            _tree.BorderStyle = BorderStyle.None;

            // Use NORMAL drawing to guarantee visibility first
            _tree.DrawMode = TreeViewDrawMode.Normal;  // <-- critical for now
            _tree.ShowLines = false;
            _tree.ShowRootLines = false;
            _tree.ShowPlusMinus = true;              // show expand arrows so you can tell sections exist

            // Size/spacing so nodes are comfortably visible
            _tree.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            _tree.ItemHeight = Math.Max(28, (int)Math.Ceiling(_tree.Font.Height * 1.6));

            // Layout
            _tree.Dock = DockStyle.Fill;
            Controls.Add(_tree);

            // Click handling
            _tree.NodeMouseClick += (s, e) =>
            {
                if (e.Node.Level == 0)   // section
                    e.Node.Toggle();     // expand/collapse
                else
                    ItemClicked?.Invoke(this, (e.Node.Parent.Text, e.Node.Text));
            };

            // Ctrl + MouseWheel zoom (still works in normal mode)
            _tree.MouseWheel += (s, e) =>
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    _zoom = e.Delta > 0 ? Math.Min(200, _zoom + 10) : Math.Max(70, _zoom - 10);
                    ApplyZoom();
                }
            };

            // Apply initial zoom
            ApplyZoom();

            // TEMP sanity: if you want to verify rendering immediately, uncomment:
            // _tree.Nodes.Add("Test Section").Nodes.Add("Test Item");
            // _tree.ExpandAll();
        }


        /// <summary>
        /// Applies the current zoom level to the menu font and row height.
        /// </summary>
        private void ApplyZoom()
        {
            float basePt = 10f; // Base font size in points
            float size = basePt * _zoom / 100f; // Adjust font size based on zoom %

            _tree.Font = new Font("Segoe UI", size, FontStyle.Regular);
            _tree.ItemHeight = Math.Max(28, (int)Math.Ceiling(_tree.Font.Height * 1.8));

            _tree.Invalidate(); // Redraw
        }

        /// <summary>
        /// Builds the menu structure from an array of section + items data.
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

                // Collapse root node so only main buttons show
                root.Collapse();
            }

             

            _tree.EndUpdate();
        }

        /// <summary>
        /// Adds a single menu item to an existing section, or creates the section if missing.
        /// </summary>
        public void AddMenuItem(string section, string item)
        {
            var root = _tree.Nodes.Cast<TreeNode>()
                                  .FirstOrDefault(n => n.Text == section)
                       ?? _tree.Nodes.Add(section); // Create section if not found

            root.Nodes.Add(item); // Add the item
        }

        /// <summary>
        /// Clears all menu items.
        /// </summary>
        public void ClearMenu() => _tree.Nodes.Clear();

        /// <summary>
        /// Selects a specific item in the menu, optionally triggering the click event.
        /// </summary>
        public void SelectItem(string section, string item, bool triggerClick = false)
        {
            var root = _tree.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == section);
            var child = root?.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == item);

            if (child != null)
            {
                _tree.SelectedNode = child; // Visually select item

                if (triggerClick)
                    ItemClicked?.Invoke(this, (section, item)); // Trigger event
            }
        }

        // Empty event handler for when the control loads
        private void ControlSidebarMenu_Load(object sender, EventArgs e)
        {
        }
    }
}
