using api_itm.Data;
using api_itm.Infrastructure.Mappers;
using api_itm.Infrastructure.Sessions;
using api_itm.Models.Employee;
using api_itm.Models.Reges;   // HeaderView
using api_itm.Models.View;    // EmployeeView
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static api_itm.Program;

namespace api_itm.UserControler.Employee
{
    public partial class ControlerEmployeeView : UserControl
    {
        private readonly AppDbContext _db;
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
        private const string InregistrareSalariatUrl = "api/Salariat";


        private const string QueueResultsUrl = "/api/queue/results";
        private const string QueueAckUrl = "/api/queue/ack";

        private static readonly JsonSerializerOptions _jsonOpts = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
        // ================================================================================================

        public ControlerEmployeeView(AppDbContext db, ISessionContext session)
        {
            InitializeComponent();
            _db = db;

            _session = session;
            BuildBarsAndLayout();
            WireGrid();
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
                Text = "Adaugare date salariati",
                AutoSize = true,
                Margin = new Padding(5, 0, 10, 6)
            };
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 14f, System.Drawing.FontStyle.Bold);
            topFlow.Controls.Add(lblTitle);
            topFlow.SetFlowBreak(lblTitle, true); // force next controls on a new row

            // Second row: Select-all + counter
            _chkSelectAll = new CheckBox
            {
                Text = "Selecteaza toati salariatii",
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
            };

            dgvViewSalariati.Sorted += (_, __) => RenumberRows();
            dgvViewSalariati.RowsAdded += (_, __) => { RenumberRows(); UpdateCounts(); };
            dgvViewSalariati.RowsRemoved += (_, __) => { RenumberRows(); UpdateCounts(); };

