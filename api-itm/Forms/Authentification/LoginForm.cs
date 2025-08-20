using api_itm.Data;
using api_itm.Infrastructure.Sessions; // <-- add this
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace api_itm
{
    public partial class LoginForm : Form
    {
        private readonly AppDbContext _db;
        private ISessionContext _session; // set via Init(...)

        public LoginForm(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            this.AcceptButton = btnLogin;
        }

        // Call this once right after resolving the form
        public void Init(ISessionContext sessionContext)
        {
            _session = sessionContext;
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

            if (user != null) // Login successful
            {
                Debug.WriteLine("Login successful");

                // Save username for next time
                Properties.Settings.Default.SavedUsername = username;
                Properties.Settings.Default.Save();

                //  create/update in-memory session 
                if (_session is SessionContext sc)
                {
                    sc.SessionId = Guid.NewGuid().ToString("D"); // one per login
                    sc.UserName = user.Username;
                 //   sc.UserId = user.IdUser.ToString(); se adauga la conectarea cu credentiale de fapt...
                }
                // 

                Debug.WriteLine("=== Session just created LoginForm ===");
                Debug.WriteLine($"SessionId: {_session.SessionId}");
                Debug.WriteLine($"UserName: {_session.UserName}");
               // Debug.WriteLine($"UserId: {_session.UserId}");

                // Open next form
                var credentials = new FormAddCredentials(_db, _session)
                {
                    StartPosition = FormStartPosition.Manual,
                    Location = this.Location,
                    Size = this.Size
                };
                credentials.Show();
                this.Hide(); // don't close app
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
                txtUsername.Text = savedUsername;
        }

        private void lbUsername_Click(object sender, EventArgs e) { }
        private void txtUsername_TextChanged(object sender, EventArgs e) { }
    }
}
