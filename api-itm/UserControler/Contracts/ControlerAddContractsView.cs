using api_itm.Data.Configurations.Salary;
using api_itm.Data.Entity;
using api_itm.Data.Entity.Ru.Contracts.Work;
using api_itm.Data.Entity.Ru.Salary;
using api_itm.Infrastructure;
using api_itm.Infrastructure.Mappers;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using api_itm.Models.Contracts;
using api_itm.Models.Contracts.Envelope;
using api_itm.Models.Employee;
using api_itm.Models.Reges;
using api_itm.Models.View;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
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
using System.Collections; // for IEnumerable

namespace api_itm.UserControler.Contracts
{
    public partial class ControlerAddContractsView : UserControl
    {
        //private readonly AppDbContext _db;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

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

#if DEBUG
        private const bool DEBUG_SHOW_PERSON_ID = true;
#else
        private const bool DEBUG_SHOW_PERSON_ID = false;
#endif


        public ControlerAddContractsView(IDbContextFactory<AppDbContext> db, ISessionContext session)
        {
            InitializeComponent();
            _dbFactory = db;

            _session = session;
            BuildBarsAndLayout();
            WireGrid();
        }

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        private void ControlerAddContractsView_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Wires up all DataGridView behaviors for dgvAddContracts.
        /// </summary>
        /// <remarks>
        /// - Commits checkbox edits immediately so <see cref="CellValueChanged"/> fires without leaving the cell.
        /// - When the Select column changes, updates the “Selected/Total” counter.
        /// - After data binding completes, ensures the special columns (Select + No) exist/are ordered,
        ///   renumbers rows (1..N), refreshes the counters, and applies row coloring based on REGES IDs.
        /// - Keeps row numbers and counters correct after sorting, adding, or removing rows.
        /// - On row double-click, opens a JSON preview for the selected person.
        /// </remarks>

