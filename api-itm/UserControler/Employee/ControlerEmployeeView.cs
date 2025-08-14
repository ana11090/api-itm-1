using api_itm.Data;
using System.Windows.Forms;
using System.Linq;
namespace api_itm.UserControler.Employee
{
    public partial class ControlerEmployeeView : UserControl
    {
        private readonly AppDbContext _db;

        public ControlerEmployeeView(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
        }

        private void ControlerEmployeeView_Load(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var employees = _db.People
                .Select(p => new
                {
                    p.Nume,
                    p.Prenume,
                    p.Cnp,
                    p.Adresa,
                    p.DataNasterii
                })
                .ToList();

            dgvViewSalariati.DataSource = employees;
        }
    }
}
