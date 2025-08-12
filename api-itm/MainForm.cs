using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using api_itm.UserControler;   // ControlSidebarMenu
using api_itm.Infrastructure; // TabManager
using api_itm.UserControler.UserProfile; 

namespace api_itm
{
    public partial class MainForm : Form
    {
        private readonly AppDbContext _db;

        // UI elements
        private SplitContainer _split;     // splits the window: left = menu, right = content
        private ControlSidebarMenu _menu;  // custom sidebar menu control
        private TabControl _tabs;          // shows opened pages in tabs

        // Helper for managing tabs
        private TabManager _tabManager;

        public MainForm(AppDbContext db)
        {
            InitializeComponent();
            _db = db;

            // Build the UI layout and wire up events
            BuildLayout();
            WireEvents();

            // Do things that require the form to be fully sized
            this.Shown += (_, __) =>
            {
                // Set final min sizes for each panel
                _split.Panel1MinSize = 220;
                _split.Panel2MinSize = 400;

                // Set starting position of the splitter
                SetSplitDistance(260);

                // Setup tab look/size
                _tabs.DrawMode = TabDrawMode.Normal;
                _tabs.Appearance = TabAppearance.Normal;
                _tabs.SizeMode = TabSizeMode.Fixed;
                _tabs.ItemSize = new Size(120, 32);
                _tabs.Padding = new Point(16, 4);
                _tabs.Visible = true;
                _tabs.BringToFront();


                // Fill the sidebar with menu items
                PopulateMenu();

                // Make sure menu is on top of its panel
                _menu.BringToFront();
            };

            // Keep the splitter position valid when resizing the form
            this.Resize += (_, __) => SetSplitDistance(_split.SplitterDistance);
        }

        /// <summary>
        /// Build the left-right layout:
        /// Panel1 = sidebar menu, Panel2 = tabs/content
        /// </summary>
        private void BuildLayout()
        {
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical, // left/right split
                SplitterWidth = 6,
                Panel1MinSize = 20, // temporarily low to avoid size errors before Shown
                Panel2MinSize = 0
            };
            Controls.Add(_split);

            // Create the menu and dock it into the left panel
            _menu = new ControlSidebarMenu { Dock = DockStyle.Fill };
            _split.Panel1.Controls.Add(_menu);

            // Create the tab control and dock it into the right panel
            _tabs = new TabControl { Dock = DockStyle.Fill };
            _split.Panel2.Controls.Add(_tabs);

            // Create helper to manage opening/activating tabs
            _tabManager = new TabManager(_tabs);
        }

        /// <summary>
        /// Safely set the splitter position without breaking min size rules
        /// </summary>
        private void SetSplitDistance(int desired)
        {
            if (_split.Width <= 0) return; // form not yet sized

            int min = _split.Panel1MinSize;
            int max = Math.Max(min, _split.Width - _split.Panel2MinSize);

            // Clamp desired distance between min and max
            _split.SplitterDistance = Math.Max(min, Math.Min(max, desired));
        }

        /// <summary>
        /// Handle menu clicks -> open/activate a tab
        /// </summary>
        private void WireEvents()
        {
            _menu.ItemClicked += (s, e) =>
            {
                var (section, item) = e;
                var key = string.IsNullOrEmpty(item) ? section : $"{section}:{item}";
                var title = string.IsNullOrEmpty(item) ? section : item;

                Control CreateContent()
                {
                    // Root-only item: Profil utilizator
                    if (section == "Profil utilizator")
                    {
                        var uc = new ControlDetailsUserProfile(_db); // or parameterless if that’s your ctor
                        uc.Dock = DockStyle.Fill;
                        return uc;
                    }

                    // Other items (example fallback)
                    switch (item)
                    {
                        case "Inregistrare salariat":
                            // return new ControlInregistrareSalariat(_db) { Dock = DockStyle.Fill };
                            break;
                            // add more cases as you implement
                    }

                    // Fallback placeholder
                    return new Label
                    {
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Segoe UI", 12),
                        Text = $"{section} → {item}"
                    };
                }

                _tabManager.OpenOrActivate(key, CreateContent, title);
                _tabs.BringToFront();
            };
        }


        /// <summary>
        /// Fill the sidebar with menu sections + items
        /// </summary>
        private void PopulateMenu()
        {
            _menu.BuildMenu(new[]
            {
                ("Profil utilizator",    null ),
                ("Salariat",   new[] { "Inregistrare salariat", "HG Agreement", "Employment Agreement", "Supplier Agreements" }),
                ("Contract", new[] { " Agreements contract" })
              //  ("Financial Reports", new[] { "Income Statement", "Balance Sheet", "Profit and Loss", "Cash Flow" }),
               // ("HR Reports",        new[] { "Employee Performance", "Attendance Record", "Employee Satisfaction" }),
               // ("Labels",            new[] { "Addresses" })
            }, expandAll: false);
        }

        // Optional load event
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Example: WindowState = FormWindowState.Maximized;
        }
    }
}
