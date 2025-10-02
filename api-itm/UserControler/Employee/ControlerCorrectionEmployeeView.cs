using api_itm.Data.Entity.Ru.Contracts;
using api_itm.Data.Entity.Ru.Reges;
using api_itm.Infrastructure;
using api_itm.Infrastructure.Sessions;
using api_itm.Models.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static api_itm.Program;

namespace api_itm.UserControler.Employee
{
    public partial class ControlerCorrectionEmployeeView : UserControl
    {
        private const int OP_CORECTARE_SALARIAT = 3; // always change this in every controller

        #region Copy in the future!
        private readonly AppDbContext _db;
        private Label lblTitle;

        private CheckBox _chkSelectAll;
        private Label _lblCount;
        private Button _btnSendSelected;
        private Panel _topBar;
        private Panel _bottomBar;
        private DataGridView dgvViewSalariati;
        private ISessionContext _session;

        private const string SelectColName = "Selectat";
        private const string RowNoColName = "No";
        private const string PersonIdPropertyName = "personId";

        // ===================== REGES API SETTINGS =====================
        private const string BaseUrl = "https://api.dev.inspectiamuncii.org/";   // <- set your actual API host
        private const string InregistrareSalariatUrl = "api/Salariat";

        private const string PollMessageUrl = "api/Status/PollMessage";   // POST, no body
        private const string ReadMessageUrl = "api/Status/ReadMessage";   // POST, no body
        private const string CommitReadUrl = "api/Status/CommitRead";    // POST, no body


#if DEBUG
        private const bool DEBUG_SHOW_PERSON_ID = true;
#else
        private const bool DEBUG_SHOW_PERSON_ID = false;
#endif

        private HashSet<int> _personsWithRegesId = new HashSet<int>();

        // // search 
        // UI
        private TextBox _txtSearch;
        private Button _btnClearSearch;

        // data cache
        private object _allRowsData;    // original full List<anon>


        private const string ConsumerId = "winforms-dev-1"; // sau string.Empty pentru implicit

        // sort state + cache for current rows (anonymous type)
        private object _rowsData;                 // List<anon>
        private Type _rowItemType;               // anon item type
        private string _lastSortProp;            // last sorted property
        private ListSortDirection _lastSortDir = ListSortDirection.Descending;

        // nulls-last comparer for object keys
        private static readonly IComparer<object> _nullsLast = Comparer<object>.Create((a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;    // nulls last
            if (b == null) return -1;
            if (a.GetType() == b.GetType() && a is IComparable ca) return ca.CompareTo(b);
            return string.Compare(a.ToString(), b.ToString(), StringComparison.CurrentCulture);
        });

        //    private static string Trunc(string s, int max = 600)
        //=> string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max) + "...[truncated]");

        private string WithConsumer(string path)
            => string.IsNullOrWhiteSpace(ConsumerId) ? path : $"{path}?consumerId={Uri.EscapeDataString(ConsumerId)}";

