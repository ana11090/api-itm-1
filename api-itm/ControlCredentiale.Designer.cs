namespace api_itm
{
    partial class ControlCredentiale
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            txtCredentialsUser = new TextBox();
            txtCredentialsPassword = new TextBox();
            lbUsername = new Label();
            lbPassword = new Label();
            lbIntoducereCredentiale = new Label();
            btnLogin = new Button();
            SuspendLayout();
            // 
            // txtCredentialsUser
            // 
            txtCredentialsUser.Location = new Point(185, 60);
            txtCredentialsUser.Name = "txtCredentialsUser";
            txtCredentialsUser.Size = new Size(288, 27);
            txtCredentialsUser.TabIndex = 0;
            txtCredentialsUser.TextChanged += txtCredentialsUser_TextChanged;
            // 
            // txtCredentialsPassword
            // 
            txtCredentialsPassword.Location = new Point(185, 103);
            txtCredentialsPassword.Name = "txtCredentialsPassword";
            txtCredentialsPassword.Size = new Size(288, 27);
            txtCredentialsPassword.TabIndex = 1;
            // 
            // lbUsername
            // 
            lbUsername.AutoSize = true;
            lbUsername.Location = new Point(103, 63);
            lbUsername.Name = "lbUsername";
            lbUsername.Size = new Size(38, 20);
            lbUsername.TabIndex = 2;
            lbUsername.Text = "User";
            // 
            // lbPassword
            // 
            lbPassword.AutoSize = true;
            lbPassword.Location = new Point(103, 110);
            lbPassword.Name = "lbPassword";
            lbPassword.Size = new Size(50, 20);
            lbPassword.TabIndex = 3;
            lbPassword.Text = "Parola";
            // 
            // lbIntoducereCredentiale
            // 
            lbIntoducereCredentiale.Location = new Point(23, 25);
            lbIntoducereCredentiale.Name = "lbIntoducereCredentiale";
            lbIntoducereCredentiale.Size = new Size(395, 21);
            lbIntoducereCredentiale.TabIndex = 4;
            lbIntoducereCredentiale.Text = "Introduceti credentialele generate de pe platofma";
            lbIntoducereCredentiale.Click += lbIntoducereCredentiale_Click;
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(262, 159);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(125, 29);
            btnLogin.TabIndex = 5;
            btnLogin.Text = "Autentificare";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // ControlCredentiale
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnLogin);
            Controls.Add(lbIntoducereCredentiale);
            Controls.Add(lbPassword);
            Controls.Add(lbUsername);
            Controls.Add(txtCredentialsPassword);
            Controls.Add(txtCredentialsUser);
            Name = "ControlCredentiale";
            Size = new Size(580, 246);
            Load += ControlCredentials_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtCredentialsUser;
        private TextBox txtCredentialsPassword;
        private Label lbUsername;
        private Label lbPassword;
        private Label lbIntoducereCredentiale;
        private Button btnLogin;
    }
}
