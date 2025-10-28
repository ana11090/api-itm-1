using api_itm.Data.Entity.Ru.Contracts.ContractsSuspended;
using api_itm.Data.Entity.Ru.Reges;
using api_itm.Infrastructure;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using api_itm.Models.Contracts;
using api_itm.Models.Contracts.Envelope;
using api_itm.Models.Contracts.SuspendariEnvelope;
using api_itm.Models.Reges;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static api_itm.Program;

namespace api_itm.UserControler.Contracts.Suspended
{
    public partial class ControlerModificationSuspendedContractsView : UserControl
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private DataGridView dgvContracteSuspendate;
        private Label lblTitle;

        private CheckBox _chkSelectAll;
        private Label _lblCount;
        private Button _btnSendSelected;
        private Panel _topBar;
        private Panel _bottomBar;

        private ISessionContext _session;

        private const string SelectColName = "Selectat";
        private const string RowNoColName = "No";
        private const string PersonIdPropertyName = "personId";

        // ===================== REGES API SETTINGS (keep these in config in real app) =====================
        private const string BaseUrl = "https://api.dev.inspectiamuncii.org/";   // <- set your actual API host
        private const string InregistrareContractUrl = "api/Contract";

        private const string ContractIdPropertyName = "idContract";
        private HashSet<int> _contractsWithRegesId = new HashSet<int>();

#if DEBUG
        private CheckBox _chkDebugValidate;
        private bool _debugValidate = true; // default ON in Debug
#endif



        private string WithConsumer(string path)
            => string.IsNullOrWhiteSpace(ConsumerId) ? path : $"{path}?consumerId={Uri.EscapeDataString(ConsumerId)}";

        // // search
        // UI
        private TextBox _txtSearch;
        private Button _btnClearSearch;

        // data cache
        private object _allRowsData;    // original full List<anon>

        private const string ConsumerId = "winforms-dev-1"; // sau string.Empty pentru implicit

        private const string PollMessageUrl = "api/Status/PollMessage";

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

#if DEBUG
        private const bool DEBUG_SHOW_PERSON_ID = true;
#else
        private const bool DEBUG_SHOW_PERSON_ID = false;
#endif

