using api_itm.Infrastructure.Sessions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace api_itm.UserControler.Contracts
{
    public partial class ControlerAddContractsView : UserControl
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
        public ControlerAddContractsView()
        {
            InitializeComponent();
        }

        private void ControlerAddContractsView_Load(object sender, EventArgs e)
        {

        }
    }
}