        private void WireGrid()
        {
            dgvAddContracts.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvAddContracts.IsCurrentCellDirty)
                    dgvAddContracts.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvAddContracts.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvAddContracts.Columns[e.ColumnIndex].Name == SelectColName)
                    UpdateCounts();
            };

            dgvAddContracts.DataBindingComplete += (_, __) =>
            {
                EnsureSpecialColumns();
                RenumberRows();
                UpdateCounts();
                ApplyRowColorsByRegesId();
            };

            dgvAddContracts.Sorted += (_, __) => RenumberRows();
            dgvAddContracts.RowsAdded += (_, __) => { RenumberRows(); UpdateCounts(); };
            dgvAddContracts.RowsRemoved += (_, __) => { RenumberRows(); UpdateCounts(); };

            dgvAddContracts.CellDoubleClick += async (_, __) => await PreviewSelectedRowJsonAsync();
            dgvAddContracts.ColumnHeaderMouseClick += (_, e) => OnHeaderClick(e.ColumnIndex);

        }

        /// <summary>
        /// Builds the full layout of the control:
        /// - Creates a top bar with a title, a "Select all contracts" checkbox, and a counter label.
        /// - Creates a bottom bar with a "Trimitere contracte" button.
        /// - Configures the DataGridView (dgvAddContracts) for displaying contracts:
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
                Text = "Adaugare contracte",
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
                Text = "Trimitere contracte",
                AutoSize = true,
                Margin = new Padding(8)
            };
            bottomFlow.Controls.Add(_btnSendSelected);
            _bottomBar.Controls.Add(bottomFlow);

            // ========= GRID =========
            if (dgvAddContracts == null)
                dgvAddContracts = new DataGridView();

            // events for checkbox behavior + counters + row numbering
            dgvAddContracts.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvAddContracts.IsCurrentCellDirty)
                    dgvAddContracts.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvAddContracts.CellValueChanged += (_, e) =>
            {
                if (e.RowIndex >= 0 && dgvAddContracts.Columns[e.ColumnIndex].Name == SelectColName)
                    UpdateCounts();
            };

            dgvAddContracts.DataBindingComplete += (_, __) =>
            {
                EnsureSpecialColumns();
                RenumberRows();
                UpdateCounts();
                ApplyRowColorsByRegesId();
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


            dgvAddContracts.Dock = DockStyle.Fill;
            dgvAddContracts.AutoGenerateColumns = true;
            dgvAddContracts.ReadOnly = false;                 // needed for checkbox column
            dgvAddContracts.AllowUserToAddRows = false;
            dgvAddContracts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvAddContracts.MultiSelect = true;
            dgvAddContracts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            // ========= ADD CONTROLS IN THE RIGHT ORDER =========
            Controls.Clear();
            Controls.Add(dgvAddContracts); // Fill
            Controls.Add(_bottomBar);       // Bottom
            Controls.Add(_topBar);          // Top

            // ========= EVENTS =========
            _chkSelectAll.CheckedChanged += (_, __) => ToggleSelectAll(_chkSelectAll.Checked);
            //_btnSendSelected.Click += async (_, __) => await SendSelectedAsync();

            dgvAddContracts.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0) // ignore header double-clicks
                    await PreviewSelectedRowJsonAsync();
            };
            dgvAddContracts.CurrentCellChanged += (_, __) => UpdateCounts();
            ResumeLayout();
        }

        private void EnsureSpecialColumns()
        {
            // 1) Select checkbox (first column)
            if (!dgvAddContracts.Columns.Contains(SelectColName))
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
                dgvAddContracts.Columns.Insert(0, chk);
            }

            // 2) Row number (second column)
            if (!dgvAddContracts.Columns.Contains(RowNoColName))
            {
                var no = new DataGridViewTextBoxColumn
                {
                    Name = RowNoColName,
                    HeaderText = "No",
                    ReadOnly = true,
                    Width = 50
                };
                dgvAddContracts.Columns.Insert(1, no);
            }

            // 3) PersonId column (auto-generated from projection new { personId = p.PersonId, ... })
            //    In DEBUG we show it and put it right after the row number; in Release it stays hidden.
            DataGridViewColumn? pidCol = null;

            foreach (DataGridViewColumn col in dgvAddContracts.Columns)
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
                dgvAddContracts.Columns.Insert(2, pidCol);
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
            //dgvAddContracts.Columns[SelectColName].DisplayIndex = 0;
            //dgvAddContracts.Columns[RowNoColName].DisplayIndex = 1;
        }

        private void RenumberRows()
        {
            if (!dgvAddContracts.Columns.Contains(RowNoColName)) return;

            for (int i = 0; i < dgvAddContracts.Rows.Count; i++)
            {
                var row = dgvAddContracts.Rows[i];
                if (!row.IsNewRow)
                    row.Cells[RowNoColName].Value = i + 1;
            }
        }

        /// <summary>
        /// On row double-click: extracts idContract from the current row and shows JSON preview.
        /// </summary>
        private async Task PreviewSelectedRowJsonAsync()
        {
            if (dgvAddContracts.CurrentRow == null) return;

            int? idContract = GetIdContractFromRow(dgvAddContracts.CurrentRow);

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

        /// <summary>
        /// Reads an int property from the row's bound object (preferred) or from a column
        /// whose DataPropertyName/Name matches <paramref name="propName"/>.
        /// </summary>
        private int? GetIntFromRow(DataGridViewRow row, string propName)
        {
            if (row == null) return null;

            // 1) From the bound object
            var item = row.DataBoundItem;
            if (item != null)
            {
                var prop = item.GetType().GetProperty(
                    propName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null)
                {
                    var v = prop.GetValue(item, null);
                    var n = CoerceToInt(v);
                    if (n.HasValue) return n.Value;
                }
            }

            // 2) From a matching column
            foreach (DataGridViewColumn col in row.DataGridView.Columns)
            {
                if (col.DataPropertyName.Equals(propName, StringComparison.OrdinalIgnoreCase) ||
                    col.Name.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    var v = row.Cells[col.Index].Value;
                    return CoerceToInt(v);
                }
            }

            return null;
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
            foreach (DataGridViewColumn col in dgvAddContracts.Columns)
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

        private async Task PreviewJsonForPerson(int personId)
        {
            var payload = await BuildContractPayloadAsync(personId);

            // also make sure names from DB are fixed BEFORE mapping (see earlier message)
            var json = RegesJson.SanitizeAndSerialize(payload);

            ShowJsonPreview(json);
        }

        //private async Task SendSelectedAsync()
        //{
        //    var ids = GetSelectedPersonIds();
        //    if (ids.Count == 0)
        //    {
        //        MessageBox.Show("Select at least one row.");
        //        return;
        //    }

        //    int ok = 0, fail = 0;

        //    foreach (var personId in ids)
        //    {
        //        try
        //        {
        //            // 1) Send to API
        //            var sync = await SendOneAsync(personId);

        //            // 2) Save sync receipt (same row we’ll later update)
        //            await SaveSyncReceiptAsync(personId, sync);

        //            // 3) Poll + update that same row (sets Status, ErrorMessage, RegesEmployeeId)
        //            await PollForResultAndUpdateAsync(sync.responseId, CancellationToken.None);

        //            // 4) Read back the row for this responseId and log result
        //            if (Guid.TryParse(sync.responseId, out var rid))
        //            {
        //                var rec = await _db.Set<RegesSync>()
        //                    .AsNoTracking()
        //                    .FirstOrDefaultAsync(x => x.MessageResultId.HasValue && x.MessageResultId.Value == rid);

        //                var p = await _db.People
        //                    .Where(x => x.PersonId == personId)
        //                    .Select(x => new { x.LastName, x.FirstName, x.NationalId })
        //                    .FirstOrDefaultAsync();

        //                if (rec != null && string.Equals(rec.Status, "Success", StringComparison.OrdinalIgnoreCase) && rec.RegesEmployeeId.HasValue)
        //                {
        //                    ok++;
        //                    Debug.WriteLine($"[REGES OK] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | salariatId={rec.RegesEmployeeId}");
        //                }
        //                else
        //                {
        //                    fail++;
        //                    var errMsg = rec?.ErrorMessage ?? "Unknown error";
        //                    Debug.WriteLine($"[REGES FAIL] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | error={errMsg}");
        //                }
        //            }
        //            else
        //            {
        //                fail++;
        //                var p = await _db.People
        //                    .Where(x => x.PersonId == personId)
        //                    .Select(x => new { x.LastName, x.FirstName, x.NationalId })
        //                    .FirstOrDefaultAsync();

        //                Debug.WriteLine($"[REGES FAIL] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | error=Invalid responseId '{sync.responseId}'");
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            fail++;

        //            var p = await _db.People
        //                .Where(x => x.PersonId == personId)
        //                .Select(x => new { x.LastName, x.FirstName, x.NationalId })
        //                .FirstOrDefaultAsync();

        //            Debug.WriteLine($"[REGES EXCEPTION] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | ex={ex.Message}");
        //            // keep going
        //        }
        //    }

        //    // final summary in Output window
        //    Debug.WriteLine($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");
        //    MessageBox.Show($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");
        //    // optional UI summary:
        //    // MessageBox.Show($"Trimise: {ids.Count}\nSucces: {ok}\nErori: {fail}", "Rezultat trimitere");
        //}

        private void ToggleSelectAll(bool @checked)
        {
            if (!dgvAddContracts.Columns.Contains(SelectColName)) return;

            foreach (DataGridViewRow row in dgvAddContracts.Rows)
            {
                if (!row.IsNewRow)
                    row.Cells[SelectColName].Value = @checked;
            }
            dgvAddContracts.EndEdit();
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            int total = dgvAddContracts.Rows.Cast<DataGridViewRow>().Count(r => !r.IsNewRow);
            int selected = 0;

            if (dgvAddContracts.Columns.Contains(SelectColName))
            {
                foreach (DataGridViewRow r in dgvAddContracts.Rows)
                {
                    if (r.IsNewRow) continue;
                    selected += Convert.ToBoolean(r.Cells[SelectColName].Value ?? false) ? 1 : 0;
                }
            }

            _lblCount.Text = $"Total: {total:n0} | Selected: {selected:n0}";
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

            var rows = await db.ContractsRu
                 .AsNoTracking()
                 .OrderBy(c => c.IdContract)
                 .ThenBy(c => c.RecordDate ?? c.ModificationDate ?? c.RevisalTransmitDate)
                 .Select(c => new
                 {
                     idContract = c.IdContract,
                     personId = (int?)null,
                     dataConsemnare = c.RecordDate,
                     dataContract = c.ContractDate,
                     dataInceputContract = c.StartDate,
                     dataSfarsitContract = c.EndDate,
                     exceptieDataSfarsitId = c.EndDateExceptionId,
                     numarContract = c.ContractNumber,
                     salariu = c.GrossSalary ?? c.BaseSalary ?? c.EmploymentSalary,
                     tipLocMunca = c.Headquarters,
                     norma112 = c.Norm112,
                     durata = c.ContractDuration,
                     intervalTimpId = c.WorkTimeIntervalId,
                     repartizareMuncaId = c.WorkDistributionId,
                     inceputInterval = c.StartHour,
                     sfarsitInterval = c.EndHour,
                     statusId = c.ContractStatusId,
                     modificatLa = c.ModificationDate,
                     terminareLa = c.TerminationDate,
                     transferLa = c.TransferDate
                 })
                 .ToListAsync();

            var greenContractIds = await db.Set<RegesContractSync>()
    .Where(r => r.RegesContractId != null && r.IdContract != null)
    .Select(r => r.IdContract!.Value)
    .Distinct()
    .ToListAsync();
             

            _contractsWithRegesId = new HashSet<int>(greenContractIds);

            dgvAddContracts.AutoGenerateColumns = true;
            dgvAddContracts.DataSource = rows;

#if DEBUG
            var idCol = dgvAddContracts.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c => c.DataPropertyName.Equals("idContract", StringComparison.OrdinalIgnoreCase));
            if (idCol != null)
            {
                idCol.Visible = true;
                idCol.HeaderText = "IdContract DEBUG only";
                idCol.ReadOnly = true;
                idCol.DisplayIndex = 0;
            }
#endif

            
            _rowsData = dgvAddContracts.DataSource;                    // List<anon>
            _rowItemType = (rows.Count > 0) ? rows[0].GetType() : null; // remember anon type

            // // search

            dgvAddContracts.DataSource = rows;

            _allRowsData = rows;                               // full list for filtering
            _rowsData = dgvAddContracts.DataSource;        // current (possibly filtered/sorted)
            _rowItemType = (rows.Count > 0) ? rows[0].GetType() : null;


            UpdateCounts();
            ApplyRowColorsByRegesId();
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

            dgvAddContracts.DataSource = list;
            _rowsData = list;

            RenumberRows();
            UpdateCounts();
            ApplyRowColorsByRegesId();

            foreach (DataGridViewColumn c in dgvAddContracts.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            if (!string.IsNullOrWhiteSpace(_lastSortProp))
            {
                var sortedCol = dgvAddContracts.Columns
                    .Cast<DataGridViewColumn>()
                    .FirstOrDefault(c =>
                        string.Equals(c.DataPropertyName, _lastSortProp, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(c.Name, _lastSortProp, StringComparison.OrdinalIgnoreCase));
                if (sortedCol != null)
                    sortedCol.HeaderCell.SortGlyphDirection =
                        _lastSortDir == ListSortDirection.Descending ? SortOrder.Descending : SortOrder.Ascending;
            }
        }


        private void EnableProgrammaticSortOnDataColumns()
        {
            foreach (DataGridViewColumn col in dgvAddContracts.Columns)
            {
                if (col == null) continue;
                if (col.Name == SelectColName || col.Name == RowNoColName) continue;
                col.SortMode = DataGridViewColumnSortMode.Programmatic;
                col.HeaderCell.SortGlyphDirection = SortOrder.None;
            }
        }

        private void OnHeaderClick(int colIndex)
        {
            if (colIndex < 0 || colIndex >= dgvAddContracts.Columns.Count) return;

            var clickedCol = dgvAddContracts.Columns[colIndex];
            if (clickedCol == null) return;
            if (clickedCol.Name == SelectColName || clickedCol.Name == RowNoColName) return;

            var prop = string.IsNullOrWhiteSpace(clickedCol.DataPropertyName) ? clickedCol.Name : clickedCol.DataPropertyName;
            if (string.IsNullOrWhiteSpace(prop)) return;

            var wantDesc = (_lastSortProp == prop) ? (_lastSortDir == ListSortDirection.Ascending) : true;

            // rebind (this recreates columns)
            ApplySort(prop, wantDesc);

            // after rebinding, work with the NEW column instances
            foreach (DataGridViewColumn c in dgvAddContracts.Columns)
                c.HeaderCell.SortGlyphDirection = SortOrder.None;

            var newCol = dgvAddContracts.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.DataPropertyName, prop, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, prop, StringComparison.OrdinalIgnoreCase));

            if (newCol != null)
                newCol.HeaderCell.SortGlyphDirection = wantDesc ? SortOrder.Descending : SortOrder.Ascending;

            _lastSortProp = prop;
            _lastSortDir = wantDesc ? ListSortDirection.Descending : ListSortDirection.Ascending;
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

            dgvAddContracts.DataSource = sortedListObj;
            _rowsData = sortedListObj; // noua ordine

            // păstrăm UX-ul existent
            RenumberRows();
            UpdateCounts();
            ApplyRowColorsByRegesId();
        }

        private async Task PreviewJsonForContract(int idContract)
        {
            var payload = await BuildContractPayloadAsync(idContract);

            // also make sure names from DB are fixed BEFORE mapping (see earlier message)
            var json = RegesJson.SanitizeAndSerialize(payload);

            ShowJsonPreview(json);
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
            foreach (DataGridViewColumn col in dgvAddContracts.Columns)
            {
                if (string.Equals(col.DataPropertyName, "idContract", StringComparison.OrdinalIgnoreCase))
                    return CoerceToInt(row.Cells[col.Index].Value);
            }

            return null;
        }

        //private int? GetPersonIdFromRow(DataGridViewRow row)
        //{
        //    foreach (DataGridViewColumn col in dgvAddContracts.Columns)
        //    {
        //        if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
        //        {
        //            var val = row.Cells[col.Index].Value;
        //            if (val == null || val == DBNull.Value) return null;
        //            if (int.TryParse(val.ToString(), out var pid)) return pid;
        //        }
        //    }
        //    return null;
        //}



        // Step 0: Build payload from DB (uses exactly your mapper + names, no defaults)
        private async Task<ContractEnvelope> BuildContractPayloadAsync(int idContract)
        {
            await using var _db = await _dbFactory.CreateDbContextAsync();

            // 1) Contract
            var c = await _db.ContractsRu
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdContract == idContract);
            if (c == null)
                throw new InvalidOperationException($"Contract {idContract} not found.");

            // 2) SafeLookup
            async Task<string> SafeLookup(Func<Task<string>> query)
            {
                try { return await query() ?? ""; } catch { return ""; }
            }

            // 3) Lookups
            var endDateExceptionCode = RegesJson.FixText(await SafeLookup(() =>
                _db.EndDateExceptions
                  .Where(e => e.EndDateExceptionId == c.EndDateExceptionId)
                  .Select(e => e.EndDateExceptionCode)
                  .FirstOrDefaultAsync()));

            Debug.WriteLine($"[DEBUG] Contract {c.IdContract} | EndDateExceptionId={c.EndDateExceptionId} => Code='{endDateExceptionCode}'");

            var isState7 = await _db.ContractsState
                .Where(s => s.ContractStateId == c.ContractStatusId)
                .Select(s => s.ContractStateId == 7)
                .FirstOrDefaultAsync();

            var educationLevelCode = await _db.FunctionsStat
                .Where(f => f.FunctionStatId == c.FunctionStatId)
                .Join(_db.EducationLevels, f => f.EducationLevelId, e => e.EducationLevelId,
                      (f, e) => e.EducationLevelCode)
                .FirstOrDefaultAsync();

            var workingscheduleCode = await _db.WorkScheduleNorm
               .Where(w => w.WorkScheduleId == c.NormId)
               .Select(w=> w.WorkScheduleCode)
               .FirstOrDefaultAsync();

            var workingTimeIntervalCode = await _db.WorkingTimeIntervals
               .Where(wt => wt.WorkingTimeIntervalId == c.WorkTimeIntervalId)
               .Select(wt => wt.WorkingTimeIntervalCode)
               .FirstOrDefaultAsync();


            var workDistributionCode = await _db.WorkDistributionId
               .Where(wd => wd.WorkDistributionIdId == c.WorkDistributionId)
               .Select(wd => wd.WorkDistributionIdCode)
               .FirstOrDefaultAsync(); //WorkDistributionIds

            Debug.WriteLine("workDistributionCode: " + workDistributionCode);

            var workTimeAllocationCode = await _db.WorkTimeAllocation
              .Where(wta => wta.WorkTimeAllocationId == c.WorkTimeAllocationId)
              .Select(wta => wta.WorkTimeAllocationCode)
              .FirstOrDefaultAsync(); //ShiftType

            var typeContractRuCode = await _db.TypeContractRu
             .Where(tyr => tyr.TypeContractRuId == c.ContractTypeId)
             .Select(tyr => tyr.TypeContractRuCode)
             .FirstOrDefaultAsync();//TypeContractRu

            var typeContractDurationCode = await _db.ContractTypeDuration
             .Where(tcd => tcd.ContractTypeDurationCode == c.DurationTypeId.ToString())
             .Select(tcd => tcd.ContractTypeDurationCode)
             .FirstOrDefaultAsync(); //ContractTypeDuration


            var shiftTypeCode = await _db.ShiftType
             .Where(shift => shift.ShiftTypeId == c.ShiftTypeId)
             .Select(shift => shift.ShiftTypeCode)
             .FirstOrDefaultAsync(); //ShiftType


            var workNormTypeCode = await _db.WorkNormType
             .Where(shift => shift.WorkNormTypeId == c.NormId)
             .Select(shift => shift.WorkNormTypeCode)
             .FirstOrDefaultAsync(); //WorkNormType

            //var countyCode = await _db.County
            //.Where(cc => cc.CountyId == c.)
            //.Select(cc => cc.WorkNormTypeCode)
            //.FirstOrDefaultAsync(); //County

            Debug.WriteLine($"[DEBUG] Contract {c.IdContract} | FunctionStatId={c.FunctionStatId} => EducationLevelCode='{educationLevelCode}'");

            // 3b) SPORURI from contracte_sporuri x tipspor (by contract)
            var sporRows = await _db.Set<ContractBonuses>()
                .AsNoTracking()
                .Where(b => b.IdContract == c.IdContract)
                .Join(_db.Set<SporType>().AsNoTracking(),
                      b => b.IdSpor,
                      t => t.SporTypeId,
                      (b, t) => new
                      {
                          b.IdContracteSporuri,
                          b.IdSpor,
                          b.ValoareSpor,           // amount (RON)
                          b.ValoareSporProcent,    // percent
                          t.SporName,
                          t.SporTypeCode
                      })
                .OrderBy(x => x.IdContracteSporuri)
                .ToListAsync();

            var sporuri = new List<SporSalariu>();
            foreach (var x in sporRows)
            {
                // Percent only if ValoareSporProcent has a non-null, non-zero value
                bool isPercent = x.ValoareSporProcent.HasValue && x.ValoareSporProcent.Value != 0m;

                var value = isPercent ? x.ValoareSporProcent!.Value : (x.ValoareSpor ?? 0m);
                if (value <= 0m) continue;

                var referintaId = string.IsNullOrWhiteSpace(x.SporTypeCode) ? x.IdSpor.ToString() : x.SporTypeCode;

                sporuri.Add(new SporSalariu
                {
                    IsProcent = isPercent,
                    Valoare = value,
                    Moneda = isPercent ? null : "RON",
                    Tip = new SporTip
                    {
                        Type = "sporAngajator",
                        Referinta = new Referinta { Id = referintaId },
                        Nume = RegesJson.FixText(x.SporName)
                    }
                });

                Debug.WriteLine($"[DEBUG] Spor row {x.IdContracteSporuri}: IdSpor={x.IdSpor}, Name='{x.SporName}', Code='{x.SporTypeCode}', IsPercent={isPercent}, Value={value}");
            }

            // 4) Time window
            DateTime? inceputInterval = null, sfarsitInterval = null;
            if (c.StartDate.HasValue && c.StartHour.HasValue)
                inceputInterval = c.StartDate.Value.Date + c.StartHour.Value;
            if (c.StartDate.HasValue && c.EndHour.HasValue)
                sfarsitInterval = c.StartDate.Value.Date + c.EndHour.Value;

            // 5) COR parse
            static int TryParseInt(string? s) => int.TryParse(s, out var n) ? n : 0;

            // 6) Build payload
            var continut = new ContinutContract
            {
                ReferintaSalariat = new Models.Contracts.Envelope.ReferintaSalariat { Id = "" }, // TODO

                Cor = new Cor
                {
                    Cod = TryParseInt(c.OccupationCode),
                    Versiune = 0
                },

                DataConsemnare = c.RecordDate,
                DataContract = c.ContractDate,
                DataInceputContract = c.StartDate,

                ExceptieDataSfarsit = string.IsNullOrWhiteSpace(endDateExceptionCode) ? null : endDateExceptionCode,

                NumarContract = c.ContractNumber ?? "",
                Radiat = isState7,

                Salariu = c.GrossSalary,
                Moneda = "RON",
                NivelStudii = educationLevelCode,

                SporuriSalariu = sporuri,

                StareCurenta = new StareCurenta(), //  Starea curenta a contractului. Se trimite ca element gol de catre client. In interogari este intors
                //populat corespunzator de catre server

                TimpMunca = new TimpMunca
                {
                    Norma = workingscheduleCode,
                    Durata = c.ContractDuration.HasValue ? (int?)Convert.ToInt32(c.ContractDuration.Value) : null,
                    IntervalTimp = workingTimeIntervalCode,
                    Repartizare = workDistributionCode, //cateodata id ul este 0 si nu apare
                    RepartizareMunca = workTimeAllocationCode, //ex. "Zilnic"  //cateodata id ul este 0 si nu apare
                    InceputInterval = inceputInterval,
                    SfarsitInterval = sfarsitInterval,
                    //NotaRepartizareMunca = null,
                    TipTura = shiftTypeCode,
                    //ObservatiiTipTuraAlta = null
                },

                TipContract = typeContractRuCode,
                TipDurata = typeContractDurationCode,
                TipNorma = workNormTypeCode,
                TipLocMunca = c.Headquarters,
                JudetLocMunca = null,

                AplicaL153 = null,

                DetaliiL153 = new DetaliiL153 // in salarycontractru
                { 
                     //AnexaL153                   = c.L153AnnexCode,
                     //CapitolL153                 = c.L153CapitolCode,
                     //LiteraL153                  = c.L153LiteraCode,
                     //ClasificareSuplimentaraL153 = c.L153ClasifCode,
                     //FunctieL153                 = c.L153FunctieCode,
                     //SpecialitateFunctieL153     = c.L153SpecFunctieCode,
                     //StructuraAprobataL153       = c.L153StructuraCode,
                     //SpecialitateStructuraAprobataL153 = c.L153SpecStructuraCode,
                     //GradProfesionalL153         = c.L153GradProfCode,
                     //GradatieL153                = c.L153GradatieCode,
                     //DenumireAltaFunctieL153     = c.L153AltaFunctieName,
                     //ExplicatieFunctieL153       = c.L153ExplicatieFunctie,
                     //AltGradProfesionalL153      = c.L153AltGradProfText
                }
            };

            var envelope = new ContractEnvelope
            {
                Header = new Header
                {
                    MessageId = Guid.NewGuid().ToString("D"),
                    ClientApplication = "SAP",
                    Version = "5",
                    Operation = "AdaugareContract",
                    AuthorId = Properties.Settings.Default.SavedCredentialsUser,
                    SessionId = _session.SessionId,
                    User = _session.UserName,
                    Timestamp = DateTime.UtcNow
                },
                Continut = continut
            };

            Debug.WriteLine($"Built contract payload for idContract={idContract}: {JsonSerializer.Serialize(envelope, _jsonOpts)}");
            return envelope;
        }

        // Step 1: Send one payload -> get synchronous MessageResponse (recipisa)
        private async Task<SyncResponse> SendOneAsync(int idContract)
        {
            var payload = await BuildContractPayloadAsync(idContract);
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

        private string GetAccessToken()
        {
            if (SessionState.Tokens == null || string.IsNullOrEmpty(SessionState.Tokens.AccessToken))
                throw new InvalidOperationException("Access token not available. Please log in first.");

            // optional: check if token is expired
            if (SessionState.Tokens.Expiration <= DateTime.UtcNow)
                throw new InvalidOperationException("Access token has expired. Please refresh or log in again.");

            return SessionState.Tokens.AccessToken;
        }

        private void ApplyRowColorsByRegesId()
        {
            if (dgvAddContracts?.Columns == null || dgvAddContracts.Rows.Count == 0) return;

            // find the idContract column
            DataGridViewColumn idCol = dgvAddContracts.Columns
                .Cast<DataGridViewColumn>()
                .FirstOrDefault(c =>
                    string.Equals(c.DataPropertyName, ContractIdPropertyName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Name, ContractIdPropertyName, StringComparison.OrdinalIgnoreCase));

            if (idCol == null) return;

            var green = Color.FromArgb(230, 255, 230);
            var greenSel = Color.FromArgb(210, 240, 210);
            var red = Color.FromArgb(255, 235, 235);
            var redSel = Color.FromArgb(240, 210, 210);

            foreach (DataGridViewRow row in dgvAddContracts.Rows)
            {
                if (row.IsNewRow) continue;

                var raw = row.Cells[idCol.Index].Value?.ToString();
                if (int.TryParse(raw, out var contractId))
                {
                    bool hasReges = _contractsWithRegesId.Contains(contractId);
                    row.DefaultCellStyle.BackColor = hasReges ? green : red;
                    row.DefaultCellStyle.SelectionBackColor = hasReges ? greenSel : redSel;
                }
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

        private void dgvAddContracts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

}