        public ControlerModificationSuspendedContractsView(IDbContextFactory<AppDbContext> db, ISessionContext session)
        {
            InitializeComponent();
            _dbFactory = db;

            _session = session;
            BuildBarsAndLayout();
            WireGrid();
        }
        private void WireGrid()
        {
            dgvContracteSuspendate.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvContracteSuspendate.IsCurrentCellDirty)
                    dgvContracteSuspendate.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvContracteSuspendate.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvContracteSuspendate.Columns[e.ColumnIndex].Name == SelectColName)
                    UpdateCounts();
            };

            dgvContracteSuspendate.DataBindingComplete += (_, __) =>
            {
                EnsureSpecialColumns();
                RenumberRows();
                UpdateCounts();
                //ApplyRowColorsByRegesId();
            };

            dgvContracteSuspendate.Sorted += (_, __) => RenumberRows();
            dgvContracteSuspendate.RowsAdded += (_, __) => { RenumberRows(); UpdateCounts(); };
            dgvContracteSuspendate.RowsRemoved += (_, __) => { RenumberRows(); UpdateCounts(); };

            dgvContracteSuspendate.CellDoubleClick += async (_, __) => await PreviewSelectedRowJsonAsync();
            dgvContracteSuspendate.ColumnHeaderMouseClick += (_, e) => OnHeaderClick(e.ColumnIndex);

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

            dgvContracteSuspendate.DataSource = sortedListObj;
            _rowsData = sortedListObj; // noua ordine

            // păstrăm UX-ul existent
            RenumberRows();
            UpdateCounts();
            //ApplyRowColorsByRegesId();
        }


        private void OnHeaderClick(int colIndex)
        {
            if (colIndex < 0 || colIndex >= dgvContracteSuspendate.Columns.Count) return;

            var clickedCol = dgvContracteSuspendate.Columns[colIndex];
            if (clickedCol == null) return;
            if (clickedCol.Name == SelectColName || clickedCol.Name == RowNoColName) return;

            var prop = string.IsNullOrWhiteSpace(clickedCol.DataPropertyName) ? clickedCol.Name : clickedCol.DataPropertyName;
            if (string.IsNullOrWhiteSpace(prop)) return;

            var wantDesc = (_lastSortProp == prop) ? (_lastSortDir == ListSortDirection.Ascending) : true;

            // rebind (this recreates columns)
            ApplySort(prop, wantDesc);

            // after rebinding, work with the NEW column instances
            foreach (DataGridViewColumn c in dgvContracteSuspendate.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            var newCol = dgvContracteSuspendate.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.DataPropertyName, prop, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, prop, StringComparison.OrdinalIgnoreCase));

            if (newCol != null)
                newCol.HeaderCell.SortGlyphDirection = wantDesc ? SortOrder.Descending : SortOrder.Ascending;

            _lastSortProp = prop;
            _lastSortDir = wantDesc ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }

        private int? GetIdContractFromRow(DataGridViewRow row)
        {
            if (row == null) return null;

            // 1) Preferred: read from bound object (anonymous type has "idContract")
            var item = row.DataBoundItem;
            if (item != null)
            {
                var prop = item.GetType().GetProperty(
                    "idContract",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null)
                    return CoerceToInt(prop.GetValue(item, null));
            }

            // 2) Fallback: read from cell whose DataPropertyName == "idContract"
            foreach (DataGridViewColumn col in dgvContracteSuspendate.Columns)
            {
                if (string.Equals(col.DataPropertyName, "idContract", StringComparison.OrdinalIgnoreCase))
                    return CoerceToInt(row.Cells[col.Index].Value);
            }

            return null;
        }


        private static int? CoerceToInt(object value)
        {
            if (value == null) return null;

            // common numeric types
            if (value is int i) return i;
            if (value is long l) return checked((int)l);
            if (value is short s) return s;
            if (value is byte b) return b;
            if (value is decimal dec) return (int)dec;
            if (value is double d) return (int)d;
            if (value is float f) return (int)f;

            // strings
            if (value is string str && int.TryParse(str, out var n)) return n;

            // last resort
            try { return Convert.ToInt32(value); } catch { return null; }
        }

        private async Task PreviewJsonForContract(int idContract)
        {
            var payload = await BuildContractPayloadAsync(idContract);

            // also make sure names from DB are fixed BEFORE mapping (see earlier message)
            var json = RegesJson.SanitizeAndSerialize(payload);

            ShowJsonPreview(json);
        }

        /// <summary>
        /// On row double-click: extracts idContract from the current row and shows JSON preview.
        /// </summary>
        private async Task PreviewSelectedRowJsonAsync()
        {
            if (dgvContracteSuspendate.CurrentRow == null) return;

            int? idContract = GetIdContractFromRow(dgvContracteSuspendate.CurrentRow);

#if DEBUG
            Debug.WriteLine($"[DEBUG] Selected row idContract={idContract}");
#endif

            if (idContract == null)
            {
                MessageBox.Show("Cannot detect IdContract for the selected row.");
                return;
            }

            await PreviewJsonForContract(idContract.Value);
        }


        private void RenumberRows()
        {
            if (!dgvContracteSuspendate.Columns.Contains(RowNoColName)) return;

            for (int i = 0; i < dgvContracteSuspendate.Rows.Count; i++)
            {
                var row = dgvContracteSuspendate.Rows[i];
                if (!row.IsNewRow)
                    row.Cells[RowNoColName].Value = i + 1;
            }
        }

        private void UpdateCounts()
        {
            int total = dgvContracteSuspendate.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
            int selected = 0;

            if (dgvContracteSuspendate.Columns.Contains(SelectColName))
            {
                foreach (DataGridViewRow r in dgvContracteSuspendate.Rows)
                {
                    if (r.IsNewRow) continue;
                    selected += Convert.ToBoolean(r.Cells[SelectColName].Value ?? false) ? 1 : 0;
                }
            }

            _lblCount.Text = $"Total: {total:n0} | Selected: {selected:n0}";
        }

        private void EnsureSpecialColumns()
        {
            // 1) Select checkbox (first column)
            if (!dgvContracteSuspendate.Columns.Contains(SelectColName))
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
                dgvContracteSuspendate.Columns.Insert(0, chk);
            }

            // 2) Row number (second column)
            if (!dgvContracteSuspendate.Columns.Contains(RowNoColName))
            {
                var no = new DataGridViewTextBoxColumn
                {
                    Name = RowNoColName,
                    HeaderText = "No",
                    ReadOnly = true,
                    Width = 50
                };
                dgvContracteSuspendate.Columns.Insert(1, no);
            }

            // 3) PersonId column (auto-generated from projection new { personId = p.PersonId, ... })
            //    In DEBUG we show it and put it right after the row number; in Release it stays hidden.
            DataGridViewColumn? pidCol = null;

            foreach (DataGridViewColumn col in dgvContracteSuspendate.Columns)
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
                dgvContracteSuspendate.Columns.Insert(2, pidCol);
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

            EnableProgrammaticSortOnDataColumns();

            // keep special columns in front
            //dgvContracteSuspendate.Columns[SelectColName].DisplayIndex = 0;
            //dgvContracteSuspendate.Columns[RowNoColName].DisplayIndex = 1;
        }

        private void EnableProgrammaticSortOnDataColumns()
        {
            foreach (DataGridViewColumn col in dgvContracteSuspendate.Columns)
            {
                if (col == null) continue;
                if (col.Name == SelectColName || col.Name == RowNoColName) continue;
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
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

        private void ControlerModificationSuspendedContractsView_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Builds the full layout of the control:
        /// - Creates a top bar with a title, a "Select all contracts" checkbox, and a counter label.
        /// - Creates a bottom bar with a "Trimitere contracte" button.
        /// - Configures the DataGridView (dgvContracteSuspendate) for displaying contracts:
        ///     • Auto-generates columns
        ///     • Handles checkboxes, row numbering, counters, and row coloring
        ///     • Supports previewing a row’s JSON on double-click
        /// - Adds all controls in the correct docking order (top, grid, bottom).
        /// </summary> 
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
                Text = "Suspendare contracte",
                AutoSize = true,
                Margin = new Padding(5, 0, 10, 6)
            };
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold);
            topFlow.Controls.Add(lblTitle);
            topFlow.SetFlowBreak(lblTitle, true); // force next controls on a new row

            // Second row: Select-all + counter
            _chkSelectAll = new CheckBox
            {
                Text = "Selecteaza toate contractele",
                AutoSize = true,
                Margin = new Padding(0, 0, 16, 0)
            };
            _lblCount = new Label
            {
                Text = "Total: 0 | Contracte selectate: 0",
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 0)
            };