            dgvViewSalariati.CellDoubleClick += async (_, __) => await PreviewSelectedRowJsonAsync();
        }

        private async Task LoadEmployeesAsync()
        {
            // IMPORTANT: this uses the ENGLISH Person property names we standardized:
            // PersonId, SirutaCode, Address, NationalId, LastName, FirstName, BirthDate,
            // NationalityTypeId, DomicileCountryId, IdentityDocTypeId, IdTipApatrid,
            // HandicapTypeId, DisabilityGradeId, HandicapCertificateDate, HandicapCertificateNumber, Notes, WorkPermitStartDate, WorkPermitEndDate
            //var apatridTest = await _db.TypePapartide.ToListAsync();
            //foreach (var a in apatridTest)
            //{
            //    Debug.WriteLine($"ID: {a.IdTypePapartid}, Name: {a.PaPartidName}, Code: {a.CodPaPatrid}");
            //}

            //var testJoin = await (from p in _db.People
            //                      join ap in _db.TypePapartide
            //                          on p.IdTipApatrid equals ap.IdTypePapartid into apgrp
            //                      from ap in apgrp.DefaultIfEmpty()
            //                      select new
            //                      {
            //                          p.PersonId,
            //                          IdTipApatrid_Person = p.IdTipApatrid,
            //                          IdTipApatrid_Db = ap != null ? ap.IdTypePapartid : (int?)null,
            //                          PaPartidName = ap != null ? ap.PaPartidName : "null"
            //                      })
            //         .ToListAsync();
            //Debug.WriteLine("Test tippartit:" + testJoin);

            var rows =
                await (from p in _db.People

                       join n in _db.NationalityTypes
                            on p.NationalityTypeId equals n.NationalityTypeId into ngrp
                       from n in ngrp.DefaultIfEmpty()

                       join c in _db.Countries
                            on p.DomicileCountryId equals c.CountryId into cgrp
                       from c in cgrp.DefaultIfEmpty()

                       join a in _db.IdentityDocumentTypes
                            on p.IdentityDocTypeId equals a.IdentityDocumentTypeId into agrp
                       from a in agrp.DefaultIfEmpty()

                       join ap in _db.TypePapartide
                            on p.IdTipApatrid equals ap.IdTypePapartid into apgrp
                       from ap in apgrp.DefaultIfEmpty()

                       join th in _db.DisabilityTypes
                            on p.HandicapTypeId equals th.DisabilityTypeId into thgrp
                       from th in thgrp.DefaultIfEmpty()

                       join gh in _db.DisabilityGrades
                            on p.DisabilityGradeId equals gh.DisabilityGradeId into ghgrp
                       from gh in ghgrp.DefaultIfEmpty()

                       orderby p.PersonId
                       select new
                       {
                           personId = p.PersonId,                 // hidden
                           codSiruta = p.SirutaCode,
                           adresa = p.Address,
                           cnp = p.NationalId,
                           nume = p.LastName,
                           prenume = p.FirstName,
                           dataNastere = p.BirthDate,

                           nationalitate = n != null ? n.NationalityTypeName : null,
                           taraDomiciliu = c != null ? c.CountryName : null,
                           tipActIdentitate = a != null ? a.IdentityDocumentName : null,
                           apatrid = ap != null ? ap.PaPartidName : null,

                           dataInceputAutorizatie = p.WorkPermitStartDate,
                           dataSfarsitAutorizatie = p.WorkPermitEndDate,
                           tipAutorizatie = (string?)null,
                           tipAutorizatieExceptie = (string?)null,
                           numarAutorizatie = (string?)null,

                           tipHandicap = th != null ? th.DisabilityTypeName : null,
                           gradHandicap = gh != null ? gh.DisabilityGradeName : null,
                           dataCertificatHandicap = p.HandicapCertificateDate,
                           numarCertificatHandicap = p.HandicapCertificateNumber,
                           dataValabilitateCertificatHandicap = (DateTime?)null,

                           mentiuni = p.Notes,
                           motivRadiere = (string?)null
                       })
                      .AsNoTracking()
                      .ToListAsync();

            dgvViewSalariati.AutoGenerateColumns = true;
            dgvViewSalariati.DataSource = rows;
        }

        private void EnsureSpecialColumns()
        {
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

            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    col.Visible = false;
                    break;
                }
            }

            dgvViewSalariati.Columns[SelectColName].DisplayIndex = 0;
            dgvViewSalariati.Columns[RowNoColName].DisplayIndex = 1;
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

            // send one-by-one, sequentially (their API is per-message and you must wait the sync response)
            foreach (var personId in ids)
            {
                try
                {
                    await SendOneAsync(personId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Eroare trimitere personId={personId}: {ex.Message}");
                    // continue with the next one
                }
            }
        }

        private async Task PreviewSelectedRowJsonAsync()
        {
            if (dgvViewSalariati.CurrentRow == null) return;

            int? personId = GetPersonIdFromRow(dgvViewSalariati.CurrentRow);
            if (personId == null)
            {
                MessageBox.Show("Cannot detect PersonId for the selected row.");
                return;
            }

            await PreviewJsonForPerson(personId.Value);
        }

        private int? GetPersonIdFromRow(DataGridViewRow row)
        {
            foreach (DataGridViewColumn col in dgvViewSalariati.Columns)
            {
                if (string.Equals(col.DataPropertyName, PersonIdPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    var val = row.Cells[col.Index].Value;
                    if (val == null || val == DBNull.Value) return null;
                    if (int.TryParse(val.ToString(), out var pid)) return pid;
                }
            }
            return null;
        }

        private async Task PreviewJsonForPerson(int personId)
        {
            var payload = await BuildPayloadForPerson(personId);
            var json = JsonSerializer.Serialize(payload, _jsonOpts);
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

        // Designer stubs (safe to keep)
        private void ControlerEmployeeView_Load(object sender, DataGridViewCellEventArgs e) { }
        private void dgvViewSalariati_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        // =====================================================================
        // ======================= REGES API INTEGRATION ========================
        // =====================================================================

        // Step 0: Build payload from DB (uses exactly your mapper + names, no defaults)
        private async Task<EmployeeView> BuildPayloadForPerson(int personId)
        {
            var p = await _db.People.FirstOrDefaultAsync(x => x.PersonId == personId);
            if (p == null) throw new InvalidOperationException($"Person {personId} not found.");

            async Task<string> SafeLookup(Func<Task<string>> q) { try { return await q() ?? ""; } catch { return ""; } }

            // RAW names straight from DB (no enum/token mapping)
            var nationalitateName = await SafeLookup(() =>
                _db.NationalityTypes
                   .Where(n => n.NationalityTypeId == p.NationalityTypeId)
                   .Select(n => n.NationalityTypeName)
                   .FirstOrDefaultAsync());

            var taraDomiciliuName = await SafeLookup(() =>
                _db.Countries
                   .Where(c => c.CountryId == p.DomicileCountryId)
                   .Select(c => c.CountryName)
                   .FirstOrDefaultAsync());

            var tipActCodeRaw = await SafeLookup(() =>
                _db.IdentityDocumentTypes
                   .Where(a => a.IdentityDocumentTypeId == p.IdentityDocTypeId)
                   .Select(a => a.IdentityDocumentCode)
                   .FirstOrDefaultAsync());

            var apatridCodeRaw = await SafeLookup(() =>
                _db.TypePapartide
                   .Where(s => s.IdTypePapartid == p.IdTipApatrid)
                   .Select(s => s.CodPaPatrid)
                   .FirstOrDefaultAsync());

            Debug.WriteLine("apatridCodeRaw:" + apatridCodeRaw);
            var tipHandicapCodeRaw = await SafeLookup(() =>
                _db.DisabilityTypes
                   .Where(h =>
                       h.DisabilityTypeId == p.HandicapTypeId &&
                       h.DisabilityTypeId >= 1 &&
                       h.DisabilityTypeId <= 10)
                   .Select(h => h.DisabilityTypeCode)
                   .FirstOrDefaultAsync());

            Debug.WriteLine("tipHandicapCodeRaw:" + tipHandicapCodeRaw);


            var gradHandicapCodeRaw = await SafeLookup(() =>
                _db.DisabilityGrades
                   .Where(g => g.DisabilityGradeId == p.DisabilityGradeId)
                   .Select(g => g.DisabilityGradeCode)
                   .FirstOrDefaultAsync());

            var gradInvaliditateCodeRaw = await SafeLookup(() =>
                _db.DisabilityGrades
                   .Where(g => g.DisabilityGradeId == p.InvalidityGradeId)
                   .Select(g => g.DisabilityGradeCode)
                   .FirstOrDefaultAsync());

     
            if (p.InvalidityGradeId.HasValue)
                gradInvaliditateCodeRaw = $"Grad{p.InvalidityGradeId.Value}";  

            // Build info with **raw** strings
            var info = EmployeeMapper.FromPerson(
                p,
                nationalitateName,
                taraDomiciliuName,
                tipActCodeRaw,        // RAW
               // apatridCodeRaw,       // RAW
                tipHandicapCodeRaw,   // RAW
                gradHandicapCodeRaw,   // RAW
                gradInvaliditateCodeRaw
            );
             
            var payload = new EmployeeView
            {
                Type = "salariat",
                Header = new HeaderView
                {
                    MessageId = Guid.NewGuid().ToString("D"),
                    ClientApplication = "SAP",
                    Version = "1",
                    Operation = "InregistrareSalariat",
                    User = _session.UserName,
                    AuthorId = Properties.Settings.Default.SavedCredentialsUser,
                    SessionId = _session.SessionId,
                    Timestamp = DateTime.UtcNow
                },
                Info = info
            };

            Debug.WriteLine($"Built payload for personId={personId}: {JsonSerializer.Serialize(payload, _jsonOpts)}");

            return payload;
        }

        // Step 1: Send one payload -> get synchronous MessageResponse (recipisa)
        private async Task SendOneAsync(int personId)
        {
            var payload = await BuildPayloadForPerson(personId);
            var json = JsonSerializer.Serialize(payload, _jsonOpts);

            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await http.PostAsync(InregistrareSalariatUrl, content);
            Debug.WriteLine($"Sending to: {http.BaseAddress}{InregistrareSalariatUrl}");

            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {body}");
            else if (resp.IsSuccessStatusCode == true)
                Debug.WriteLine($"Succes {(int)resp.StatusCode}: {body}");

        var messageResponse = JsonSerializer.Deserialize<MessageResponse>(body, _jsonOpts)
                               ?? throw new InvalidOperationException("Empty sync response.");

            // Save sync receipt to DB (link it with your local personId)
            await SaveSyncReceiptAsync(personId, messageResponse);

            // Step 2: Poll for async result for this receiptId (as per their docs)
            var result = await PollForResultAsync(messageResponse.receiptId, CancellationToken.None);

            // Save result to DB
            await SaveAsyncResultAsync(personId, messageResponse.receiptId, result);

            // Step 3: ACK the processed result
            await AckAsync(result.messageId);
        }

        // Step 2a: Poll results queue until we see our receiptId
        private async Task<MessageResult> PollForResultAsync(string receiptId, CancellationToken ct)
        {
            // Basic polling loop; tune delay/backoff in real project.
            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var resp = await http.GetAsync(QueueResultsUrl, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Results HTTP {(int)resp.StatusCode}: {body}");

                var list = JsonSerializer.Deserialize<List<MessageResult>>(body, _jsonOpts) ?? new List<MessageResult>();

                // If queue is empty, wait a bit and try again
                if (list.Count == 0)
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                // try find the result for our receiptId
                var match = list.FirstOrDefault(r => string.Equals(r.receiptId, receiptId, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                    return match;

                // Not found yet — optionally ACK unrelated messages or just wait
                await Task.Delay(1000, ct);
            }
        }

        // Step 3: ACK a processed result
        private async Task AckAsync(string messageId)
        {
            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

            var ack = new AckRequest { messageId = messageId };
            var json = JsonSerializer.Serialize(ack, _jsonOpts);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var resp = await http.PostAsync(QueueAckUrl, content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"ACK HTTP {(int)resp.StatusCode}: {body}");
        }

        // ======= Helpers you plug into your DB layer =======
        private Task SaveSyncReceiptAsync(int personId, MessageResponse r)
        {
            // TODO: insert/update a table, e.g. RegesSync(personId, receiptId, messageId, createdAt)
            // r.receiptId, r.messageId, r.timestamp, r.status etc. — adapt to their real schema
            return Task.CompletedTask;
        }

        private Task SaveAsyncResultAsync(int personId, string receiptId, MessageResult r)
        {
            // TODO: insert/update a table, e.g. RegesResult(personId, receiptId, resultMessageId, success, code, description, receivedAt)
            return Task.CompletedTask;
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

        private void ControlerEmployeeView_Load(object sender, EventArgs e)
        {

        }
    }

    // ==================== DTOs to match REGES docs (minimal) ====================
    // Sync response after POST /api/salariat/inregistrare
    public sealed class MessageResponse
    {
        public string messageId { get; set; }   // server message id
        public string receiptId { get; set; }  // recipisa you must use to match results
        public string status { get; set; }      // e.g. "Accepted" (depends on their API)
        public DateTime? timestamp { get; set; }
    }

    // Async result pulled from /api/queue/results
    public sealed class MessageResult
    {
        public string messageId { get; set; }   // id you must ACK
        public string receiptId { get; set; }   // links back to MessageResponse.receiptId
        public bool success { get; set; }   // success/fail
        public string code { get; set; }   // e.g. created employee code
        public string description { get; set; } // error or info message
        public DateTime? timestamp { get; set; }
    }

    public sealed class AckRequest
    {
        public string messageId { get; set; }
    }
}
