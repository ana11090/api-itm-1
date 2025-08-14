using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace api_itm.UserControler.UserProfile
{
    public partial class ControlDetailsUserProfile : UserControl
    {
        private readonly AppDbContext _db;
        public ControlDetailsUserProfile(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
        }

        private void ControlDetailsUserProfile_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