        //private const string QueueResultsUrl = "api/queue/results";
        //private const string QueueAckUrl = "api/queue/ack";

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        public ControlerCorrectionEmployeeView(AppDbContext db, ISessionContext session)
        {
            InitializeComponent();
            _db = db;

            _session = session;
            BuildBarsAndLayout();
            WireGrid();
        }
        private void WireGrid()
        {
            dgvViewSalariati.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvViewSalariati.IsCurrentCellDirty)
                    dgvViewSalariati.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvViewSalariati.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvViewSalariati.Columns[e.ColumnIndex].Name == SelectColName)
                    UpdateCounts();
            };

            dgvViewSalariati.DataBindingComplete += (_, __) =>
            {
                EnsureSpecialColumns();
                RenumberRows();
                UpdateCounts();
                ApplyRowColorsByRegesId();
            };

            dgvViewSalariati.Sorted += (_, __) => RenumberRows();
            dgvViewSalariati.RowsAdded += (_, __) => { RenumberRows(); UpdateCounts(); };
            dgvViewSalariati.RowsRemoved += (_, __) => { RenumberRows(); UpdateCounts(); };

            dgvViewSalariati.CellDoubleClick += async (_, __) => await PreviewSelectedRowJsonAsync();
            dgvViewSalariati.ColumnHeaderMouseClick += (_, e) => OnHeaderClick(e.ColumnIndex);

            dgvViewSalariati.KeyDown += (_, e) => { if (e.Control && e.KeyCode == Keys.F) { _txtSearch.Focus(); e.Handled = true; } };


        }

        private void ApplySort(string propName, bool desc)
        {
            if (_rowsData == null || _rowItemType == null) return;

            var pi = _rowItemType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null) return;

            // IEnumerable<object> peste lista curentă
            var enumerable = ((IEnumerable)_rowsData).Cast<object>();

            var sortedEnum = desc
                ? enumerable.OrderByDescending(o => pi.GetValue(o, null), _nullsLast)
                : enumerable.OrderBy(o => pi.GetValue(o, null), _nullsLast);

            // Cast<T> + ToList<T> pentru tipul anonim
            var castM = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(_rowItemType);
            var toListM = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(_rowItemType);
            var casted = castM.Invoke(null, new object[] { sortedEnum });
            var sortedListObj = toListM.Invoke(null, new object[] { casted });

            dgvViewSalariati.DataSource = sortedListObj;
            _rowsData = sortedListObj; // noua ordine

            // păstrăm UX-ul existent
            RenumberRows();
            UpdateCounts();
            ApplyRowColorsByRegesId();
        }


        private void OnHeaderClick(int colIndex)
        {
            if (colIndex < 0 || colIndex >= dgvViewSalariati.Columns.Count) return;

            var clickedCol = dgvViewSalariati.Columns[colIndex];
            if (clickedCol == null) return;
            if (clickedCol.Name == SelectColName || clickedCol.Name == RowNoColName) return;

            var prop = string.IsNullOrWhiteSpace(clickedCol.DataPropertyName) ? clickedCol.Name : clickedCol.DataPropertyName;
            if (string.IsNullOrWhiteSpace(prop)) return;

            var wantDesc = (_lastSortProp == prop) ? (_lastSortDir == ListSortDirection.Ascending) : true;

            // rebind (this recreates columns)
            ApplySort(prop, wantDesc);

            // after rebinding, work with the NEW column instances
            foreach (DataGridViewColumn c in dgvViewSalariati.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            var newCol = dgvViewSalariati.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.DataPropertyName, prop, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, prop, StringComparison.OrdinalIgnoreCase));

            if (newCol != null)
                newCol.HeaderCell.SortGlyphDirection = wantDesc ? SortOrder.Descending : SortOrder.Ascending;

            _lastSortProp = prop;
            _lastSortDir = wantDesc ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }


        private int? GetPersonIdFromRow(DataGridViewRow row)
        {
            if (row == null) return null;

            // 1) Preferred: read from the bound object (anonymous type has "personId")
            var item = row.DataBoundItem;
            if (item != null)
            {
                var prop = item.GetType().GetProperty(
                    "personId",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null)
                {
                    var value = prop.GetValue(item, null);
                    if (value == null) return null;

                    if (value is int i) return i;

                    var ni = value as int?;
                    if (ni.HasValue) return ni.Value;

                    if (int.TryParse(value.ToString(), out var parsed)) return parsed;
                    return null;
                }
            }

            // 2) Fallback: read from the cell whose DataPropertyName == "personId"
            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    var val = row.Cells[col.Index].Value;
                    if (val == null || val == DBNull.Value) return null;

                    if (val is int i) return i;

                    var ni = val as int?;
                    if (ni.HasValue) return ni.Value;

                    if (int.TryParse(val.ToString(), out var parsed)) return parsed;
                    return null;
                }
            }

            return null;
        }


        /// <summary>
        /// Return the json preview on click on selected row 
        /// </summary>
        /// <returns></returns>
        private async Task PreviewSelectedRowJsonAsync()
        {
            if (dgvViewSalariati.CurrentRow == null) return;

            int? personId = GetPersonIdFromRow(dgvViewSalariati.CurrentRow);
#if DEBUG
            Debug.WriteLine($"[DEBUG] Selected row personId={personId}");
#endif

            if (personId == null)
            {
                MessageBox.Show("Cannot detect PersonId for the selected row.");
                return;
            }

            await PreviewJsonForPerson(personId.Value);
        }

        private async Task PreviewJsonForPerson(int personId)
        {
            var payload = await BuildPayloadForPerson(personId);

            // also make sure names from DB are fixed BEFORE mapping (see earlier message)
            var json = RegesJson.SanitizeAndSerialize(payload);

            ShowJsonPreview(json);
        }
        [Conditional("DEBUG")]
        private void ShowJsonPreview(string json)
        {
            using var dlg = new Form
            {
                Text = "Preview JSON",
                StartPosition = FormStartPosition.CenterParent,
                Width = 900,
                Height = 700
            };

            var txt = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10f),
                Text = json
            };

            dlg.Controls.Add(txt);
            dlg.ShowDialog(this);
        }


        private async Task<EmployeeView> BuildPayloadForPerson(int personId)
        {
            // 1) reuse the original builder (unchanged!)
            var employeeView = Program.App.Services.GetRequiredService<ControlerAddEmployeeView>();
            var payload = await employeeView.BuildPayloadForPerson(personId);

            // 2) flip to Corectie
            payload.Header.Operation = "CorectieSalariat";

            // 3) fetch reges_salariat_id from idsreges_salariat table
            var salariatId = await _db.RegesSyncs
                .Where(x => x.PersonId == personId && x.RegesEmployeeId != null)
                .OrderByDescending(x => x.Id) // latest record, in case of history
                .Select(x => x.RegesEmployeeId.ToString())
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(salariatId))
                throw new InvalidOperationException($"No REGES salariat GUID found for PersonId={personId}");

            // 4) assign referintaSalariat
            payload.ReferintaSalariat = new Models.Employee.ReferintaSalariat { Id = salariatId };

            return payload;
        }
        private void ApplyRowColorsByRegesId()
        {
            if (dgvViewSalariati?.Columns == null || dgvViewSalariati.Rows.Count == 0) return;

            // find the personId column
            DataGridViewColumn pidCol = null;
            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    pidCol = col; break;
                }
            }
            if (pidCol == null) return;

            // gentle green / red-ish
            var green = System.Drawing.Color.FromArgb(230, 255, 230);
            var greenSel = System.Drawing.Color.FromArgb(210, 240, 210);
            var red = System.Drawing.Color.FromArgb(255, 235, 235);
            var redSel = System.Drawing.Color.FromArgb(240, 210, 210);

            foreach (DataGridViewRow row in dgvViewSalariati.Rows)
            {
                if (row.IsNewRow) continue;

                var raw = row.Cells[pidCol.Index].Value?.ToString();
                if (int.TryParse(raw, out var pid))
                {
                    bool hasId = _personsWithRegesId.Contains(pid);
                    row.DefaultCellStyle.BackColor = hasId ? green : red;
                    row.DefaultCellStyle.SelectionBackColor = hasId ? greenSel : redSel;
                }
            }
        }

        protected async override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                await LoadEmployeesAsync();
            }
        }

        private void BuildBarsAndLayout()
        {
            SuspendLayout();


            _topBar = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(8, 8, 8, 4)
            };

            var topFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true, // allow multiple rows
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Title on its own line
            lblTitle = new Label
            {
                Text = "Corectare date salariati",
                AutoSize = true,
                Margin = new Padding(5, 0, 10, 6)
            };
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold);
            topFlow.Controls.Add(lblTitle);
            topFlow.SetFlowBreak(lblTitle, true); // force next controls on a new row

            // Second row: Select-all + counter
            _chkSelectAll = new CheckBox
            {
                Text = "Selecteaza toti salariatii",
                AutoSize = true,
                Margin = new Padding(0, 0, 16, 0)
            };
            _lblCount = new Label
            {
                Text = "Total: 0 | Salariati selectati: 0",
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 0)
            };

            topFlow.Controls.Add(_chkSelectAll);
            topFlow.Controls.Add(_lblCount);
            _topBar.Controls.Add(topFlow);

            // ========= BOTTOM BAR (Send button under the grid) =========
            _bottomBar = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                Padding = new Padding(8)
            };

            var bottomFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            _btnSendSelected = new Button
            {
                Text = "Trimitere data salariati",
                AutoSize = true,
                Margin = new Padding(8)
            };
            bottomFlow.Controls.Add(_btnSendSelected);
            _bottomBar.Controls.Add(bottomFlow);

            // ========= GRID =========
            if (dgvViewSalariati == null)
                dgvViewSalariati = new DataGridView();

            ////search 

            var lblSearch = new Label
            {
                Text = "Căutare:",
                AutoSize = true,
                Margin = new Padding(24, 3, 6, 0)
            };
            _txtSearch = new TextBox
            {
                Width = 220,
                Margin = new Padding(0, 0, 6, 0)
            };
            _btnClearSearch = new Button
            {
                Text = "X",
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0)
            };

            topFlow.Controls.Add(lblSearch);
            topFlow.Controls.Add(_txtSearch);
            topFlow.Controls.Add(_btnClearSearch);

            // events
            _txtSearch.TextChanged += (_, __) => ApplySearchFilter();
            _btnClearSearch.Click += (_, __) => { _txtSearch.Clear(); _txtSearch.Focus(); };


            // color after data binds
            ApplyRowColorsByRegesId();

            dgvViewSalariati.Dock = DockStyle.Fill;
            dgvViewSalariati.AutoGenerateColumns = true;
            dgvViewSalariati.ReadOnly = false;                 // needed for checkbox column
            dgvViewSalariati.AllowUserToAddRows = false;
            dgvViewSalariati.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvViewSalariati.MultiSelect = true;
            dgvViewSalariati.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            // ========= ADD CONTROLS IN THE RIGHT ORDER =========
            Controls.Clear();
            Controls.Add(dgvViewSalariati); // Fill
            Controls.Add(_bottomBar);       // Bottom
            Controls.Add(_topBar);          // Top

            // ========= EVENTS =========
            _chkSelectAll.CheckedChanged += (_, __) => ToggleSelectAll(_chkSelectAll.Checked);
            _btnSendSelected.Click += async (_, __) => await SendSelectedAsync();

            ResumeLayout();
        }

        private async Task SaveSyncReceiptAsync(int personId, SyncResponse sync)
        {
            Debug.WriteLine($"Saving sync receipt for personId={personId}, messageId={sync?.header?.messageId}, receiptId={sync?.responseId}");
            Debug.WriteLine(_session.UserId);
            var rec = new RegesSyncOpenrationsEmployee
            {
                OperationId = OP_CORECTARE_SALARIAT,
                PersonId = personId,
                UserId = int.Parse(_session.UserId),
                MessageResponseId = Guid.TryParse(sync.header.messageId, out var varr) ? varr : (Guid?)null,
                MessageResultId = Guid.TryParse(sync.responseId, out var rid) ? rid : (Guid?)null,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Add(rec);
            await _db.SaveChangesAsync();
        }

        private List<int> GetSelectedPersonIds()
        {
            DataGridViewColumn? pidCol = null;
            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    pidCol = col; break;
                }
            }
            if (pidCol == null) return new List<int>();

            var ids = new List<int>();
            foreach (DataGridViewRow row in dgvViewSalariati.Rows)
            {
                if (row.IsNewRow) continue;
                bool isSel = Convert.ToBoolean(row.Cells[SelectColName].Value ?? false);
                if (!isSel) continue;

                var val = row.Cells[pidCol.Index].Value;
                if (val == null || val == DBNull.Value) continue;
                if (int.TryParse(val.ToString(), out var id))
                    ids.Add(id);
            }
            return ids;
        }


        private async Task SendSelectedAsync()
        {
            var ids = GetSelectedPersonIds();
            if (ids.Count == 0)
            {
                MessageBox.Show("Select at least one row.");
                return;
            }

            int ok = 0, fail = 0;

            // collect failures to show at the end in a window
            var failures = new List<(int personId, string name, string cnp, string error)>();

            foreach (var personId in ids)
            {
                try
                {
                    //   VALIDARE câmpuri obligatorii — dacă lipsesc, NU trimitem JSON
                    int missing = 0; //punem  ca nu lipsesc campuri deocamdata
                    if (missing > 0)
                    {
                        fail++;

                        var pmeta = await _db.People
                            .Where(x => x.PersonId == personId)
                            .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                            .FirstOrDefaultAsync();

                        var errMsg = "Lipsesc câmpuri obligatorii: " + string.Join(", ", missing);
                        Debug.WriteLine($"[REGES INVALID] personId={personId} | name={pmeta?.LastName} {pmeta?.FirstName} | cnp={pmeta?.NationalId} | {errMsg}");

                        // mesaj de warning imediat
                        MessageBox.Show(errMsg, "Date lipsă", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // adaug și în lista de erori pentru sumarul final
                        failures.Add((
                            personId,
                            $"{pmeta?.LastName} {pmeta?.FirstName}",
                            pmeta?.NationalId,
                            errMsg
                        ));

                        continue; // ⛔ skip trimiterea pentru această persoană
                    }

                    // 1) Send to API
                    var sync = await SendOneAsync(personId);

                    // 2) Save sync receipt (same row we’ll later update)
                    await SaveSyncReceiptAsync(personId, sync);

                    // 3) Poll + update that same row (sets Status, ErrorMessage, RegesEmployeeId)
                    await PollForResultAndUpdateAsync(sync.responseId, CancellationToken.None);

                    // 4) Read back the row for this responseId and log result
                    if (Guid.TryParse(sync.responseId, out var rid))
                    {
                        var rec = await _db.Set<RegesSyncOpenrationsEmployee>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.MessageResultId.HasValue && x.MessageResultId.Value == rid);

                        var p = await _db.People
                            .Where(x => x.PersonId == personId)
                            .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                            .FirstOrDefaultAsync();

                        if (rec != null && string.Equals(rec.Status, "Success", StringComparison.OrdinalIgnoreCase) && rec.RegesEmployeeId.HasValue)
                        {
                            ok++;
                            await _db.People
                                    .Where(x => x.PersonId == personId)
                                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.RegesSyncVariable, 0));

                            Debug.WriteLine($"[REGES OK] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | salariatId={rec.RegesEmployeeId}");
                        }
                        else
                        {
                            fail++;
                            var errMsg = rec?.ErrorMessage ?? "Unknown error";
                            Debug.WriteLine($"[REGES FAIL] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | error={errMsg}");

                            failures.Add((
                                personId,
                                $"{p?.LastName ?? ""} {p?.FirstName ?? ""}".Trim(),
                                p?.NationalId ?? "",
                                errMsg ?? ""
                            ));

                        }
                    }
                    else
                    {
                        fail++;

                        var p = await _db.People
                            .Where(x => x.PersonId == personId)
                            .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                            .FirstOrDefaultAsync();

                        var errMsg = $"Invalid responseId '{sync.responseId}'";
                        Debug.WriteLine($"[REGES FAIL] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | error={errMsg}");

                        failures.Add((
                            personId,
                            $"{p?.LastName} {p?.FirstName}",
                            p?.NationalId,
                            errMsg
                        ));
                    }
                }
                catch (Exception ex)
                {
                    fail++;

                    var p = await _db.People
                        .Where(x => x.PersonId == personId)
                        .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                        .FirstOrDefaultAsync();

                    Debug.WriteLine($"[REGES EXCEPTION] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | ex={ex.Message}");

                    failures.Add((
                        personId,
                        $"{p?.LastName} {p?.FirstName}",
                        p?.NationalId,
                        ex.Message
                    ));
                    // keep going
                }
            }

            // final summary in Output window AND small toast
            Debug.WriteLine($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");
            MessageBox.Show($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");

            // if there are failures, show a detailed window with reasons
            if (failures.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("DETALII ERORI TRIMITERE REGES");
                sb.AppendLine(new string('─', 80));
                foreach (var f in failures)
                {
                    sb.AppendLine($"personId={f.personId} | nume={f.name} | CNP={f.cnp}");
                    sb.AppendLine($"  → eroare: {f.error}");
                    sb.AppendLine();
                }

                using (var dlg = new Form
                {
                    Text = $"REGES – Fail details ({failures.Count})",
                    Width = 950,
                    Height = 550,
                    StartPosition = FormStartPosition.CenterParent
                })
                {
                    var tb = new TextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        Dock = DockStyle.Fill,
                        ScrollBars = ScrollBars.Both,
                        WordWrap = false,
                        Font = new Font("Consolas", 10f),
                        Text = sb.ToString()
                    };

                    var copyBtn = new Button
                    {
                        Text = "Copy all",
                        Dock = DockStyle.Bottom,
                        Height = 36
                    };
                    copyBtn.Click += (s, e) =>
                    {
                        try { Clipboard.SetText(tb.Text); } catch { /* ignore */ }
                    };

                    dlg.Controls.Add(tb);
                    dlg.Controls.Add(copyBtn);
                    dlg.ShowDialog(this.FindForm());
                }
            }

            // 🔄 reload + UX refresh (păstrat ca înainte)
            await LoadEmployeesAsync();
            RenumberRows();
            UpdateCounts();
        }

        private async Task<SyncResponse> SendOneAsync(int personId)
        {
            var payload = await BuildPayloadForPerson(personId);
            var json = RegesJson.SanitizeAndSerialize(payload);

            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            Debug.WriteLine($"[SEND] POST {http.BaseAddress}{InregistrareSalariatUrl}");
            Debug.WriteLine($"[SEND] Body={json}");

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PostAsync(InregistrareSalariatUrl, content);
            var body = await resp.Content.ReadAsStringAsync();
            Debug.WriteLine($"[SEND] Status={(int)resp.StatusCode} Body={body}");

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {body}");

            var sync = JsonSerializer.Deserialize<SyncResponse>(body, _jsonOpts)
                       ?? throw new InvalidOperationException("Empty sync response.");

            Debug.WriteLine($"[SYNC OK] personId={personId}, responseId={sync.responseId}, messageId={sync.header?.messageId}");

            return sync;
        }

        private string GetAccessToken()
        {
            if (SessionState.Tokens == null || string.IsNullOrEmpty(SessionState.Tokens.AccessToken))
                throw new InvalidOperationException("Access token not available. Please log in first.");

            // optional: check if token is expired
            if (SessionState.Tokens.Expiration <= DateTime.UtcNow)
                throw new InvalidOperationException("Access token has expired. Please refresh or log in again.");

            return SessionState.Tokens.AccessToken;
        }


        private async Task UpdateIdsRegesRowAsync(
    string responseId,
    string codeType,
    string code,
    string description,
    string regesRefStr,
    string operation,
    string authorIdStr)
        {
            try
            {
                if (!Guid.TryParse(responseId, out var rid))
                {
                    Debug.WriteLine($"[DB] Invalid responseId (not GUID): {responseId}");
                    return;
                }

                var rec = await _db.Set<RegesSyncOpenrationsEmployee>()
                    .FirstOrDefaultAsync(x => x.MessageResultId.HasValue && x.MessageResultId.Value == rid);

                if (rec == null)
                {
                    Debug.WriteLine($"[DB] No RegesSyncModificationEmployee row for responseId={responseId}");
                    return;
                }

                // status + error
                var isError = string.Equals(codeType, "ERROR", StringComparison.OrdinalIgnoreCase);
                rec.Status = isError ? "Error" : "Success";
                rec.ErrorMessage = isError ? description : null;

                // ✅ Save REGES salariat ID into the SAME row
                if (!string.IsNullOrWhiteSpace(regesRefStr) && Guid.TryParse(regesRefStr, out var regesId))
                    rec.RegesEmployeeId = regesId;

                // optional: keep author in sync
                if (!string.IsNullOrWhiteSpace(authorIdStr) && Guid.TryParse(authorIdStr, out var aid))
                    rec.AuthorId = aid;

                rec.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                Debug.WriteLine($"[DB] Updated RegesSyncModificationEmployee: status={rec.Status}, RegesEmployeeId={rec.RegesEmployeeId}, code={code}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[DB] Save error: " + ex);
                throw;
            }
        }

        private async Task PollForResultAndUpdateAsync(string expectedReceiptId, CancellationToken ct)
        {
            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            var url = WithConsumer(PollMessageUrl);
            Debug.WriteLine($"[POLL] Using {http.BaseAddress}{url}");
            Debug.WriteLine($"[POLL] Expecting responseId={expectedReceiptId}");

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var resp = await http.PostAsync(url, content: null, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[POLL] Status={(int)resp.StatusCode} Body={body}");

                if (resp.StatusCode == System.Net.HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(body))
                {
                    await Task.Delay(1000, ct);
                    continue; // queue empty
                }
                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Poll HTTP {(int)resp.StatusCode}: {body}");

                string responseId = null;
                string codeType = null, code = null, description = null, operation = null, authorIdStr = null;
                string regesSalariatId = null;

                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    // must match our recipisă
                    if (root.TryGetProperty("responseId", out var ridEl) && ridEl.ValueKind == JsonValueKind.String)
                        responseId = ridEl.GetString();

                    if (!string.Equals(responseId, expectedReceiptId, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("[POLL] Different responseId; continue polling...");
                        await Task.Delay(600, ct);
                        continue;
                    }

                    // result (status + possible ref)
                    if (root.TryGetProperty("result", out var res) && res.ValueKind == JsonValueKind.Object)
                    {
                        if (res.TryGetProperty("codeType", out var v) && v.ValueKind == JsonValueKind.String) codeType = v.GetString();
                        if (res.TryGetProperty("code", out v) && v.ValueKind == JsonValueKind.String) code = v.GetString();
                        if (res.TryGetProperty("description", out v) && v.ValueKind == JsonValueKind.String) description = v.GetString();

                        // primary: result.ref (most common place for salariat id)
                        if (res.TryGetProperty("ref", out v) && v.ValueKind == JsonValueKind.String)
                            regesSalariatId = v.GetString();
                    }

                    // header bits (optional)
                    if (root.TryGetProperty("header", out var header) && header.ValueKind == JsonValueKind.Object)
                    {
                        if (header.TryGetProperty("operation", out var v) && v.ValueKind == JsonValueKind.String) operation = v.GetString();
                        if (header.TryGetProperty("authorId", out v) && v.ValueKind == JsonValueKind.String) authorIdStr = v.GetString();
                    }

                    // fallback: try ReferintaSalariat.Id or any "salariat{... Id: ...}"
                    if (string.IsNullOrWhiteSpace(regesSalariatId))
                        regesSalariatId = TryExtractSalariatIdFallback(root);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[POLL] Parse error: " + ex);
                    await Task.Delay(800, ct);
                    continue;
                }

                // simple console log when failure OR missing salariat id
                if (string.Equals(codeType, "ERROR", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(regesSalariatId))
                {
                    try
                    {
                        int? personId = null;
                        string ln = null, fn = null, cnp = null;

                        if (Guid.TryParse(responseId, out var rid))
                        {
                            var rec = await _db.Set<RegesSyncOpenrationsEmployee>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.MessageResultId.HasValue && x.MessageResultId.Value == rid, ct);

                            if (rec != null)
                            {
                                personId = rec.PersonId;
                                var p = await _db.People.AsNoTracking()
                                    .Where(x => x.PersonId == rec.PersonId)
                                    .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                                    .FirstOrDefaultAsync(ct);

                                ln = p?.LastName; fn = p?.FirstName; cnp = p?.NationalId;
                            }
                        }
                        MessageBox.Show($"[REGES FAIL] resp={responseId} | personId={(personId?.ToString() ?? "?")} | name={(ln ?? "?")} {(fn ?? "")} | cnp={(cnp ?? "?")} | codeType={(codeType ?? "-")} | code={(code ?? "-")} | desc={(description ?? "-")}");

                    }
                    catch { /* best-effort logging only */ }
                }

                // update same RegesSyncModificationEmployee row (saves RegesEmployeeId if present)
                await UpdateIdsRegesRowAsync(responseId, codeType, code, description, regesSalariatId, operation, authorIdStr);
                return; // done for this receipt
            }


            // ---- local fallback extractor (no new DTOs) ----
            static string TryExtractSalariatIdFallback(JsonElement root)
            {
                string found = null;

                void TryPickId(JsonElement obj, ref string target)
                {
                    foreach (var p in obj.EnumerateObject())
                    {
                        if ((p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) &&
                            p.Value.ValueKind == JsonValueKind.String)
                        {
                            var s = p.Value.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) { target = s; return; }
                        }
                    }
                }

                void Scan(JsonElement el)
                {
                    if (found != null) return;
                    switch (el.ValueKind)
                    {
                        case JsonValueKind.Object:
                            foreach (var prop in el.EnumerateObject())
                            {
                                var lname = prop.Name.ToLowerInvariant();

                                if (lname.Contains("referinta") && prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var rp in prop.Value.EnumerateObject())
                                        if (rp.Name.ToLowerInvariant().Contains("salariat") && rp.Value.ValueKind == JsonValueKind.Object)
                                            TryPickId(rp.Value, ref found);
                                }
                                if (found != null) break;

                                if (lname.Contains("salariat") && prop.Value.ValueKind == JsonValueKind.Object)
                                    TryPickId(prop.Value, ref found);
                                if (found != null) break;

                                Scan(prop.Value);
                                if (found != null) break;
                            }
                            break;

                        case JsonValueKind.Array:
                            foreach (var item in el.EnumerateArray())
                            {
                                Scan(item);
                                if (found != null) break;
                            }
                            break;
                    }
                }

                try { Scan(root); } catch { }
                return found;
            }
        }

        private void EnsureSpecialColumns()
        {
            // 1) Select checkbox (first column)
            if (!dgvViewSalariati.Columns.Contains(SelectColName))
            {
                var chk = new DataGridViewCheckBoxColumn
                {
                    Name = SelectColName,
                    HeaderText = "",
                    Width = 30,
                    ReadOnly = false,
                    TrueValue = true,
                    FalseValue = false
                };
                dgvViewSalariati.Columns.Insert(0, chk);
            }

            // 2) Row number (second column)
            if (!dgvViewSalariati.Columns.Contains(RowNoColName))
            {
                var no = new DataGridViewTextBoxColumn
                {
                    Name = RowNoColName,
                    HeaderText = "No",
                    ReadOnly = true,
                    Width = 50
                };
                dgvViewSalariati.Columns.Insert(1, no);
            }

            // 3) PersonId column (auto-generated from projection new { personId = p.PersonId, ... })
            //    In DEBUG we show it and put it right after the row number; in Release it stays hidden.
            DataGridViewColumn? pidCol = null;

            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    pidCol = col;
                    break;
                }
            }

            // If somehow AutoGenerateColumns missed it, create a bound debug column
            if (pidCol == null && DEBUG_SHOW_PERSON_ID)
            {
                pidCol = new DataGridViewTextBoxColumn
                {
                    Name = "PersonIdDebug",
                    DataPropertyName = PersonIdPropertyName, // binds to anonymous type property "personId"
                    HeaderText = "PersonId",
                    ReadOnly = true,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
                };
                dgvViewSalariati.Columns.Insert(2, pidCol);
            }

            if (pidCol != null)
            {
                pidCol.Visible = DEBUG_SHOW_PERSON_ID;
                if (DEBUG_SHOW_PERSON_ID)
                {
                    pidCol.HeaderText = "PersonId";
                    pidCol.ReadOnly = true;
                    pidCol.DisplayIndex = 2; // after Select + No
                }
            }

            // keep special columns in front
            dgvViewSalariati.Columns[SelectColName].DisplayIndex = 0;
            dgvViewSalariati.Columns[RowNoColName].DisplayIndex = 1;

            EnableProgrammaticSortOnDataColumns();   // arrows in header we control

        }
        private void EnableProgrammaticSortOnDataColumns()
        {
            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (col == null) continue;
                if (col.Name == SelectColName || col.Name == RowNoColName) continue;
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
        }
        private void ApplySearchFilter()
        {
            if (_allRowsData == null || _rowItemType == null) return;

            var q = _txtSearch.Text?.Trim();
            IEnumerable<object> items = ((IEnumerable)_allRowsData).Cast<object>();

            if (!string.IsNullOrEmpty(q))
            {
                var props = _rowItemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                items = items.Where(o =>
                {
                    foreach (var p in props)
                    {
                        var v = p.GetValue(o, null);
                        if (v == null) continue;

                        string s = v is DateTime dt ? dt.ToString("yyyy-MM-dd") : v.ToString();
                        if (!string.IsNullOrEmpty(s) && s.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                });
            }

            // keep current sort if any
            if (!string.IsNullOrWhiteSpace(_lastSortProp))
            {
                var pi = _rowItemType.GetProperty(_lastSortProp, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi != null)
                {
                    items = _lastSortDir == ListSortDirection.Descending
                        ? items.OrderByDescending(o => pi.GetValue(o, null), _nullsLast)
                        : items.OrderBy(o => pi.GetValue(o, null), _nullsLast);
                }
            }

            // Cast<T> + ToList<T> for anonymous type
            var castM = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast))!.MakeGenericMethod(_rowItemType);
            var toList = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList))!.MakeGenericMethod(_rowItemType);
            var casted = castM.Invoke(null, new object[] { items });
            var list = toList.Invoke(null, new object[] { casted });

            dgvViewSalariati.DataSource = list;
            _rowsData = list;

            RenumberRows();
            UpdateCounts();
            ApplyRowColorsByRegesId();

            foreach (DataGridViewColumn c in dgvViewSalariati.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            if (!string.IsNullOrWhiteSpace(_lastSortProp))
            {
                var sortedCol = dgvViewSalariati.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c =>
                        string.Equals(c.DataPropertyName, _lastSortProp, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Name, _lastSortProp, StringComparison.OrdinalIgnoreCase));
                if (sortedCol != null)
                    sortedCol.HeaderCell.SortGlyphDirection =
                        _lastSortDir == ListSortDirection.Descending ? SortOrder.Descending : SortOrder.Ascending;
            }
        }

        private void RenumberRows()
        {
            if (!dgvViewSalariati.Columns.Contains(RowNoColName)) return;

            for (int i = 0; i < dgvViewSalariati.Rows.Count; i++)
            {
                var row = dgvViewSalariati.Rows[i];
                if (!row.IsNewRow)
                    row.Cells[RowNoColName].Value = i + 1;
            }
        }

        private void UpdateCounts()
        {
            int total = dgvViewSalariati.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
            int selected = 0;

            if (dgvViewSalariati.Columns.Contains(SelectColName))
            {
                foreach (DataGridViewRow r in dgvViewSalariati.Rows)
                {
                    if (r.IsNewRow) continue;
                    selected += Convert.ToBoolean(r.Cells[SelectColName].Value ?? false) ? 1 : 0;
                }
            }

            _lblCount.Text = $"Total: {total:n0} | Selected: {selected:n0}";
        }

        private void ToggleSelectAll(bool @checked)
        {
            if (!dgvViewSalariati.Columns.Contains(SelectColName)) return;

            foreach (DataGridViewRow row in dgvViewSalariati.Rows)
            {
                if (!row.IsNewRow)
                    row.Cells[SelectColName].Value = @checked;
            }
            dgvViewSalariati.EndEdit();
            UpdateCounts();
        }

        #endregion
        public ControlerCorrectionEmployeeView()
        {
            InitializeComponent();
        }

        private void ControlerCorrectionEmployeeView_Load(object sender, EventArgs e)
        {

        }

        private async Task LoadEmployeesAsync()
        {
            // 1) Raw read + lookups needed for payload-like values
            var raw = await (
                  from p in _db.People
                  where p.Status == "A"  
  && p.RegesSyncVariable == 3 // Correction / Corectie 
  && _db.Set<RegesSync>().AsNoTracking()
       .Any(rs => rs.PersonId == p.PersonId && rs.RegesEmployeeId != null)
  && _db.Set<ContractsRu>().AsNoTracking()
       .Any(cr => cr.PersonId == p.PersonId)


                  // LEFT JOIN contracts, but pre-filter contracts to status=1
                  //              join cr0 in _db.ContractsRu.Where(x =>
                  //   x.ContractStatusId == 1
                  //|| x.ContractStatusId == 2
                  //|| x.ContractStatusId == 3)
                  //                  on p.PersonId equals cr0.PersonId into pc
                  //from cr in pc.DefaultIfEmpty()

                  // keep only rows that actually matched an active contract
                  //where cr != null


                  from c in _db.Countries
                     .Where(x => x.CountryId == p.DomicileCountryId)
                     .DefaultIfEmpty()

                  from a in _db.IdentityDocumentTypes
                       .Where(x => x.IdentityDocumentTypeId == p.IdentityDocTypeId)
                       .DefaultIfEmpty()

                  from ap in _db.TypePapartide
                       .Where(x => x.IdTypePapartid == p.IdTipApatrid)
                       .DefaultIfEmpty()

                  from th in _db.DisabilityTypes
                       .Where(x => x.DisabilityTypeId == p.HandicapTypeId)
                       .DefaultIfEmpty()

                  from gh in _db.DisabilityGrades
                       .Where(x => x.DisabilityGradeId == p.DisabilityGradeId)
                       .DefaultIfEmpty()

                  from wpt in _db.WorkPermitTypes
                       .Where(x => x.WorkPermitId == p.WorkPermitTypeId)
                       .DefaultIfEmpty()

                  orderby p.PersonId
                  select new
                  {
                      p.PersonId,
                      p.SirutaCode,
                      p.Address,
                      p.NationalId,
                      p.LastName,
                      p.FirstName,
                      p.BirthDate,

                      // pentru fallback-uri (aceleași ca în payload)
                      DomicileCountryRevisal = c.CountryNameRevisal,
                      CountryName = c.CountryName, // if ever needed
                      IdentityDocumentName = a.IdentityDocumentName,
                      IdentityDocumentCode = a.IdentityDocumentCode,
                      p.IdentityDocTypeId,

                      ApatridName = ap.PaPartidName,

                      HandicapTypeName = th.DisabilityTypeName,
                      HandicapTypeId = p.HandicapTypeId,
                      DisabilityGradeName = gh.DisabilityGradeName,
                      DisabilityGradeId = p.DisabilityGradeId,

                      p.HandicapCertificateDate,
                      p.HandicapCertificateNumber,
                      p.DisabilityReviewDate,

                      p.InvalidityGradeId, // for GradInvaliditate

                      p.WorkPermitStartDate,
                      p.WorkPermitEndDate,
                      WorkPermitTypeName = wpt.WorkPermitName,
                      p.ApprovalNumber
                  }
            )
            .AsNoTracking()
            .ToListAsync();

            // 2) Proiecția finală pentru DGV (oglindă a câmpurilor din JSON, plus debug)
            var rows = raw.Select(r =>
            {
                // “passport” ca în payload


                // fallback-uri identice cu payload
                var taraDomiciliu = RegesJson.FixText(r.DomicileCountryRevisal) ?? "";
                var nationalitate = RegesJson.Norma(taraDomiciliu) ?? taraDomiciliu;

                // în payload: TipActIdentitate = IdentityDocumentCode
                var tipActCode = r.IdentityDocumentCode ?? r.IdentityDocumentName ?? "";

                // în payload: GradInvaliditate -> "GradX" dacă există (same quick rule you used)
                string gradInvaliditate = null;


                return new
                {
                    // === DEBUG + key (ca în DGV) ===
                    personId = r.PersonId,

                    // === Info.* (nume/valori ca în JSON) ===
                    CodSiruta = r.SirutaCode,                                   // info.localitate.codSiruta (flatten leaf)
                    Adresa = r.Address,                                         // info.adresa
                    Cnp = r.NationalId,                                         // info.cnp
                    Nume = r.LastName,                                          // info.nume
                    Prenume = r.FirstName,                                      // info.prenume
                    DataNastere = r.BirthDate,                                  // info.dataNastere min ocurance 0

                    // NamedEntity -> afișăm .Nume
                    Nationalitate = nationalitate,                              // info.nationalitate.nume
                    TaraDomiciliu = taraDomiciliu,                              // info.taraDomiciliu.nume

                    TipActIdentitate = tipActCode,                              // info.tipActIdentitate

                    // handicap – numai dacă se încadrează
                    TipHandicap = r.HandicapTypeName,              // info.tipHandicap
                    GradHandicap = r.DisabilityGradeName,          // info.gradHandicap
                    DataCertificatHandicap = r.HandicapCertificateDate, // info.dataCertificatHandicap
                    NumarCertificatHandicap = r.HandicapCertificateNumber, // info.numarCertificatHandicap
                    DataValabilitateCertificatHandicap = r.DisabilityReviewDate, // info.dataValabilitateCertificatHandicap
                    GradInvaliditate = gradInvaliditate,           // info.gradInvaliditate (same heuristic)

                    // câmpuri simple din payload (populate ca în BuildPayload)
                    Mentiuni = "",                                                         // info.mentiuni
                    MotivRadiere = "",                                                     // info.motivRadiere

                    // detaliiSalariatStrain – doar la pașaport (ca în payload)
                    DetaliiSalariatStrain_DataInceputAutorizatie = r.WorkPermitStartDate,
                    DetaliiSalariatStrain_DataSfarsitAutorizatie = r.WorkPermitEndDate,
                    DetaliiSalariatStrain_TipAutorizatie = r.WorkPermitTypeName,
                    DetaliiSalariatStrain_TipAutorizatieExceptie = r.WorkPermitTypeName,
                    DetaliiSalariatStrain_NumarAutorizatie = r.ApprovalNumber
                };
            })
            .ToList();

            // 3) setul de persoane care au deja RegesEmployeeId (pentru colorare)
            var withRegesNullable = await _db.Set<RegesSync>()
                .Where(r => r.RegesEmployeeId.HasValue)
                .Select(r => r.PersonId)
                .Distinct()
                .ToListAsync();

            _personsWithRegesId = new HashSet<int>(
                withRegesNullable.Where(pid => pid.HasValue).Select(pid => pid.Value)
            );

            // 4) bind + color
            dgvViewSalariati.AutoGenerateColumns = true;
            dgvViewSalariati.DataSource = rows;

            _rowsData = dgvViewSalariati.DataSource;                    // List<anon>
            _rowItemType = (rows.Count > 0) ? rows[0].GetType() : null; // remember anon type

            // //search
            dgvViewSalariati.DataSource = rows;

            _allRowsData = rows;                         // full list for filtering
            _rowsData = dgvViewSalariati.DataSource;     // current (possibly filtered/sorted)
            _rowItemType = (rows.Count > 0) ? rows[0].GetType() : null;

            //  after DataSource
            //ApplyRowColorsByRegesId();
        }

    }
}
