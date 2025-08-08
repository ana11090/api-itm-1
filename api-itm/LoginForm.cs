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
    public partial class LoginForm : Form
    {
        private readonly AppDbContext _db;

        public LoginForm(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            this.AcceptButton = btnLogin;
        }


        private void lbUsername_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            Debug.WriteLine($"username: {username}");
            Debug.WriteLine($"password: {password}");

            var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            Debug.WriteLine($"user: {user}");
            if (user != null) //Login successful
            {
                Debug.WriteLine("Login successful");

                //  Save username in settings
                // This will allow the username to be pre-filled next time the form is opened
                Properties.Settings.Default.SavedUsername = username;
                Properties.Settings.Default.Save();

                // Open new form first
                FormAddCredentials credentials = new FormAddCredentials(_db);

                // Match position and size
                credentials.StartPosition = FormStartPosition.Manual;
                credentials.Location = this.Location;
                credentials.Size = this.Size;

                credentials.Show();

                // Then hide the current login form ( can't close it, as it will shut down the application )
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            string savedUsername = Properties.Settings.Default.SavedUsername;
            if (!string.IsNullOrWhiteSpace(savedUsername))
            {
                txtUsername.Text = savedUsername;
            }
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
