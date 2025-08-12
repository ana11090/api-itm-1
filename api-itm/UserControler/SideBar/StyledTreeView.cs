using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace api_itm.UserControler.SideBar
{
    /// <summary>
    /// A custom TreeView with a modern "styled" look.
    /// Features:
    /// - Rounded row backgrounds
    /// - Hover and selection colors
    /// - Custom chevron expand/collapse indicators
    /// - Padding and indentation control
    /// </summary>
    public class StyledTreeView : TreeView
    {
        private TreeNode _hoverNode; // The node currently under the mouse

        // Theme customization properties
        public Color RowBackColor { get; set; } = Color.FromArgb(245, 247, 250); // Default row background
        public Color RowHoverColor { get; set; } = Color.FromArgb(230, 240, 255); // Background on hover
        public Color RowSelectedColor { get; set; } = Color.FromArgb(209, 232, 255); // Background on select
        public Color AccentColor { get; set; } = Color.FromArgb(0, 120, 215); // Used for chevrons and focus ring
        public Color RowTextColor { get; set; } = Color.FromArgb(22, 27, 34); // Text color
        public int RowCornerRadius { get; set; } = 8; // Rounded corners for rows
        public int RowHPad { get; set; } = 12; // Horizontal padding for content inside a row
        public int RowVPad { get; set; } = 8; // Vertical padding for row height calculation
        public int LevelIndent { get; set; } = 18; // Indentation per tree level
        public bool ShowChevrons { get; set; } = true; // Whether to show expand/collapse arrows

        public StyledTreeView()
        {
            // Font & row height
            Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            ItemHeight = 36;

            // Show standard tree lines and plus/minus
            ShowLines = true;
            ShowPlusMinus = true;
            ShowRootLines = true;

            // Borderless, keep selection highlighted even when losing focus
            BorderStyle = BorderStyle.None;
            HideSelection = false;

            // Let TreeView draw text normally so it’s visible
            DrawMode = TreeViewDrawMode.Normal;

            // Make sure colors are visible
            ForeColor = Color.Black;
            BackColor = Color.White;

            // Reduce flicker
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // Hover tracking (no custom drawing yet — just ready for later)
            MouseMove += (s, e) =>
            {
                var n = GetNodeAt(e.Location);
                if (!ReferenceEquals(n, _hoverNode))
                {
                    _hoverNode = n;
                    // Only repaint if we switch to owner-draw later
                }
            };

            MouseLeave += (s, e) =>
            {
                _hoverNode = null;
            };
        }


        /// <summary>
        /// Draws each node with custom style instead of default TreeView style.
        /// </summary>
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            // Always draw background
            e.Graphics.FillRectangle(new SolidBrush(BackColor), e.Bounds);

            // Decide text color
            Color textColor = ForeColor;
            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(197, 224, 255)), e.Bounds);
                textColor = Color.Black;
            }
            else if ((e.State & TreeNodeStates.Hot) == TreeNodeStates.Hot)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(213, 232, 255)), e.Bounds);
                textColor = Color.Black;
            }

            // Draw the node text
            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                Font,
                e.Bounds,
                textColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );

            // Draw default expand/collapse icon and lines if needed
            base.OnDrawNode(e);
        }

        /// <summary>
        /// Handles mouse clicks — select node and toggle expansion when clicking near left edge.
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var node = GetNodeAt(e.Location);
            if (node != null)
            {
                SelectedNode = node;
                if (node.Nodes.Count > 0 && e.X < node.Bounds.Left + 24)
                    node.Toggle();
            }
        }

        /// <summary>
        /// Calculates the full rectangle to draw for a node row.
        /// </summary>
        private Rectangle GetRowRect(TreeNode node)
        {
            var b = node.Bounds;
            int h = Math.Max(ItemHeight, Font.Height + RowVPad * 2);
            return new Rectangle(4, b.Top + 1, ClientSize.Width - 8, h - 2);
        }

        /// <summary>
        /// Creates a rounded rectangle path for drawing backgrounds or borders.
        /// </summary>
        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            if (radius <= 0)
            {
                var p = new GraphicsPath();
                p.AddRectangle(r);
                return p;
            }

            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Draws a chevron (arrow) for expand/collapse.
        /// </summary>
        private static void DrawChevron(Graphics g, int x, int y, int size, bool expanded, Color color)
        {
            using var pen = new Pen(color, 2);
            if (expanded)
            {
                // Downward arrow (expanded)
                g.DrawLines(pen, new[]
                {
                    new Point(x, y - size / 4),
                    new Point(x + size / 2, y + size / 4),
                    new Point(x + size, y - size / 4)
                });
            }
            else
            {
                // Right-pointing arrow (collapsed)
                g.DrawLines(pen, new[]
                {
                    new Point(x, y - size / 2),
                    new Point(x + size / 2, y),
                    new Point(x, y + size / 2)
                });
            }
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.Clear(BackColor);
        }
    }
}
