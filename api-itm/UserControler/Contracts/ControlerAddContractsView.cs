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
using System.Net.Http;       // HttpClient
using System.Threading;      // CancellationToken

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
        static ControlerAddContractsView()
        {
            _jsonOpts.Converters.Add(new DateTimeMsConverter());
            _jsonOpts.Converters.Add(new NullableDateTimeMsConverter());
        }


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
            _btnSendSelected.Click += async (_, __) => await SendSelectedAsync();

            dgvAddContracts.CellDoubleClick += async (s, e) =>
            {
                if (e.RowIndex >= 0) // ignore header double-clicks
                    await PreviewSelectedRowJsonAsync();
            };
            dgvAddContracts.CurrentCellChanged += (_, __) => UpdateCounts();
            ResumeLayout();
        }

        private List<int> GetSelectedContractIds()
        {
            DataGridViewColumn? pidCol = null;
            foreach (DataGridViewColumn col in dgvAddContracts.Columns)
            {
                if (string.Equals(col.DataPropertyName, ContractIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    pidCol = col; break;
                }
            }
            if (pidCol == null) return new List<int>();

            var ids = new List<int>();
            foreach (DataGridViewRow row in dgvAddContracts.Rows)
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
        private async Task SaveSyncReceiptAsync(int idContract, SyncResponse sync)
        {
            Debug.WriteLine($"[DB] Save sync receipt for idContract={idContract}, messageId={sync?.header?.messageId}, responseId={sync?.responseId}");

            var rec = new RegesContractSync
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
                    // 1) Send
                    var sync = await SendOneAsync(idContract);

                    // 2) Save receipt
                    await SaveSyncReceiptAsync(idContract, sync);

                    // 3) Poll + update DB row
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

                    // 4) Read back this response and build result line
                    var ridGuid = CoerceGuid(sync?.responseId);
                    if (ridGuid.HasValue)
                    {
                        await using var db = await _dbFactory.CreateDbContextAsync();
                        var rec = await db.Set<RegesContractSync>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id_Rezultat_Mesaj.HasValue && x.Id_Rezultat_Mesaj.Value == ridGuid.Value);

                        if (rec != null &&
                            string.Equals(rec.Status, "Success", StringComparison.OrdinalIgnoreCase) &&
                            rec.RegesContractId.HasValue)
                        {
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

            //   Show the same reasons the console had, to the user:
            ShowSendSummary(lines, ids.Count);
            //   Reload the contracts grid  
            await LoadContractsAsync();
            RenumberRows();
            UpdateCounts(); 
            ApplyRowColorsByRegesId();

        }

        // Collect per-contract results for the final popup
        private sealed class SendLine
        {
            public int IdContract { get; init; }
            public bool Success { get; init; }
            public string Message { get; init; } = "";
            public Guid? RegesContractId { get; init; }
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

        // Add this inside ControlerAddContractsView (or a shared utils class)
        private static Guid? CoerceGuid(object? value)
        {
            if (value is null) return null;
            if (value is Guid g) return g;
            if (value is string s && Guid.TryParse(s, out var gs)) return gs;

            var text = value.ToString();
            return Guid.TryParse(text, out var g2) ? g2 : (Guid?)null;
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

            var rec = await db.Set<RegesContractSync>()
                .FirstOrDefaultAsync(x => x.Id_Rezultat_Mesaj.HasValue && x.Id_Rezultat_Mesaj.Value == rid);

            if (rec == null)
            {
                Debug.WriteLine($"[DB] No RegesContractSync row for responseId={responseId}");
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

            Debug.WriteLine($"[DB] Updated RegesContractSync: status={rec.Status}, RegesContractId={rec.RegesContractId}, code={code}");
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

            // Base set: only contracts whose person already has a REGES employee id
            var baseRows = await (
                from c in db.ContractsRu.AsNoTracking()
                join rs in db.Set<RegesSync>().AsNoTracking()
                     on c.PersonId equals rs.PersonId
                where rs.RegesEmployeeId != null
                orderby c.IdContract, (c.RecordDate ?? c.ModificationDate ?? c.RevisalTransmitDate)
                select new { C = c, RegesEmployeeId = rs.RegesEmployeeId }
            ).ToListAsync();

            // contracts that already have a REGES Contract Id (for green/red coloring)
            var greenContractIds = await db.Set<RegesContractSync>()
                .Where(r => r.RegesContractId != null && r.IdContract != null)
                .Select(r => r.IdContract!.Value)
                .Distinct()
                .ToListAsync();

            _contractsWithRegesId = new HashSet<int>(greenContractIds);

            // small helper to safely read single values
            async Task<string> Safe(Func<Task<string?>> q)
            {
                try { return (await q()) ?? ""; } catch { return ""; }
            }

            var rows = new List<object>(baseRows.Count);

            foreach (var it in baseRows)
            {
                var c = it.C;

                // Lookups (same logic/fields you already use in BuildContractPayloadAsync)
                var endDateExceptionCode = RegesJson.FixText(await Safe(() =>
                    db.EndDateExceptions
                      .Where(e => e.EndDateExceptionId == c.EndDateExceptionId)
                      .Select(e => e.EndDateExceptionCode)
                      .FirstOrDefaultAsync()));

                var isState7 = await db.ContractsState
                    .Where(s => s.ContractStateId == c.ContractStatusId)
                    .Select(s => s.ContractStateId == 7)
                    .FirstOrDefaultAsync();

                var educationLevelCode = await db.FunctionsStat
                    .Where(f => f.FunctionStatId == c.FunctionStatId)
                    .Join(db.EducationLevels, f => f.EducationLevelId, e => e.EducationLevelId,
                          (f, e) => e.EducationLevelCodeReges)
                    .FirstOrDefaultAsync();

                var workingScheduleCode = await db.WorkScheduleNorm
                   .Where(w => w.WorkScheduleId == c.NormId)
                   .Select(w => w.WorkScheduleCode)
                   .FirstOrDefaultAsync();

                var workingTimeIntervalCode = await db.WorkingTimeIntervals
                   .Where(wt => wt.WorkingTimeIntervalId == c.WorkTimeIntervalId)
                   .Select(wt => wt.WorkingTimeIntervalCode)
                   .FirstOrDefaultAsync();

                var repartizareMunca = await db.WorkDistributionId
                   .Where(wd => wd.WorkDistributionIdId == c.WorkDistributionId)
                   .Select(wd => wd.WorkDistributionIdCode)
                   .FirstOrDefaultAsync();

                var repartizare = await db.WorkTimeAllocation
                  .Where(wta => wta.WorkTimeAllocationId == c.WorkTimeAllocationId)
                  .Select(wta => wta.WorkTimeAllocationCode)
                  .FirstOrDefaultAsync();

                var typeContractRuCode = await db.TypeContractRu
                 .Where(tyr => tyr.TypeContractRuId == c.ContractTypeId)
                 .Select(tyr => tyr.TypeContractRuCode)
                 .FirstOrDefaultAsync();

                var typeContractDurationCode = await db.ContractTypeDuration
                 .Where(tcd => tcd.ContractTypeDurationId == c.DurationTypeId)
                 .Select(tcd => tcd.ContractTypeDurationCode)
                 .FirstOrDefaultAsync();

                Debug.WriteLine("typeContractDurationCode" + typeContractDurationCode);

                var shiftTypeCode = await db.ShiftType
                 .Where(shift => shift.ShiftTypeId == c.ShiftTypeId)
                 .Select(shift => shift.ShiftTypeCode)
                 .FirstOrDefaultAsync();

                Debug.WriteLine("shiftTypeCode" + shiftTypeCode);

                var workNormTypeCode = await db.WorkNormType
                 .Where(n => n.WorkNormTypeId == c.WorkNormTypeId)
                 .Select(n => n.WorkNormTypeCode)
                 .FirstOrDefaultAsync();

              
                Debug.WriteLine("workNormTypeCode" + workNormTypeCode);

                // Time window
                DateTime? inceputInterval = null, sfarsitInterval = null;
                if (c.StartDate.HasValue && c.StartHour.HasValue)
                    inceputInterval = c.StartDate.Value.Date + c.StartHour.Value;
                if (c.StartDate.HasValue && c.EndHour.HasValue)
                    sfarsitInterval = c.StartDate.Value.Date + c.EndHour.Value;

                // Salary like in payload (rounded to int)
                int? salariu = c.GrossSalary.HasValue
                    ? Convert.ToInt32(Math.Round(c.GrossSalary.Value, 0, MidpointRounding.AwayFromZero))
                    : (int?)null;

                // Shape grid rows with the SAME names you use in the payload/debug
                rows.Add(new
                {
                    idContract = c.IdContract,
                    numarContract = c.ContractNumber,
                    dataConsemnare = c.RecordDate,
                    dataContract = c.ContractDate,
                    dataInceputContract = c.StartDate,

                    exceptieDataSfarsit = string.IsNullOrWhiteSpace(endDateExceptionCode) ? null : endDateExceptionCode,
                    radiat = isState7,
                    salariu = salariu,
                    moneda = "RON",
                    nivelStudii = educationLevelCode,

                    norma = workingScheduleCode,
                    durata = c.ContractDuration,
                    intervalTimp = workingTimeIntervalCode,
                    repartizare = repartizareMunca,
                    repartizareMunca = repartizare,
                    inceputInterval,
                    sfarsitInterval,
                    tipTura = shiftTypeCode,

                    tipContract = typeContractRuCode,
                    tipDurata = typeContractDurationCode,
                    tipNorma = workNormTypeCode,

                    tipLocMunca =  "Mobil", //to be completed to do
                    judetLocMunca = "CL",  //To DO 
                    regesEmployeeId = it.RegesEmployeeId  
                });
            }

            dgvAddContracts.AutoGenerateColumns = true;
            dgvAddContracts.DataSource = rows;

            // keep search/sort helpers working
            _allRowsData = rows;
            _rowsData = rows;
            _rowItemType = rows.Count > 0 ? rows[0].GetType() : null;

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

        public async Task BindContractsGridAsync(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();

            void AddText(string dataProperty, string header, int fillWeight = 100)
            {
                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = dataProperty,
                    HeaderText = header,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                    FillWeight = fillWeight,
                    ReadOnly = true
                });
            }

            // Columns derived from payload/lookups (PersonId intentionally NOT added)
            AddText(nameof(ContractGridRow.IdContract), "Id");
            AddText(nameof(ContractGridRow.NumarContract), "Număr");
            AddText(nameof(ContractGridRow.DataConsemnare), "Data Consemnare");
            AddText(nameof(ContractGridRow.DataContract), "Data Contract");
            AddText(nameof(ContractGridRow.DataInceputContract), "Data Început");

            AddText(nameof(ContractGridRow.ExceptieDataSfarsit), "Exceptie Sfârșit");
            AddText(nameof(ContractGridRow.Radiat), "Radiat");
            AddText(nameof(ContractGridRow.Salariu), "Salariu");
            AddText(nameof(ContractGridRow.Moneda), "Monedă");
            AddText(nameof(ContractGridRow.NivelStudii), "Nivel Studii");

            AddText(nameof(ContractGridRow.Norma), "Norma");
            AddText(nameof(ContractGridRow.Durata), "Durata");
            AddText(nameof(ContractGridRow.IntervalTimp), "Interval Timp");
            AddText(nameof(ContractGridRow.Repartizare), "Repartizare");
            AddText(nameof(ContractGridRow.RepartizareMunca), "Repartizare Muncă");
            AddText(nameof(ContractGridRow.InceputInterval), "Început Interval");
            AddText(nameof(ContractGridRow.SfarsitInterval), "Sfârșit Interval");
            AddText(nameof(ContractGridRow.TipTura), "Tip Tură");

            AddText(nameof(ContractGridRow.TipContract), "Tip Contract");
            AddText(nameof(ContractGridRow.TipDurata), "Tip Durată");
            AddText(nameof(ContractGridRow.TipNorma), "Tip Normă");
            AddText(nameof(ContractGridRow.TipLocMunca), "Tip Loc Muncă");

            AddText(nameof(ContractGridRow.RegesEmployeeId), "REGES Ref");

            // Bind
            var data = await LoadContractGridRowsAsync();
            dgv.DataSource = new BindingList<ContractGridRow>(data);
        }

        private async Task<List<ContractGridRow>> LoadContractGridRowsAsync()
        {
            await using var _db = await _dbFactory.CreateDbContextAsync();

            // Pull contracts joined with REGES employee reference (filter by reference id, not by showing PersonId)
            var baseRows = await (
                from c in _db.ContractsRu.AsNoTracking()
                join rs in _db.Set<RegesSync>().AsNoTracking()
                     on c.PersonId equals rs.PersonId
                where rs.RegesEmployeeId != null
                select new { c, rs.RegesEmployeeId }
            )
            .OrderBy(x => x.c.IdContract)
            .ToListAsync();

            // Helper: safe single-value lookup
            async Task<string> Safe(Func<Task<string?>> q)
            {
                try { return (await q()) ?? ""; } catch { return ""; }
            }

            var rows = new List<ContractGridRow>(baseRows.Count);

            foreach (var item in baseRows)
            {
                var c = item.c;

                // Lookups (same as in BuildContractPayloadAsync)
                var endDateExceptionCode = RegesJson.FixText(await Safe(() =>
                    _db.EndDateExceptions
                       .Where(e => e.EndDateExceptionId == c.EndDateExceptionId)
                       .Select(e => e.EndDateExceptionCode)
                       .FirstOrDefaultAsync()));

                var isState7 = await _db.ContractsState
                    .Where(s => s.ContractStateId == c.ContractStatusId)
                    .Select(s => s.ContractStateId == 7)
                    .FirstOrDefaultAsync();

                var educationLevelCode = await _db.FunctionsStat
                    .Where(f => f.FunctionStatId == c.FunctionStatId)
                    .Join(_db.EducationLevels, f => f.EducationLevelId, e => e.EducationLevelId,
                          (f, e) => e.EducationLevelCodeReges)
                    .FirstOrDefaultAsync();

                var workingScheduleCode = await _db.WorkScheduleNorm
                    .Where(w => w.WorkScheduleId == c.NormId)
                    .Select(w => w.WorkScheduleCode)
                    .FirstOrDefaultAsync();

                var workingTimeIntervalCode = await _db.WorkingTimeIntervals
                    .Where(wt => wt.WorkingTimeIntervalId == c.WorkTimeIntervalId)
                    .Select(wt => wt.WorkingTimeIntervalCode)
                    .FirstOrDefaultAsync();

                var repartizareMunca = await _db.WorkDistributionId
                    .Where(wd => wd.WorkDistributionIdId == c.WorkDistributionId)
                    .Select(wd => wd.WorkDistributionIdCode)
                    .FirstOrDefaultAsync();

                var repartizare = await _db.WorkTimeAllocation
                    .Where(wta => wta.WorkTimeAllocationId == c.WorkTimeAllocationId)
                    .Select(wta => wta.WorkTimeAllocationCode)
                    .FirstOrDefaultAsync();

                var typeContractRuCode = await _db.TypeContractRu
                    .Where(tyr => tyr.TypeContractRuId == c.ContractTypeId)
                    .Select(tyr => tyr.TypeContractRuCode)
                    .FirstOrDefaultAsync();

                var typeContractDurationCode = await _db.ContractTypeDuration
                    .Where(tcd => tcd.ContractTypeDurationCode == c.DurationTypeId.ToString())
                    .Select(tcd => tcd.ContractTypeDurationCode)
                    .FirstOrDefaultAsync();

                var shiftTypeCode = await _db.ShiftType
                    .Where(shift => shift.ShiftTypeId == c.ShiftTypeId)
                    .Select(shift => shift.ShiftTypeCode)
                    .FirstOrDefaultAsync();

                var workNormTypeCode = await _db.WorkNormType
                    .Where(n => n.WorkNormTypeId == c.NormId)
                    .Select(n => n.WorkNormTypeCode)
                    .FirstOrDefaultAsync();

                // Time window
                DateTime? inceputInterval = null, sfarsitInterval = null;
                if (c.StartDate.HasValue && c.StartHour.HasValue)
                    inceputInterval = c.StartDate.Value.Date + c.StartHour.Value;
                if (c.StartDate.HasValue && c.EndHour.HasValue)
                    sfarsitInterval = c.StartDate.Value.Date + c.EndHour.Value;

                // Salary (rounded as in payload)
                int? salariu = c.GrossSalary.HasValue
                    ? Convert.ToInt32(Math.Round(c.GrossSalary.Value, 0, MidpointRounding.AwayFromZero))
                    : (int?)null;

                rows.Add(new ContractGridRow
                {
                    IdContract = c.IdContract,
                    NumarContract = c.ContractNumber,
                    DataConsemnare = c.RecordDate,
                    DataContract = c.ContractDate,
                    DataInceputContract = c.StartDate,

                    ExceptieDataSfarsit = string.IsNullOrWhiteSpace(endDateExceptionCode) ? null : endDateExceptionCode,
                    Radiat = isState7,
                    Salariu = salariu,
                    Moneda = "RON",
                    NivelStudii = educationLevelCode,

                    Norma = workingScheduleCode,
                    Durata = c.ContractDuration.HasValue ? Convert.ToInt32(c.ContractDuration.Value) : (int?)null,
                    IntervalTimp = workingTimeIntervalCode,
                    Repartizare = repartizareMunca,
                    RepartizareMunca = repartizare,
                    InceputInterval = inceputInterval,
                    SfarsitInterval = sfarsitInterval,
                    TipTura = shiftTypeCode,

                    TipContract = typeContractRuCode,
                    TipDurata = typeContractDurationCode,
                    TipNorma = workNormTypeCode,

                    TipLocMunca = c.Headquarters,
                    RegesEmployeeId = item.RegesEmployeeId
                });
            }

            return rows;
        }


        // Step 0: Build payload from DB (uses exactly your mapper + names, no defaults)
        public async Task<ContractEnvelope> BuildContractPayloadAsync(int idContract)
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
                      (f, e) => e.EducationLevelCodeReges)
                .FirstOrDefaultAsync();

            var workingscheduleCode = await _db.WorkScheduleNorm
               .Where(w => w.WorkScheduleId == c.NormId)
               .Select(w => w.WorkScheduleCode)
               .FirstOrDefaultAsync();

            var workingTimeIntervalCode = await _db.WorkingTimeIntervals
               .Where(wt => wt.WorkingTimeIntervalId == c.WorkTimeIntervalId)
               .Select(wt => wt.WorkingTimeIntervalCode)
               .FirstOrDefaultAsync();

            var repartizareMunca = await _db.WorkDistributionId
               .Where(wd => wd.WorkDistributionIdId == c.WorkDistributionId)
               .Select(wd => wd.WorkDistributionIdCode)
               .FirstOrDefaultAsync(); //WorkDistributionIds

            Debug.WriteLine("repartizareMunca: " + repartizareMunca);

            var repartizare = await _db.WorkTimeAllocation
              .Where(wta => wta.WorkTimeAllocationId == c.WorkTimeAllocationId)
              .Select(wta => wta.WorkTimeAllocationCode)
              .FirstOrDefaultAsync(); //ShiftType

            Debug.WriteLine("repartizare:" + repartizare);

            var typeContractRuCode = await _db.TypeContractRu
             .Where(tyr => tyr.TypeContractRuId == c.ContractTypeId)
             .Select(tyr => tyr.TypeContractRuCode)
             .FirstOrDefaultAsync();//TypeContractRu

            var typeContractDurationCode = await ResolveContractTypeDurationCodeAsync(_db, c.DurationTypeId);


            Debug.WriteLine("typeContractDurationCode" + typeContractDurationCode);


            var shiftTypeCode = await _db.ShiftType
             .Where(shift => shift.ShiftTypeId == c.ShiftTypeId)
             .Select(shift => shift.ShiftTypeCode)
             .FirstOrDefaultAsync(); //ShiftType

            Debug.WriteLine("shiftTypeCode" + shiftTypeCode);

            var workNormTypeCode = await _db.WorkNormType
                  .Where(n => n.WorkNormTypeId == c.WorkNormTypeId)
                  .Select(n => n.WorkNormTypeCode)
                  .FirstOrDefaultAsync();

            Debug.WriteLine("workNormTypeCode" + workNormTypeCode);

            // Find REGES employee id for this contract's person
            var regesEmployeeGuid = await _db.Set<RegesSync>()
                .AsNoTracking()
                .Where(rs => rs.PersonId == c.PersonId && rs.RegesEmployeeId != null)
                .OrderByDescending(rs => rs.UpdatedAt)
                .Select(rs => rs.RegesEmployeeId)
                .FirstOrDefaultAsync();

            var Code = await _db.WorkNormType
               .Where(n => n.WorkNormTypeId == c.WorkNormTypeId)
               .Select(n => n.WorkNormTypeCode)
               .FirstOrDefaultAsync();

            var worktypelocationtype = await _db.WorkLocationType
              .Where(wlt => wlt.WorkLocationTypeId == c.WorkTypeID)
              .Select(wlt => wlt.WorkLocationTypeCode)
              .FirstOrDefaultAsync();

            var versionCor = await _db.RegesCor
              .Where(cor => cor.Code.ToString() == c.OccupationCode)
              .Select(cor => cor.Version)  //c.OccupationCode
              .FirstOrDefaultAsync();

            Debug.WriteLine("workNormTypeCode" + workNormTypeCode);
            var salariatRefId = regesEmployeeGuid?.ToString("D") ?? "";
            if (string.IsNullOrWhiteSpace(salariatRefId))
                throw new InvalidOperationException($"No REGES employee Id for person {c.PersonId}; cannot build contract.");

            Debug.WriteLine($"[DEBUG] Contract {c.IdContract} | FunctionStatId={c.FunctionStatId} => EducationLevelCode='{educationLevelCode}'");


            // 3b) SPORURI
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
            DateTime? startDate = c.StartDate;   // din contracte_ru
            TimeSpan? startHour = c.StartHour;   // din contracte_ru.ora_inceput
            TimeSpan? endHour = c.EndHour;     // din contracte_ru.ora_sfarsit

            DateTime? inceputInterval = (startDate.HasValue && startHour.HasValue)
                ? startDate.Value.Date + startHour.Value
                : (DateTime?)null;

            DateTime? sfarsitInterval = (startDate.HasValue && endHour.HasValue)
                ? startDate.Value.Date + endHour.Value
                : (DateTime?)null;

            // pentru vizibilitate în Output:
            Debug.WriteLine($"[DEBUG] IdContract={c.IdContract} StartDate={startDate:yyyy-MM-dd} " +
                            $"StartHour={(startHour.HasValue ? startHour.Value.ToString(@"hh\:mm") : "<null>")} " +
                            $"EndHour={(endHour.HasValue ? endHour.Value.ToString(@"hh\:mm") : "<null>")} " +
                            $"=> inceputInterval={inceputInterval:yyyy-MM-ddTHH:mm:ss.fff} " +
                            $"sfarsitInterval={sfarsitInterval:yyyy-MM-ddTHH:mm:ss.fff}");
             
            // 5) COR parse
            static int TryParseInt(string? s) => int.TryParse(s, out var n) ? n : 0;

            // 6) Build payload
            var continut = new ContinutContract
            {
                ReferintaSalariat = new Models.Contracts.Envelope.ReferintaSalariat { Id = salariatRefId },

                Cor = new Cor
                {
                    Cod = TryParseInt(c.OccupationCode),
                    Versiune = versionCor
                },

                DataConsemnare = c.RecordDate,
                DataContract = c.ContractDate,
                DataInceputContract = c.StartDate,
                DataSfarsitContract = c.EndDate,
                ExceptieDataSfarsit = string.IsNullOrWhiteSpace(endDateExceptionCode) ? null : endDateExceptionCode,

                NumarContract = c.ContractNumber ?? "",
                Radiat = isState7,

                Salariu = c.GrossSalary.HasValue
                    ? (int?)Convert.ToInt32(Math.Round(c.GrossSalary.Value, 0, MidpointRounding.AwayFromZero))
                    : null,
                Moneda = "RON",
                NivelStudii = educationLevelCode,

                // 🔧 Only change: omit when empty to satisfy schema
                SporuriSalariu = (sporuri.Count > 0) ? sporuri : null,

                StareCurenta = new StareCurenta(),

                TimpMunca = new TimpMunca
                {
                    Norma = workingscheduleCode,
                    Durata = c.ContractDuration.HasValue ? (int?)Convert.ToInt32(c.ContractDuration.Value) : null,
                    IntervalTimp = workingTimeIntervalCode,
                    Repartizare = repartizare,  //
                    RepartizareMunca = repartizareMunca, //repartizareMunca
                    InceputInterval = inceputInterval,
                    SfarsitInterval = sfarsitInterval,
                    TipTura = shiftTypeCode,
                },

                TipContract = typeContractRuCode,
                TipDurata = typeContractDurationCode,
                TipNorma = workNormTypeCode,
                TipLocMunca =  "Mobil", // worktypelocationtype, //hardcode, to !!!
                JudetLocMunca = "CJ", //do

                AplicaL153 = null,

                DetaliiL153 = new DetaliiL153 {
                    // AnexaL153 = c.L153AnnexCode,
                    // CapitolL153 = c.L153CapitolCode,
                    // LiteraL153 = c.L153LiteraCode,
                    // ClasificareSuplimentaraL153 = c.L153ClasifCode,
                    // FunctieL153 = c.L153FunctieCode,
                    // SpecialitateFunctieL153 = c.L153SpecFunctieCode,
                    // StructuraAprobataL153 = c.L153StructuraCode,
                    // SpecialitateStructuraAprobataL153 = c.L153SpecStructuraCode,
                    // GradProfesionalL153 = c.L153GradProfCode,
                    // GradatieL153 = c.L153GradatieCode,
                    // DenumireAltaFunctieL153 = c.L153AltaFunctieName,
                    // ExplicatieFunctieL153 = c.L153ExplicatieFunctie,
                    // AltGradProfesionalL153 = c.L153AltGradProfText
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

        private static async Task<string?> ResolveContractTypeDurationCodeAsync(
    AppDbContext db,
    int? durationTypeId,
    bool verbose = true,
    bool listAllOnMiss = true)
        {
            var idText = durationTypeId?.ToString();

            if (verbose)
                Debug.WriteLine($"[CTD] DurationTypeId={durationTypeId}  as string='{idText}'");

            // 1) Prefer match by Id (most reliable)
            var qById = db.ContractTypeDuration
                .AsNoTracking()
                .Where(t => t.ContractTypeDurationId == durationTypeId)
                .Select(t => new { t.ContractTypeDurationId, t.ContractTypeDurationCode })
                .TagWith("CTD Resolve: by Id");

            if (verbose)
            {
                Debug.WriteLine("[CTD] SQL (by Id):");
                Debug.WriteLine(qById.ToQueryString()); // EF Core 5+
            }

            var row = await qById.FirstOrDefaultAsync();
            if (row != null)
            {
                if (verbose)
                    Debug.WriteLine($"[CTD] MATCH by Id => Id={row.ContractTypeDurationId}, Code='{row.ContractTypeDurationCode}'");
                return row.ContractTypeDurationCode;
            }

            // 2) Fallback: match by Code == DurationTypeId.ToString()
            var qByCode = db.ContractTypeDuration
                .AsNoTracking()
                .Where(t => t.ContractTypeDurationCode == idText)
                .Select(t => new { t.ContractTypeDurationId, t.ContractTypeDurationCode })
                .TagWith("CTD Resolve: by Code == DurationTypeId.ToString()");

            if (verbose)
            {
                Debug.WriteLine("[CTD] SQL (by Code):");
                Debug.WriteLine(qByCode.ToQueryString());
            }

            var row2 = await qByCode.FirstOrDefaultAsync();
            if (row2 != null)
            {
                if (verbose)
                    Debug.WriteLine($"[CTD] MATCH by Code => Id={row2.ContractTypeDurationId}, Code='{row2.ContractTypeDurationCode}'");
                return row2.ContractTypeDurationCode;
            }

            // 3) Nothing matched — list available rows to eyeball mismatches (padding, etc.)
            if (listAllOnMiss)
            {
                var all = await db.ContractTypeDuration
                    .AsNoTracking()
                    .OrderBy(x => x.ContractTypeDurationId)
                    .Select(x => new { x.ContractTypeDurationId, x.ContractTypeDurationCode })
                    .ToListAsync();

                Debug.WriteLine("[CTD] No match. Available ContractTypeDuration rows:");
                foreach (var r in all)
                    Debug.WriteLine($"  Id={r.ContractTypeDurationId}, Code='{r.ContractTypeDurationCode}'");
            }

            return null;
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

            if (mustValidate && !GuardRequiredFieldsOrWarn(payload.Continut, idContract))
                throw new InvalidOperationException("Câmpuri obligatorii lipsă – trimitere blocată.");


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
        // Put this in your control/form class (where SendSelectedAsync lives)
        private static (bool IsValid, List<string> Missing) ValidateRequiredForContract(ContinutContract cont)
        {
            var missing = new List<string>();

            // Helper for date display
            static string D(DateTime? dt) => dt?.ToString("yyyy-MM-dd") ?? "<null>";

            // Required always
            if (cont == null) { missing.Add("Conținut contract (null)"); return (false, missing); }
            if (cont.Cor == null || cont.Cor.Cod <= 0) missing.Add("COR.cod");
            if (!cont.DataConsemnare.HasValue) missing.Add("dataConsemnare");
            if (!cont.DataContract.HasValue) missing.Add("dataContract");
            if (!cont.DataInceputContract.HasValue) missing.Add("dataInceputContract");
            if (string.IsNullOrWhiteSpace(cont.NumarContract)) missing.Add("numarContract");

            // salariu (int?) required by cerința ta
            if (!cont.Salariu.HasValue || cont.Salariu.Value <= 0) missing.Add("salariu");

            // tipContract, tipDurata, tipNorma, tipLocMunca – always check
            if (string.IsNullOrWhiteSpace(cont.TipContract)) missing.Add("tipContract");
            if (string.IsNullOrWhiteSpace(cont.TipDurata)) missing.Add("tipDurata");
            if (string.IsNullOrWhiteSpace(cont.TipNorma)) missing.Add("tipNorma");
            if (string.IsNullOrWhiteSpace(cont.TipLocMunca)) missing.Add("tipLocMunca");

            // judetLocMunca – Obligatoriu pentru contractele adăugate/modificate cu dataConsemnare după 01.04.2025
            var cutoff = new DateTime(2025, 4, 1);
            var needsCounty = cont.DataConsemnare.HasValue && cont.DataConsemnare.Value.Date >= cutoff;
            if (needsCounty && string.IsNullOrWhiteSpace(cont.JudetLocMunca))
                missing.Add("judetLocMunca (obligatoriu pentru dataConsemnare ≥ 2025-04-01)");

            // Optionally: validate TimpMunca subfields if you want (left as-is since nu ai cerut)

            return (missing.Count == 0, missing);
        }

        private bool GuardRequiredFieldsOrWarn(ContinutContract cont, int idContract)
        {
            var (ok, missing) = ValidateRequiredForContract(cont);
            if (ok) return true;

            var msg =
                "Nu pot trimite contractul deoarece lipsesc câmpuri obligatorii:\n" +
                string.Join("\n", missing.Select(m => " • " + m)) +
                $"\n\nIdContract: {idContract}\n" +
                $"DataConsemnare: {(cont?.DataConsemnare?.ToString("yyyy-MM-dd") ?? "<null>")}";

            MessageBox.Show(msg, "REGES – câmpuri obligatorii lipsă",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return false;
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

        sealed class DateTimeMsConverter : JsonConverter<DateTime>
        {
            private const string F = "yyyy-MM-dd'T'HH:mm:ss.fff";
            public override DateTime Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
                => DateTime.Parse(r.GetString()!);
            public override void Write(Utf8JsonWriter w, DateTime v, JsonSerializerOptions o)
                => w.WriteStringValue(v.ToString(F));
        }

        sealed class NullableDateTimeMsConverter : JsonConverter<DateTime?>
        {
            private const string F = "yyyy-MM-dd'T'HH:mm:ss.fff";
            public override DateTime? Read(ref Utf8JsonReader r, Type t, JsonSerializerOptions o)
                => r.TokenType == JsonTokenType.Null ? (DateTime?)null : DateTime.Parse(r.GetString()!);
            public override void Write(Utf8JsonWriter w, DateTime? v, JsonSerializerOptions o)
            {
                if (v.HasValue) w.WriteStringValue(v.Value.ToString(F));
                else w.WriteNullValue();
            }
        }

       

    }

}
