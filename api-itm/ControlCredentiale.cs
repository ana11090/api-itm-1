using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IdentityModel.Tokens.Jwt;

namespace api_itm
{
    public partial class ControlCredentiale : UserControl
    {
        private readonly AppDbContext _db;

        public ControlCredentiale(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
        }

        private void lbIntoducereCredentiale_Click(object sender, EventArgs e)
        {
            // Optional click event logic
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
            if (token != null)
            {
                MessageBox.Show("Login successful!");

                // Optional: parse token to read claims
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                string exp = jwt.Payload.Exp.HasValue
                    ? DateTimeOffset.FromUnixTimeSeconds(jwt.Payload.Exp.Value).ToLocalTime().ToString()
                    : "unknown";

                MessageBox.Show($"Token expires at: {exp}");
            }
            else
            {
                MessageBox.Show("Invalid credentials or login failed.");
            }
        }

        private async Task<string> LoginAsync(string username, string password)
        {
            using var client = new HttpClient();

            var values = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", "reges-api" },
                { "client_secret", "FjtrYvDTGZKiyHGdSWymOvxhqifTJ7Em" },
                { "username", username },
                { "password", password }
            };

            var content = new FormUrlEncodedContent(values);
            string tokenUrl = "https://sso.dev.inspectiamuncii.org/realms/API/protocol/openid-connect/token";

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync(tokenUrl, content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error contacting server: {ex.Message}");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show($"Login failed: {response.StatusCode}");
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
            return tokenResponse?.access_token;
        }

        private class TokenResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string refresh_token { get; set; }
            public string token_type { get; set; }
        }
    }
}
