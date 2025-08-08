using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace api_itm
{
    public partial class FormAddCredentials : Form
    {
        private readonly AppDbContext _db;
        public FormAddCredentials(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            this.Load += FormAddCredentials_Load;

        }

        private void FormAddCredentials_Load(object sender, EventArgs e)
        {
            var credentialControl = new ControlCredentiale(_db);
            //to put everything in the middle of the form
            int x = (this.ClientSize.Width - credentialControl.Width) / 2;
            int y = (this.ClientSize.Height - credentialControl.Height) / 2;
            credentialControl.Location = new Point(x, y);
            this.AcceptButton = credentialControl.LoginButton;

            this.Controls.Add(credentialControl);
        }

        private void FormAddCredentials_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit(); // ensures app exits fully
           }
        }
}