#if DEBUG
            _chkDebugValidate = new CheckBox
            {
                Text = "Validate before send (DEBUG)",
                Checked = true,
                AutoSize = true,
                Margin = new Padding(16, 0, 0, 0)
            };
            _chkDebugValidate.CheckedChanged += (_, __) => _debugValidate = _chkDebugValidate.Checked;
            topFlow.Controls.Add(_chkDebugValidate);
#endif

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
                Text = "Trimitere contracte suspendate",
                AutoSize = true,
                Margin = new Padding(8)
            };
            bottomFlow.Controls.Add(_btnSendSelected);
            _bottomBar.Controls.Add(bottomFlow);

            // ========= GRID =========
            if (dgvContracteSuspendate == null)
                dgvContracteSuspendate = new DataGridView();

            // events for checkbox behavior + counters + row numbering
            dgvContracteSuspendate.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvContracteSuspendate.IsCurrentCellDirty)
                    dgvContracteSuspendate.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvContracteSuspendate.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvContracteSuspendate.Columns[e.ColumnIndex].Name == SelectColName)
                    UpdateCounts();
            };

            dgvContracteSuspendate.DataBindingComplete += (_, __) =>
            {
                EnsureSpecialColumns();
                RenumberRows();
                UpdateCounts();
                //ApplyRowColorsByRegesId();
            };

            // // search

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


            dgvContracteSuspendate.Dock = DockStyle.Fill;
            dgvContracteSuspendate.AutoGenerateColumns = true;
            dgvContracteSuspendate.ReadOnly = false;                 // needed for checkbox column
            dgvContracteSuspendate.AllowUserToAddRows = false;
            dgvContracteSuspendate.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvContracteSuspendate.MultiSelect = true;
            dgvContracteSuspendate.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            // ========= ADD CONTROLS IN THE RIGHT ORDER =========
            Controls.Clear();
            Controls.Add(dgvContracteSuspendate); // Fill
            Controls.Add(_bottomBar);       // Bottom
            Controls.Add(_topBar);          // Top

            // ========= EVENTS =========
            _chkSelectAll.CheckedChanged += (_, __) => ToggleSelectAll(_chkSelectAll.Checked);
            _btnSendSelected.Click += async (_, __) => await SendSelectedAsync();

            dgvContracteSuspendate.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0) // ignore header double-clicks
                    await PreviewSelectedRowJsonAsync();
            };
            dgvContracteSuspendate.CurrentCellChanged += (_, __) => UpdateCounts();
            ResumeLayout();
        }


        private void ToggleSelectAll(bool @checked)
        {
            if (!dgvContracteSuspendate.Columns.Contains(SelectColName)) return;

            foreach (DataGridViewRow row in dgvContracteSuspendate.Rows)
            {
                if (!row.IsNewRow)
                    row.Cells[SelectColName].Value = @checked;
            }
            dgvContracteSuspendate.EndEdit();
            UpdateCounts();
        }

        private async Task SaveSyncReceiptAsync(int idContract, SyncResponse sync)
        {
            Debug.WriteLine($"[DB] Save sync receipt for idContract={idContract}, messageId={sync?.header?.messageId}, responseId={sync?.responseId}");

            var rec = new RegesSyncModificationContracts
            {
                IdContract = idContract,
                IdUtilizator = int.TryParse(_session.UserId, out var uid) ? uid : (int?)null,
                Id_Raspuns_Mesaj = CoerceGuid(sync?.header?.messageId),
                Id_Rezultat_Mesaj = CoerceGuid(sync?.responseId),
                Status = "Pending",
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };

            await _dbFactory.WithDb(async db =>
            {
                db.Add(rec);
                await db.SaveChangesAsync();
            });
        }


        private async Task PollForResultAndUpdateAsync(string expectedResponseId, CancellationToken ct)
        {
            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            var url = WithConsumer(PollMessageUrl);
            Debug.WriteLine($"[POLL] Using {http.BaseAddress}{url}");
            Debug.WriteLine($"[POLL] Expecting responseId={expectedResponseId}");

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var resp = await http.PostAsync(url, content: null, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[POLL] Status={(int)resp.StatusCode} Body={(string.IsNullOrWhiteSpace(body) ? "<empty>" : body)}");

                if (resp.StatusCode == System.Net.HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(body))
                {
                    await Task.Delay(1000, ct);
                    continue; // queue empty
                }
                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Poll HTTP {(int)resp.StatusCode}: {body}");

                string responseId = null;
                string codeType = null, code = null, description = null, operation = null, authorIdStr = null;
                string regesContractId = null;

                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    // responseId must match our receipt
                    if (root.TryGetProperty("responseId", out var ridEl) && ridEl.ValueKind == JsonValueKind.String)
                        responseId = ridEl.GetString();

                    if (!string.Equals(responseId, expectedResponseId, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine("[POLL] Different responseId; continue polling...");
                        await Task.Delay(600, ct);
                        continue;
                    }

                    // result
                    if (root.TryGetProperty("result", out var res) && res.ValueKind == JsonValueKind.Object)
                    {
                        if (res.TryGetProperty("codeType", out var v) && v.ValueKind == JsonValueKind.String) codeType = v.GetString();
                        if (res.TryGetProperty("code", out v) && v.ValueKind == JsonValueKind.String) code = v.GetString();
                        if (res.TryGetProperty("description", out v) && v.ValueKind == JsonValueKind.String) description = v.GetString();

                        // most APIs return the created id in "ref"
                        if (res.TryGetProperty("ref", out v) && v.ValueKind == JsonValueKind.String)
                            regesContractId = v.GetString();
                    }

                    // header (optional)
                    if (root.TryGetProperty("header", out var header) && header.ValueKind == JsonValueKind.Object)
                    {
                        if (header.TryGetProperty("operation", out var v) && v.ValueKind == JsonValueKind.String) operation = v.GetString();
                        if (header.TryGetProperty("authorId", out v) && v.ValueKind == JsonValueKind.String) authorIdStr = v.GetString();
                    }

                    // fallback: look for any "...Contract... Id" in the payload if "ref" missing
                    if (string.IsNullOrWhiteSpace(regesContractId))
                        regesContractId = TryExtractContractIdFallback(root);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[POLL] Parse error: " + ex);
                    await Task.Delay(800, ct);
                    continue;
                }

                // update DB row (maps to your RegesContractSync fields)
                await UpdateContractSyncRowAsync(responseId, codeType, code, description, regesContractId, operation, authorIdStr);
                return; // done for this receipt
            }
        }

        private async Task UpdateContractSyncRowAsync(
    string responseId,
    string codeType,
    string code,
    string description,
    string regesRefStr,
    string operation,
    string authorIdStr)
        {
            if (!Guid.TryParse(responseId, out var rid))
            {
                Debug.WriteLine($"[DB] Invalid responseId (not GUID): {responseId}");
                return;
            }

            await using var db = await _dbFactory.CreateDbContextAsync();

            var rec = await db.Set<RegesSyncModificationContracts>()
                .FirstOrDefaultAsync(x => x.Id_Rezultat_Mesaj.HasValue && x.Id_Rezultat_Mesaj.Value == rid);

            if (rec == null)
            {
                Debug.WriteLine($"[DB] No RegesSyncModificationContracts row for responseId={responseId}");
                return;
            }

            var isError = string.Equals(codeType, "ERROR", StringComparison.OrdinalIgnoreCase);
            rec.Status = isError ? "Error" : "Success";
            rec.Error_Message = isError ? description : null;

            if (!string.IsNullOrWhiteSpace(regesRefStr) && Guid.TryParse(regesRefStr, out var regesId))
                rec.RegesContractId = regesId;

            if (!string.IsNullOrWhiteSpace(authorIdStr) && Guid.TryParse(authorIdStr, out var aid))
                rec.IdAutor = aid;

            rec.Updated_At = DateTime.UtcNow;
            await db.SaveChangesAsync();

            Debug.WriteLine($"[DB] Updated RegesSyncModificationContracts: status={rec.Status}, RegesContractId={rec.RegesContractId}, code={code}");
        }


        static string TryExtractContractIdFallback(JsonElement root)
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
                                    if (rp.Name.ToLowerInvariant().Contains("contract") && rp.Value.ValueKind == JsonValueKind.Object)
                                        TryPickId(rp.Value, ref found);
                            }
                            if (found != null) break;

                            if (lname.Contains("contract") && prop.Value.ValueKind == JsonValueKind.Object)
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





        private async Task SendSelectedAsync()
        {
            var ids = GetSelectedContractIds();
            if (ids.Count == 0)
            {
                MessageBox.Show("Select at least one row.");
                return;
            }

            var lines = new List<SendLine>(ids.Count);
            int ok = 0, fail = 0;

            foreach (var idContract in ids)
            {
                try
                {
                    var sync = await SendOneAsync(idContract);
                    await SaveSyncReceiptAsync(idContract, sync);

                    var expectedRid = CoerceGuid(sync?.responseId)?.ToString("D");
                    if (string.IsNullOrWhiteSpace(expectedRid))
                    {
                        fail++;
                        var msg = $"Invalid responseId '{sync?.responseId}'";
                        Debug.WriteLine($"[REGES FAIL] idContract={idContract} | error={msg}");
                        lines.Add(new SendLine { IdContract = idContract, Success = false, Message = msg });
                        continue;
                    }

                    await PollForResultAndUpdateAsync(expectedRid, CancellationToken.None);

                    var ridGuid = CoerceGuid(sync?.responseId);
                    if (ridGuid.HasValue)
                    {
                        await using var db = await _dbFactory.CreateDbContextAsync();
                        var rec = await db.Set<RegesSyncModificationContracts>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id_Rezultat_Mesaj.HasValue && x.Id_Rezultat_Mesaj.Value == ridGuid.Value);

                        if (rec != null &&
                            string.Equals(rec.Status, "Success", StringComparison.OrdinalIgnoreCase) &&
                            rec.RegesContractId.HasValue)
                        {
                            await db.ContractRuSuspended
                                   .Where(x => x.ContractId == idContract)
                                   .ExecuteUpdateAsync(s => s.SetProperty(p => p.RegesSyncVariable, 0));
                            ok++;
                            lines.Add(new SendLine
                            {
                                IdContract = idContract,
                                Success = true,
                                Message = "OK",
                                RegesContractId = rec.RegesContractId
                            });
                            Debug.WriteLine($"[REGES OK] idContract={idContract} | regesContractId={rec.RegesContractId}");
                        }
                        else
                        {
                            fail++;
                            var errMsg = rec?.Error_Message ?? "Unknown error";
                            lines.Add(new SendLine { IdContract = idContract, Success = false, Message = errMsg });
                            Debug.WriteLine($"[REGES FAIL] idContract={idContract} | error={errMsg}");
                        }
                    }
                    else
                    {
                        fail++;
                        var msg = $"Invalid responseId '{sync?.responseId}'";
                        lines.Add(new SendLine { IdContract = idContract, Success = false, Message = msg });
                        Debug.WriteLine($"[REGES FAIL] idContract={idContract} | error={msg}");
                    }
                }
                catch (Exception ex)
                {
                    fail++;
                    lines.Add(new SendLine { IdContract = idContract, Success = false, Message = ex.Message });
                    Debug.WriteLine($"[REGES EXCEPTION] idContract={idContract} | ex={ex.Message}");
                }
            }

            Debug.WriteLine($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");
            ShowSendSummary(lines, ids.Count);
            await LoadContractsAsync();
            RenumberRows();
            UpdateCounts();
        }

        protected async override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                await LoadContractsAsync();
            }
        }
        private async Task LoadContractsAsync()
        {
            await using var db = await _dbFactory.CreateDbContextAsync();

            var baseQuery =
                (from c in db.ContractsRu.AsNoTracking()
                 join rs in db.Set<RegesSync>().AsNoTracking() on c.PersonId equals rs.PersonId
                 join cc in db.Set<RegesContractSync>().AsNoTracking() on c.IdContract equals cc.IdContract
                 join cs in db.ContractRuSuspended.AsNoTracking() on c.IdContract equals cs.ContractId
                 where rs.RegesEmployeeId != null
                    && cc.RegesContractId != null
                    && c.ContractStatusId == 2
                    && cs.RegesSyncVariable == 2
                 orderby c.IdContract, cc.Updated_At descending
                 select new { c.IdContract, cc.RegesContractId, cc.Updated_At })
                .GroupBy(x => x.IdContract)
                .Select(g => g.OrderByDescending(z => z.Updated_At).FirstOrDefault());

            System.Diagnostics.Debug.WriteLine(baseQuery.ToQueryString());

            var baseRows = await baseQuery.ToListAsync();
            var rows = new List<object>(baseRows.Count);

            foreach (var it in baseRows)
            {
                var susp = await db.ContractRuSuspended
                    .AsNoTracking()
                    .Where(x => x.ContractId == it.IdContract)
                    .OrderByDescending(x => x.SuspensionStartDate ?? DateTime.MinValue)
                    .Select(x => new { x.SuspensionStartDate, x.SuspensionEndDate, x.SuspensionLegalGroundId })
                    .FirstOrDefaultAsync();

                DateTime? dataInceput = susp?.SuspensionStartDate
                    ?? await db.ContractRuSuspended
                        .AsNoTracking()
                        .Where(x => x.ContractId == it.IdContract && x.SuspensionStartDate != null)
                        .OrderByDescending(x => x.SuspensionStartDate)
                        .Select(x => x.SuspensionStartDate)
                        .FirstOrDefaultAsync();

                DateTime? dataSfarsit = susp?.SuspensionEndDate
                    ?? await db.ContractRuSuspended
                        .AsNoTracking()
                        .Where(x => x.ContractId == it.IdContract && x.SuspensionEndDate != null)
                        .OrderByDescending(x => x.SuspensionEndDate)
                        .Select(x => x.SuspensionEndDate)
                        .FirstOrDefaultAsync();

                string temeiLegal = string.Empty;
                if (susp?.SuspensionLegalGroundId != null)
                {
                    temeiLegal = await db.Set<SuspensionLegalGround>()
                        .AsNoTracking()
                        .Where(g => g.SuspensionLegalGroundId == susp.SuspensionLegalGroundId)
                        .Select(g => g.SuspensionLegalGroundCode)
                        .FirstOrDefaultAsync() ?? string.Empty;
                }

                rows.Add(new
                {
                    idContract = it.IdContract,
                    referintaContractId = it.RegesContractId?.ToString("D") ?? string.Empty,
                    actiuneTip = "actiuneSuspendare",
                    dataInceput,
                    dataSfarsit,
                    temeiLegal
                });
            }

            dgvContracteSuspendate.AutoGenerateColumns = true;
            dgvContracteSuspendate.DataSource = rows;

            _allRowsData = rows;
            _rowsData = rows;
            _rowItemType = rows.Count > 0 ? rows[0].GetType() : null;

            UpdateCounts();
        }

        // Build and show a detailed summary. Falls back to a scrollable dialog if long.
        private void ShowSendSummary(IReadOnlyList<SendLine> lines, int total)
        {
            var ok = lines.Count(l => l.Success);
            var fail = lines.Count - ok;

            var sb = new StringBuilder();
            sb.AppendLine($"[REGES SUMMARY] total={total} ok={ok} fail={fail}");

            if (fail > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Erori:");
                foreach (var l in lines.Where(x => !x.Success))
                    sb.AppendLine($" • Contract {l.IdContract}: {l.Message}");
            }

            if (ok > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Succes:");
                foreach (var l in lines.Where(x => x.Success))
                {
                    var idText = l.RegesContractId.HasValue ? l.RegesContractId.Value.ToString("D") : "-";
                    sb.AppendLine($" • Contract {l.IdContract}: {idText}");
                }
            }

            var text = sb.ToString();

            // MessageBox is fine for shorter text, otherwise open a scrollable window
            if (text.Length < 6000)
            {
                MessageBox.Show(text, "Rezultat trimitere contracte",
                    MessageBoxButtons.OK,
                    fail > 0 ? MessageBoxIcon.Error : MessageBoxIcon.Information);
            }
            else
            {
                using var dlg = new Form
                {
                    Text = "Rezultat trimitere contracte",
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
                    Font = new Font("Consolas", 10f),
                    Text = text
                };
                var copy = new Button { Text = "Copiază", Dock = DockStyle.Bottom, Height = 36 };
                copy.Click += (_, __) => Clipboard.SetText(text);

                dlg.Controls.Add(txt);
                dlg.Controls.Add(copy);
                dlg.ShowDialog(this);
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

            dgvContracteSuspendate.DataSource = list;
            _rowsData = list;

            RenumberRows();
            UpdateCounts();
            //ApplyRowColorsByRegesId();

            foreach (DataGridViewColumn c in dgvContracteSuspendate.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            if (!string.IsNullOrWhiteSpace(_lastSortProp))
            {
                var sortedCol = dgvContracteSuspendate.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c =>
                        string.Equals(c.DataPropertyName, _lastSortProp, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Name, _lastSortProp, StringComparison.OrdinalIgnoreCase));
                if (sortedCol != null)
                    sortedCol.HeaderCell.SortGlyphDirection =
                        _lastSortDir == ListSortDirection.Descending ? SortOrder.Descending : SortOrder.Ascending;
            }
        }


        private List<int> GetSelectedContractIds()
        {
            DataGridViewColumn? pidCol = null;
            foreach (DataGridViewColumn col in dgvContracteSuspendate.Columns)
            {
                if (string.Equals(col.DataPropertyName, ContractIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    pidCol = col; break;
                }
            }
            if (pidCol == null) return new List<int>();

            var ids = new List<int>();
            foreach (DataGridViewRow row in dgvContracteSuspendate.Rows)
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


        // to do later
        private bool GuardRequiredFieldsOrWarn(ContinutContract cont, int idContract)
        {
            //var (ok, missing) = ValidateRequiredForContract(cont);
            //if (ok) return true;

            //var msg =
            //    "Nu pot trimite contractul deoarece lipsesc câmpuri obligatorii:\n" +
            //    string.Join("\n", missing.Select(m => " • " + m)) +
            //    $"\n\nIdContract: {idContract}\n" +
            //    $"DataConsemnare: {(cont?.DataConsemnare?.ToString("yyyy-MM-dd") ?? "<null>")}";

            //MessageBox.Show(msg, "REGES – câmpuri obligatorii lipsă",
            //    MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return true;
        }


        // Step 0: Build payload from DB (uses exactly your mapper + names, no defaults)
        public async Task<ContractSuspendariEnvelope> BuildContractPayloadAsync(int idContract)
        {
            await using var _db = await _dbFactory.CreateDbContextAsync();

            // 1) Contract
            var c = await _db.ContractRuSuspended
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractId == idContract);
            if (c == null)
                throw new InvalidOperationException($"Contract {idContract} not found.");

            // 2) SafeLookup
            async Task<string> SafeLookup(Func<Task<string>> query)
            {
                try { return await query() ?? ""; } catch { return ""; }
            }

            // 3) Lookups

            int? personId = await _db.ContractsRu
                .AsNoTracking()
                .Where(cr => cr.IdContract == idContract)
                .Select(cr => (int?)cr.PersonId)
                .FirstOrDefaultAsync();



            if (personId is null)
                throw new InvalidOperationException($"No PersonId for contract {idContract}.");


            var regesEmployeeGuid = await _db.Set<RegesSync>()
              .AsNoTracking()
              .Where(rs => rs.PersonId == personId.Value && rs.RegesEmployeeId != null)
              .OrderByDescending(rs => rs.UpdatedAt)
              .Select(rs => rs.RegesEmployeeId)
              .FirstOrDefaultAsync();

            //regesContractsGuid
            var regesContractsGuid = await _db.Set<RegesContractSync>()
            .AsNoTracking()
            .Where(rss => rss.IdContract == c.ContractId && rss.RegesContractId != null)
            .OrderByDescending(rs => rs.Updated_At)
            .Select(rs => rs.RegesContractId)
            .FirstOrDefaultAsync();

            var suspensionStart = c.SuspensionStartDate
     ?? await _db.ContractRuSuspended
          .AsNoTracking()
          .Where(x => x.ContractId == idContract && x.SuspensionStartDate != null)
          .OrderByDescending(x => x.SuspensionStartDate)
          .Select(x => x.SuspensionStartDate)
          .FirstOrDefaultAsync()

     ?? throw new InvalidOperationException($"No SuspensionStartDate for contract {idContract}.");


            var suspensionEnd = c.SuspensionEndDate
                ?? await _db.ContractRuSuspended
                     .AsNoTracking()
                     .Where(x => x.ContractId == idContract && x.SuspensionEndDate != null)
                     .OrderByDescending(x => x.SuspensionEndDate)
                     .Select(x => x.SuspensionEndDate)
                     .FirstOrDefaultAsync();

            string legalGroundCode = await _db.Set<SuspensionLegalGround>()
    .Where(g => g.SuspensionLegalGroundId == c.SuspensionLegalGroundId)
    .Select(g => g.SuspensionLegalGroundCode)


    .FirstOrDefaultAsync() ?? "";

            var env = new ContractSuspendariEnvelope
            {
                Header = new Header
                {
                    MessageId = Guid.NewGuid().ToString("D"),
                    ClientApplication = "SAP",
                    Version = "5",
                    Operation = "ModificareSuspendareContract",
                    AuthorId = Properties.Settings.Default.SavedCredentialsUser,
                    SessionId = _session.SessionId,
                    User = _session.UserName,
                    Timestamp = DateTime.UtcNow
                },
                ReferintaContract = new ReferintaContract
                {
                    // "$type":"referinta" lives in this model
                    Id = regesContractsGuid?.ToString("D") ?? ""
                },
                Actiune = new Actiune
                {
                    // must be "actiuneSuspendare"
                    Type = "actiuneSuspendare",
                    DataInceput = suspensionStart,    // DateTime (with offset if you store it)
                    DataSfarsit = suspensionEnd,      // can be null (omitted in JSON)
                    //Explicatie = $"SUSPENDARE CONFORM {legalGroundCode}",
                    TemeiLegal = legalGroundCode
                },
                // Do NOT send document: leave null -> omitted from JSON
                DocumentJustificativ = null
            };

            // For visibility
            Debug.WriteLine($"[SUSP PAYLOAD] {JsonSerializer.Serialize(env, _jsonOpts)}");
            return env;

        }


        // Step 1: Send one payload -> get synchronous MessageResponse (recipisa)
        private async Task<SyncResponse> SendOneAsync(int idContract)
        {
            // 0) Build payload exactly as before
            var payload = await BuildContractPayloadAsync(idContract);

            // 1) Validate required fields
            bool mustValidate = true;   // RELEASE: always validate
#if DEBUG
            mustValidate = _debugValidate; // DEBUG: controlled by checkbox
#endif



            // 2) Serialize after validation (unchanged)
            var json = RegesJson.SanitizeAndSerialize(payload);

            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            Debug.WriteLine($"[SEND] POST {http.BaseAddress}{InregistrareContractUrl}");
            Debug.WriteLine($"[SEND] Body={json}");

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PostAsync(InregistrareContractUrl, content);
            var body = await resp.Content.ReadAsStringAsync();
            Debug.WriteLine($"[SEND] Status={(int)resp.StatusCode} Body={(body)}");

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {body}");

            var sync = JsonSerializer.Deserialize<SyncResponse>(body, _jsonOpts)
                       ?? throw new InvalidOperationException("Empty sync response.");

            Debug.WriteLine($"[SYNC OK] contractId={idContract}, responseId={sync.responseId}, messageId={sync.header?.messageId}");
            return sync;
        }

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        
        private string GetAccessToken()
        {
            if (SessionState.Tokens == null || string.IsNullOrEmpty(SessionState.Tokens.AccessToken))
                throw new InvalidOperationException("Access token not available. Please log in first.");

            // optional: check if token is expired
            if (SessionState.Tokens.Expiration <= DateTime.UtcNow)
                throw new InvalidOperationException("Access token has expired. Please refresh or log in again.");

            return SessionState.Tokens.AccessToken;
        }


        private static Guid? CoerceGuid(object? value)
        {
            if (value is null) return null;
            if (value is Guid g) return g;
            if (value is string s && Guid.TryParse(s, out var gs)) return gs;

            var text = value.ToString();
            return Guid.TryParse(text, out var g2) ? g2 : (Guid?)null;
        }


        private sealed class SendLine
        {
            public int IdContract { get; init; }
            public bool Success { get; init; }
            public string Message { get; init; } = "";
            public Guid? RegesContractId { get; init; }
        }
    }


}
