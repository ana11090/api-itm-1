using api_itm.Infrastructure; // TabManager
using api_itm.UserControler;   // ControlSidebarMenu
using api_itm.UserControler.Contracts;
using api_itm.UserControler.Contracts.Cessation___Reactivation;
using api_itm.UserControler.Contracts.Operations;
using api_itm.UserControler.Contracts.Suspended;
using api_itm.UserControler.Employee;
using api_itm.UserControler.UserProfile;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace api_itm
{
    public partial class MainForm : Form
    {
        private readonly ControlerAddEmployeeView _employeeView;
        

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
                _split.Panel1MinSize = 220;
                _split.Panel2MinSize = 400;
                SetSplitDistance(260);

                // your current tab setup
                _tabs.DrawMode = TabDrawMode.Normal;        // will be overridden below
                _tabs.Appearance = TabAppearance.Normal;
                _tabs.SizeMode = TabSizeMode.Fixed;
                _tabs.ItemSize = new Size(120, 32);
                _tabs.Padding = new Point(16, 4);
                _tabs.Visible = true;
                _tabs.BringToFront();

                _tabs.SizeMode = TabSizeMode.Normal;          // ←  (variable width)
                _tabs.ItemSize = new Size(_tabs.ItemSize.Width, 32);

                EnableClosableTabs();                         // draw close buttons, handle clicks

                PopulateMenu();
                _menu.BringToFront();



            };

            // git hub method update installation kit
            // Keep the splitter position valid when resizing the form
            //this.Resize += (_, __) => SetSplitDistance(_split.SplitterDistance);
            //WebClient webClient = new WebClient();
            //var client = new WebClient();
            //if (!webClient.DownloadString("link to web host/Version.txt").Contains("1.0.0"))
            //{
            //    if (MessageBox.Show("A new update is available! Do you want to download it?", "Demo", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            //    {
            //        try
            //        {
            //            if (File.Exists(@".\MyAppSetup.msi")) { File.Delete(@".\MyAppSetup.msi"); }
            //            client.DownloadFile("link to web host/MyAppSetup.zip", @"MyAppSetup.zip");
            //            string zipPath = @".\MyAppSetup.zip";
            //            string extractPath = @".\";
            //            ZipFile.ExtractToDirectory(zipPath, extractPath);
            //            Process process = new Process();
            //            process.StartInfo.FileName = "msiexec.exe";
            //            process.StartInfo.Arguments = string.Format("/i MyAppSetup.msi");
            //            this.Close();
            //            process.Start();
            //        }
            //        catch
            //        {
            //        }
            //    }
            //}
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
                        case "Adaugare date salariati":
                            var employeeView = Program.App.Services.GetRequiredService<ControlerAddEmployeeView>();
                            employeeView.Dock = DockStyle.Fill;
                            return employeeView;
                    }
                    switch (item)
                    {
                        case "Modificare salariat":
                            var employeeView = Program.App.Services.GetRequiredService<ControlerModifyEmployeeView>();
                            employeeView.Dock = DockStyle.Fill;
                            return employeeView;
                    }
                    switch (item)
                    {
                        case "Corectie salariat":
                            var correctionContractView = Program.App.Services.GetRequiredService<ControlerCorrectionEmployeeView>();
                            correctionContractView.Dock = DockStyle.Fill;
                            return correctionContractView;
                    }//CorectieSalariat 
                    switch (item)
                    {
                        case "Radiere salariat":
                            var correctionContractView = Program.App.Services.GetRequiredService<ControlerDeleteEmployeeView>();
                            correctionContractView.Dock = DockStyle.Fill;
                            return correctionContractView;
                    }//RadiereSalariat
                    switch (item)
                    {
                        case "Inregistrare contracte":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerAddContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    switch (item)
                    {
                        case "Modificare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerModificationContractsView>(); //ControlerModificationContractsView
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    switch (item)
                    {
                        case "Corectie contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerrCorrectionContractsView>(); //ControlerrCorrectionContractsView
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }//CorectieContract
                    //Suspendare contract
                    switch (item)
                    {
                        case "Suspendare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerSuspendedContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    switch (item)
                    {
                        case "Corectie suspendare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerCorrectionSuspendedContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    switch (item)
                    {
                        case "Modificare suspendare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerModificationSuspendedContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    switch (item)
                    {
                        case "Anulare suspendare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerCancelSuspendedContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    } 
                    switch (item)
                    {
                        case "Incetare suspendare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerStopedSuspendedContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    switch (item)
                    {
                        case "Corectie incetare suspendare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerStopedSuspendedContractsView>();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
                    }
                    //
                    switch (item)
                    {
                        case "Incetare contract":
                            var contracteView = Program.App.Services.GetRequiredService<ControlerCorrectionStopedSuspendedContractsView> ();
                            contracteView.Dock = DockStyle.Fill;
                            return contracteView;
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
        ControlSidebarMenu.Group("Profil utilizator",
            ControlSidebarMenu.Leaf("Profil utilizator")   // button inside the drop-down
        ),

        ControlSidebarMenu.Group("Salariati",
            ControlSidebarMenu.Leaf("Adaugare date salariati"),
             ControlSidebarMenu.Group("Modificare",
                ControlSidebarMenu.Leaf("Modificare salariat"),
                ControlSidebarMenu.Leaf("Corectie salariat"),//CorectieSalariat
               ControlSidebarMenu.Leaf("Radiere salariat")// RadiereSalariat
            )
        ),

        ControlSidebarMenu.Group("Contracte",
            ControlSidebarMenu.Leaf("Inregistrare contracte"),
            ControlSidebarMenu.Group("Operatii contract",
                ControlSidebarMenu.Leaf("Modificare contract"),
                ControlSidebarMenu.Leaf("Corectie contract")//CorectieContract
            ),
            ControlSidebarMenu.Group("Suspendare",
                ControlSidebarMenu.Leaf("Suspendare contract"), //ControlerSuspendedContractsView
                ControlSidebarMenu.Leaf("Corectie suspendare contract"), // ControlerCorrectionSuspendedContractsView
                ControlSidebarMenu.Leaf("Modificare suspendare contract"), // ControlerModificationSuspendedContractsView
                ControlSidebarMenu.Leaf("Anulare suspendare contract")//, // ControlerCancelSuspendedContractsView
                //ControlSidebarMenu.Leaf("Incetare suspendare contract"), // ControlerStopedSuspendedContractsView
                //ControlSidebarMenu.Leaf("Corectie incetare suspendare contract") // ControlerCorrectionStopedSuspendedContractsView
            ),
            ControlSidebarMenu.Group("Incetare - Reactivare",
                ControlSidebarMenu.Leaf("Incetare contract")
            )
        )
    }, expandAll: false);
        }


        // Optional load event
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Example: WindowState = FormWindowState.Maximized;
        }

        // --- closable tabs support ---

        // Size/margins for the small close button (×) drawn on each tab
        private const int CloseBtnSize = 12;
        private const int CloseBtnMargin = 8;

        private void EnableClosableTabs()
        {
            _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;   // owner draw
            _tabs.Padding = new Point(30, 6);             // reserve room for [x] on the right
            _tabs.Multiline = false;                       // single row (optional)

            _tabs.DrawItem += (s, e) =>
            {
                var tab = _tabs.TabPages[e.Index];
                var bounds = e.Bounds;

                // close box rect (12x12) near right edge of the tab
                var closeRect = new Rectangle(
                    bounds.Right - 18,
                    bounds.Y + (bounds.Height - 12) / 2,
                    12, 12);

                // text area = tab rect minus left padding and the close box
                var textRect = new Rectangle(
                    bounds.X + 8,
                    bounds.Y,
                    Math.Max(0, bounds.Width - 8 - (bounds.Right - closeRect.X) - 4),
                    bounds.Height);

                // draw background (optional: highlight selected)
                using (var bg = new SolidBrush(_tabs.SelectedIndex == e.Index
                    ? SystemColors.ControlLightLight
                    : SystemColors.Control))
                {
                    e.Graphics.FillRectangle(bg, bounds);
                }

                // draw text with ellipsis if still too long
                TextRenderer.DrawText(
                    e.Graphics,
                    tab.Text,
                    _tabs.Font,
                    textRect,
                    SystemColors.ControlText,
                    TextFormatFlags.VerticalCenter
                    | TextFormatFlags.Left
                    | TextFormatFlags.EndEllipsis
                    | TextFormatFlags.NoPadding);

                // draw the close button
                ControlPaint.DrawCaptionButton(
                    e.Graphics,
                    closeRect,
                    CaptionButton.Close,
                    ButtonState.Flat);

                e.DrawFocusRectangle();
            };

            _tabs.MouseDown += (s, e) =>
            {
                for (int i = 0; i < _tabs.TabPages.Count; i++)
                {
                    var r = _tabs.GetTabRect(i);
                    var closeRect = new Rectangle(r.Right - 18, r.Y + (r.Height - 12) / 2, 12, 12);
                    if (closeRect.Contains(e.Location))
                    {
                        var page = _tabs.TabPages[i];
                        _tabs.TabPages.Remove(page);
                        page.Dispose();
                        break;
                    }
                }
            };
        }

        private Rectangle GetCloseRect(Rectangle tabBounds)
        {
            // square on the right side of the tab header
            var x = tabBounds.Right - CloseBtnMargin - CloseBtnSize;
            var y = tabBounds.Top + (tabBounds.Height - CloseBtnSize) / 2;
            return new Rectangle(x, y, CloseBtnSize, CloseBtnSize);
        }

        private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var g = e.Graphics;
            var page = _tabs.TabPages[e.Index];
            var r = _tabs.GetTabRect(e.Index);

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Background
            using (var b = new SolidBrush(selected ? SystemColors.ControlLightLight : SystemColors.Control))
                g.FillRectangle(b, r);

            // Text (truncate if needed, leaving room for the "×")
            var closeRect = GetCloseRect(r);
            var textBounds = Rectangle.Inflate(r, -8, -4);
            textBounds.Width = closeRect.Left - textBounds.Left - 6;

            TextRenderer.DrawText(
                g,
                page.Text,
                _tabs.Font,
                textBounds,
                SystemColors.ControlText,
                TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            // Close button box (optional light border)
            using (var p = new Pen(SystemColors.ControlDark))
                g.DrawRectangle(p, closeRect);

            // Draw the "×" (two diagonal lines)
            var inset = 3;
            var x1 = closeRect.Left + inset;
            var y1 = closeRect.Top + inset;
            var x2 = closeRect.Right - inset;
            var y2 = closeRect.Bottom - inset;

            using (var p2 = new Pen(SystemColors.ControlText, 1.5f))
            {
                g.DrawLine(p2, x1, y1, x2, y2);
                g.DrawLine(p2, x1, y2, x2, y1);
            }

            // Focus rectangle (optional)
            e.DrawFocusRectangle();
        }

        private void Tabs_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            for (int i = 0; i < _tabs.TabPages.Count; i++)
            {
                var headerRect = _tabs.GetTabRect(i);
                var closeRect = GetCloseRect(headerRect);

                if (closeRect.Contains(e.Location))
                {
                    CloseTabAt(i);
                    break;
                }
            }
        }

        private void CloseTabAt(int index)
        {
            if (index < 0 || index >= _tabs.TabPages.Count) return;

            var page = _tabs.TabPages[index];

            // Dispose hosted content to release resources
            foreach (Control c in page.Controls) c.Dispose();

            // Remove & dispose the tab page
            _tabs.TabPages.RemoveAt(index);
            page.Dispose();

            // Select a sensible tab after close
            if (_tabs.TabPages.Count > 0)
            {
                _tabs.SelectedIndex = Math.Min(index, _tabs.TabPages.Count - 1);
            }
        }

    }
}
