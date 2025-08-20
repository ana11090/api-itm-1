using api_itm.Data;
using api_itm.Infrastructure.Sessions;
using api_itm.Models;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using static api_itm.Program;
using static api_itm.Program;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace api_itm
{
    public partial class ControlCredentiale : UserControl
    {
        private readonly AppDbContext _db;
        public Button LoginButton => btnLogin;

        private System.Windows.Forms.Timer tokenRefreshTimer;

        private readonly ISessionContext _session;
        public ControlCredentiale(AppDbContext db, ISessionContext session)
        {
            InitializeComponent();
            _db = db; 
            _session = session; // set via constructor
            this.Load += ControlCredentials_Load;
        }

        private void lbIntoducereCredentiale_Click(object sender, EventArgs e)
        {

        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtCredentialsUser.Text.Trim();
            string password = txtCredentialsPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            string token = await LoginAsync(username, password);
            Debug.WriteLine("=== Raw token from API ===");
            Debug.WriteLine(token);

            if (token != null)
            {
                MessageBox.Show("Login successful!");

                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

                Debug.WriteLine("=== JWT Header ===");
                foreach (var h in jwt.Header)
                    Debug.WriteLine($"{h.Key}: {h.Value}");

                Debug.WriteLine("=== JWT Claims ===");
                foreach (var claim in jwt.Claims)
                    Debug.WriteLine($"{claim.Type}: {claim.Value}");

                if (jwt.Payload.Exp.HasValue)
                {
                    var exp = DateTimeOffset.FromUnixTimeSeconds(jwt.Payload.Exp.Value).ToLocalTime();
                    Debug.WriteLine($"Token expires at: {exp}");
                    StartTokenRefreshTimer(SessionState.Tokens.Expiration);

                    //  Get service provider from Program or store it globally
                    var scope = App.Services.CreateScope();
                    var mainForm = scope.ServiceProvider.GetRequiredService<MainForm>();
                    mainForm.StartPosition = FormStartPosition.CenterScreen;

                    var parentForm = this.FindForm();
                    mainForm.FormClosed += (s, ev) => parentForm?.Close();

                    mainForm.Show();
                    parentForm?.Hide();

                      
                }
            }
            else
            {
                MessageBox.Show("Invalid credentials or login failed.");
            }
        }

        private void StartTokenRefreshTimer(DateTime expirationTime)
        {
            int millisecondsUntilRefresh = (int)(expirationTime - DateTime.UtcNow - TimeSpan.FromSeconds(60)).TotalMilliseconds;

            if (millisecondsUntilRefresh <= 0)
                millisecondsUntilRefresh = 1000; // fallback to refresh in 1 sec if already near expiration

            tokenRefreshTimer = new System.Windows.Forms.Timer();
            tokenRefreshTimer.Interval = millisecondsUntilRefresh;
            tokenRefreshTimer.Tick += async (s, e) =>
            {
                tokenRefreshTimer.Stop(); // stop current timer to avoid overlaps
                await RefreshTokenAsync(); // call your refresh method
            };
            tokenRefreshTimer.Start();
        }

        private async Task<string> LoginAsync(string username, string password)
        {
            using var client = new HttpClient();

            // Discover OpenID endpoints
            var disco = await client.GetDiscoveryDocumentAsync("https://sso.dev.inspectiamuncii.org/realms/API");
            if (disco.IsError)
            {
                MessageBox.Show($"Discovery failed: {disco.Error}");
                return null;
            }

            // Ask for access and refresh token
            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "reges-api",
                ClientSecret = "FjtrYvDTGZKiyHGdSWymOvxhqifTJ7Em",
                UserName = username,
                Password = password,
                Scope = "openid profile"
            });

            if (response.IsError)
            {
                MessageBox.Show($"Token request failed: {response.Error}");
                return null;
            }

            // Store the token for reuse
            SessionState.Tokens = new TokenStore
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                Expiration = DateTime.UtcNow.AddSeconds(response.ExpiresIn)
            };

            Properties.Settings.Default.SavedCredentialsUser = username;
            Properties.Settings.Default.SavedCredentialsPassword = password;
            Properties.Settings.Default.Save();

            return response.AccessToken;
        }

        private async Task RefreshTokenAsync()
        {
            if (SessionState.Tokens == null || string.IsNullOrEmpty(SessionState.Tokens.RefreshToken))
            {
                Debug.WriteLine("No refresh token available.");
                return;
            }

            using var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://sso.dev.inspectiamuncii.org/realms/API");

            if (disco.IsError)
            {
                Debug.WriteLine($"Discovery error: {disco.Error}");
                return;
            }

            var refreshResponse = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "reges-api",
                ClientSecret = "FjtrYvDTGZKiyHGdSWymOvxhqifTJ7Em",
                RefreshToken = SessionState.Tokens.RefreshToken
            });

            if (refreshResponse.IsError)
            {
                Debug.WriteLine($"Refresh failed: {refreshResponse.Error}");
                return;
            }

            SessionState.Tokens = new TokenStore
            {
                AccessToken = refreshResponse.AccessToken,
                RefreshToken = refreshResponse.RefreshToken,
                Expiration = DateTime.UtcNow.AddSeconds(refreshResponse.ExpiresIn)
            };

            Debug.WriteLine("Token refreshed successfully.");

            // Restart timer for next refresh
            StartTokenRefreshTimer(SessionState.Tokens.Expiration);
        }


        private void txtCredentialsUser_TextChanged(object sender, EventArgs e)
        {

        }

        private void ControlCredentials_Load(object sender, EventArgs e)
        {
            txtCredentialsUser.Text = Properties.Settings.Default.SavedCredentialsUser;
            txtCredentialsPassword.Text = Properties.Settings.Default.SavedCredentialsPassword;
        } 
    }
}
