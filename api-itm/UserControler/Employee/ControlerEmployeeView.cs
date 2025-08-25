using api_itm.Data;
using api_itm.Data.Entity;
using api_itm.Data.Entity.Ru;   
using api_itm.Infrastructure.Mappers;
using api_itm.Infrastructure.Sessions;
using api_itm.Infrastructure;

using api_itm.Models.Employee;
using api_itm.Models.Reges;      
using api_itm.Models.Reges;   // HeaderView
using api_itm.Models.Reges;      
using api_itm.Models.View;    // EmployeeView
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.ApplicationServices;
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

        private const string PollMessageUrl = "api/Status/PollMessage";   // POST, no body
        private const string ReadMessageUrl = "api/Status/ReadMessage";   // POST, no body
        private const string CommitReadUrl = "api/Status/CommitRead";    // POST, no body


        private HashSet<int> _personsWithRegesId = new HashSet<int>();


        private const string ConsumerId = "winforms-dev-1"; // sau string.Empty pentru implicit


        private static string Trunc(string s, int max = 600)
    => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s.Substring(0, max) + "...[truncated]");

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

        private async Task LoadEmployeesAsync()
        {
            // IMPORTANT: this uses the ENGLISH Person property names we standardized:
            // PersonId, SirutaCode, Address, NationalId, LastName, FirstName, BirthDate,
            // NationalityTypeId, DomicileCountryId, IdentityDocTypeId, IdTipApatrid,
            // HandicapTypeId, DisabilityGradeId, HandicapCertificateDate, HandicapCertificateNumber, Notes, WorkPermitStartDate, WorkPermitEndDate
            //var apatridTest = await _db.Countries.ToListAsync();
            //foreach (var a in apatridTest)
            //{
            //    Debug.WriteLine($"ID: {a.IdTypePapartid}, Name: {a.PaPartidName}, Code: {a.CodPaPatrid}");
            //}

            var countries = await _db.Countries.ToListAsync();

            foreach (var country in countries)
            {
                Debug.WriteLine($@" --- COUNTRY --- ID: {country.CountryId},Name: {country.CountryName},Type: {country.CountryType}  ,Revisal Name: {country.CountryNameRevisal}");
            }


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

                       join c in _db.Countries
                            on p.DomicileCountryId equals c.CountryId into cgrp
                       from c in cgrp.DefaultIfEmpty()

                       join n in _db.NationalityTypes
                          on c.CountryId equals n.NationalityTypeId into ngrp
                       from n in ngrp.DefaultIfEmpty()

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
                           Nationalitate = RegesJson.FixText(c.CountryNameRevisal),
                           TaraDomiciliu = RegesJson.FixText((c.CountryNameRevisal)) ,


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

            var withRegesNullable = await _db.Set<RegesSync>()
      .Where(r => r.RegesEmployeeId.HasValue)
      .Select(r => r.PersonId)          // List<int?>
      .Distinct()
      .ToListAsync();

            _personsWithRegesId = new HashSet<int>(
                withRegesNullable.Where(pid => pid.HasValue)
                                 .Select(pid => pid.Value)
            );


            dgvViewSalariati.AutoGenerateColumns = true;
            dgvViewSalariati.DataSource = rows;

            // 👇 ADD THIS after DataSource
            ApplyRowColorsByRegesId();
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
                    // hide the PersonId column from the UI
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

            int ok = 0, fail = 0;

            foreach (var personId in ids)
            {
                try
                {
                    // 1) Send to API
                    var sync = await SendOneAsync(personId);

                    // 2) Save sync receipt (same row we’ll later update)
                    await SaveSyncReceiptAsync(personId, sync);

                    // 3) Poll + update that same row (sets Status, ErrorMessage, RegesEmployeeId)
                    await PollForResultAndUpdateAsync(sync.responseId, CancellationToken.None);

                    // 4) Read back the row for this responseId and log result
                    if (Guid.TryParse(sync.responseId, out var rid))
                    {
                        var rec = await _db.Set<RegesSync>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.MessageResultId.HasValue && x.MessageResultId.Value == rid);

                        var p = await _db.People
                            .Where(x => x.PersonId == personId)
                            .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                            .FirstOrDefaultAsync();

                        if (rec != null && string.Equals(rec.Status, "Success", StringComparison.OrdinalIgnoreCase) && rec.RegesEmployeeId.HasValue)
                        {
                            ok++;
                            Debug.WriteLine($"[REGES OK] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | salariatId={rec.RegesEmployeeId}");
                        }
                        else
                        {
                            fail++;
                            var errMsg = rec?.ErrorMessage ?? "Unknown error";
                            Debug.WriteLine($"[REGES FAIL] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | error={errMsg}");
                        }
                    }
                    else
                    {
                        fail++;
                        var p = await _db.People
                            .Where(x => x.PersonId == personId)
                            .Select(x => new { x.LastName, x.FirstName, x.NationalId })
                            .FirstOrDefaultAsync();

                        Debug.WriteLine($"[REGES FAIL] personId={personId} | name={p?.LastName} {p?.FirstName} | cnp={p?.NationalId} | error=Invalid responseId '{sync.responseId}'");
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
                    // keep going
                }
            }

            // final summary in Output window
            Debug.WriteLine($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");
            MessageBox.Show($"[REGES SUMMARY] total={ids.Count} ok={ok} fail={fail}");
            // optional UI summary:
            // MessageBox.Show($"Trimise: {ids.Count}\nSucces: {ok}\nErori: {fail}", "Rezultat trimitere");
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

        //private async Task PreviewJsonForPerson(int personId)
        //{
        //    var payload = await BuildPayloadForPerson(personId);
        //    var json = JsonSerializer.Serialize(payload, _jsonOpts);
        //    ShowJsonPreview(json);
        //}

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
            // RAW names -> FIX THEM here
            var nationalitateName = RegesJson.FixText(await SafeLookup(() =>
    _db.NationalityTypes.Where(n => n.NationalityTypeId == p.NationalityTypeId)
       .Select(n => n.NationalityTypeName).FirstOrDefaultAsync()));

            var taraDomiciliuName = RegesJson.FixText(await SafeLookup(() =>
                _db.Countries.Where(c => c.CountryId == p.DomicileCountryId)
                   .Select(c => c.CountryName).FirstOrDefaultAsync()));



            var tipActCodeRaw = RegesJson.FixText((await SafeLookup(() =>
                _db.IdentityDocumentTypes
                   .Where(a => a.IdentityDocumentTypeId == p.IdentityDocTypeId)
                   .Select(a => a.IdentityDocumentCode)
                   .FirstOrDefaultAsync())  ));

             nationalitateName = RegesJson.Norma(taraDomiciliuName);

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
            Debug.WriteLine($"[SEND] Status={(int)resp.StatusCode} Body={Trunc(body)}");

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {body}");

            var sync = JsonSerializer.Deserialize<SyncResponse>(body, _jsonOpts)
                       ?? throw new InvalidOperationException("Empty sync response.");

            Debug.WriteLine($"[SYNC OK] personId={personId}, responseId={sync.responseId}, messageId={sync.header?.messageId}");

            return sync;
        }


        private async Task<MessageResult?> PollForResultAsync(Guid expectedReceiptId, CancellationToken ct)
        {
            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            var url = WithConsumer(PollMessageUrl);
            Debug.WriteLine($"[POLL] Using {http.BaseAddress}{url}");
            Debug.WriteLine($"[POLL] Expecting receiptId={expectedReceiptId:D}");

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // POST, no body
                var resp = await http.PostAsync(url, content: null, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[POLL] Status={(int)resp.StatusCode} Body={Trunc(body)}");

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Poll HTTP {(int)resp.StatusCode}: {body}");

                // Nothing available yet (body empty or 204)
                if (string.IsNullOrWhiteSpace(body))
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                MessageResult? result = null;
                try
                {
                    result = JsonSerializer.Deserialize<MessageResult>(body, _jsonOpts);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[POLL] Deserialize error: " + ex);
                    await Task.Delay(1000, ct);
                    continue;
                }

                if (result == null)
                {
                    Debug.WriteLine("[POLL] Null result, retrying...");
                    await Task.Delay(1000, ct);
                    continue;
                }

                Debug.WriteLine($"[POLL] Got messageId={result.messageId}, receiptId={result.receiptId}");

                if (string.Equals(result.receiptId, expectedReceiptId.ToString(), StringComparison.OrdinalIgnoreCase))
                    return result;

                // Not our message → you can also persist it; for now just continue.
                Debug.WriteLine("[POLL] Different receipt; continue polling...");
                await Task.Delay(600, ct);
            }
        }
        private async Task<PollMessageResponse?> PollForResultAsync(string expectedReceiptId, CancellationToken ct)
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

                // POST, no body
                var resp = await http.PostAsync(url, content: null, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[POLL] Status={(int)resp.StatusCode} Body={Trunc(body)}");

                if (resp.StatusCode == System.Net.HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(body))
                {
                    await Task.Delay(1000, ct);
                    continue; // queue empty
                }

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Poll HTTP {(int)resp.StatusCode}: {body}");

                PollMessageResponse? envelope = null;
                try
                {
                    envelope = JsonSerializer.Deserialize<PollMessageResponse>(body, _jsonOpts);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[POLL] Deserialize error: " + ex);
                    await Task.Delay(800, ct);
                    continue;
                }

                if (envelope == null)
                {
                    Debug.WriteLine("[POLL] Null envelope, retrying...");
                    await Task.Delay(800, ct);
                    continue;
                }

                Debug.WriteLine($"[POLL] Got responseId={envelope.responseId}, header.messageId={envelope.header?.messageId}, result.codeType={envelope.result?.codeType}, code={envelope.result?.code}");

                // Is this the async result for our sync response?
                if (string.Equals(envelope.responseId, expectedReceiptId, StringComparison.OrdinalIgnoreCase))
                    return envelope;

                // Different operation’s result — you may store it or ignore it
                Debug.WriteLine("[POLL] Different responseId; continue polling...");
                await Task.Delay(600, ct);
            }
        }

        private async Task<string?> PollForResultRawAsync(Guid expectedReceiptId, CancellationToken ct)
        {
            using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", GetAccessToken());

            var url = WithConsumer(PollMessageUrl);
            Debug.WriteLine($"[POLL-RAW] Using {http.BaseAddress}{url}");
            Debug.WriteLine($"[POLL-RAW] Expecting receiptId={expectedReceiptId:D}");

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var resp = await http.PostAsync(url, content: null, ct);
                var body = await resp.Content.ReadAsStringAsync(ct);
                Debug.WriteLine($"[POLL-RAW] Status={(int)resp.StatusCode} Body={Trunc(body)}");

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Poll HTTP {(int)resp.StatusCode}: {body}");

                if (string.IsNullOrWhiteSpace(body))
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                // We only care that it's the message for our receiptId; use a tiny check:
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("receiptId", out var ridEl))
                    {
                        var rid = ridEl.GetString();
                        if (string.Equals(rid, expectedReceiptId.ToString(), StringComparison.OrdinalIgnoreCase))
                            return body; // ← raw json envelope
                    }
                }
                catch { /* if parse fails, just loop */ }

                await Task.Delay(600, ct);
            }
        }


        //private async Task<MessageResult> PollForResultAsync(Guid receiptId, CancellationToken ct)
        //{
        //    // Basic polling loop; tune delay/backoff in real project.
        //    using var http = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        //    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAccessToken());

        //    while (true)
        //    {
        //        ct.ThrowIfCancellationRequested();

        //        var resp = await http.GetAsync(QueueResultsUrl, ct);
        //        var body = await resp.Content.ReadAsStringAsync(ct);

        //        if (!resp.IsSuccessStatusCode)
        //            throw new InvalidOperationException($"Results HTTP {(int)resp.StatusCode}: {body}");

        //        var list = JsonSerializer.Deserialize<List<MessageResult>>(body, _jsonOpts) ?? new List<MessageResult>();

        //        // If queue is empty, wait a bit and try again
        //        if (list.Count == 0)
        //        {
        //            await Task.Delay(1000, ct);
        //            continue;
        //        }

        //        // try find the result for our receiptId
        //        var match = list.FirstOrDefault(r => string.Equals(r.receiptId, receiptId, StringComparison.OrdinalIgnoreCase));
        //        if (match != null)
        //            return match;

        //        // Not found yet — optionally ACK unrelated messages or just wait
        //        await Task.Delay(1000, ct);
        //    }
        //}

        // Step 3: ACK a processed result

        // ======= Helpers you plug into your DB layer =======
        private async Task SaveSyncReceiptAsync(int personId, SyncResponse sync)
        {
            Debug.WriteLine($"Saving sync receipt for personId={personId}, messageId={sync?.header?.messageId}, receiptId={sync?.responseId}");
            Debug.WriteLine(_session.UserId);
            var rec = new RegesSync
            {
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
                Debug.WriteLine($"[POLL] Status={(int)resp.StatusCode} Body={Trunc(body)}");

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
                            var rec = await _db.Set<RegesSync>()
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

                // update same RegesSync row (saves RegesEmployeeId if present)
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

                var rec = await _db.Set<RegesSync>()
                    .FirstOrDefaultAsync(x => x.MessageResultId.HasValue && x.MessageResultId.Value == rid);

                if (rec == null)
                {
                    Debug.WriteLine($"[DB] No RegesSync row for responseId={responseId}");
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

                Debug.WriteLine($"[DB] Updated RegesSync: status={rec.Status}, RegesEmployeeId={rec.RegesEmployeeId}, code={code}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[DB] Save error: " + ex);
                throw;
            }
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
    // Envelope returned by POST /api/Status/PollMessage
    public sealed class PollMessageResponse
    {
        public PollResult result { get; set; }
        public string responseId { get; set; }   // <-- this is your receiptId
        public SyncHeader header { get; set; }   // reuse your existing SyncHeader
    }

    public sealed class PollResult
    {
        public string code { get; set; }         // e.g., "OK" or "FAIL"
        public string codeType { get; set; }     // e.g., "SUCCESS" or "ERROR"
        public bool? signSpecified { get; set; }
        public string description { get; set; }
        public bool? relatedResultsExpected { get; set; }
    }

    // Keep these from earlier
    public sealed class SyncResponse
    {
        public string responseId { get; set; }
        public SyncHeader header { get; set; }
    }

    public sealed class SyncHeader
    {
        public string messageId { get; set; }
        public string authorId { get; set; }
        public string clientApplication { get; set; }
        public string version { get; set; }
        public string operation { get; set; }
        public string sessionId { get; set; }
        public string user { get; set; }
        public string userId { get; set; }
        public DateTime? timestamp { get; set; }
    }

}
