namespace Project_Final_Database_Fundamentals
{
    partial class LoginForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnLogin = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            txtPassword = new TextBox();
            linkRegister = new LinkLabel();
            label4 = new Label();
            txtUsername = new TextBox();
            SuspendLayout();
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(77, 161, 103);
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Instrument Sans", 11.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = SystemColors.ControlLight;
            btnLogin.Location = new Point(183, 298);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(109, 38);
            btnLogin.TabIndex = 0;
            btnLogin.Text = "Log in";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Instrument Sans", 27F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(77, 161, 103);
            label1.Location = new Point(170, 47);
            label1.Name = "label1";
            label1.Size = new Size(122, 48);
            label1.TabIndex = 2;
            label1.Text = "Log In";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Instrument Sans SemiBold", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = SystemColors.ControlLight;
            label2.Location = new Point(115, 131);
            label2.Name = "label2";
            label2.Size = new Size(104, 26);
            label2.TabIndex = 3;
            label2.Text = "Username";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Instrument Sans SemiBold", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.ForeColor = SystemColors.ControlLight;
            label3.Location = new Point(116, 221);
            label3.Name = "label3";
            label3.Size = new Size(98, 26);
            label3.TabIndex = 5;
            label3.Text = "Password";
            // 
            // txtPassword
            // 
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(116, 250);
            txtPassword.Multiline = true;
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(246, 30);
            txtPassword.TabIndex = 4;
            // 
            // linkRegister
            // 
            linkRegister.ActiveLinkColor = Color.FromArgb(59, 193, 74);
            linkRegister.AutoSize = true;
            linkRegister.Font = new Font("Instrument Sans Medium", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            linkRegister.LinkColor = Color.FromArgb(77, 161, 103);
            linkRegister.Location = new Point(273, 432);
            linkRegister.Name = "linkRegister";
            linkRegister.Size = new Size(69, 20);
            linkRegister.TabIndex = 6;
            linkRegister.TabStop = true;
            linkRegister.Text = "Sign up?";
            linkRegister.LinkClicked += linkRegister_LinkClicked;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Instrument Sans Medium", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.ForeColor = SystemColors.ControlLight;
            label4.Location = new Point(94, 432);
            label4.Name = "label4";
            label4.Size = new Size(173, 20);
            label4.TabIndex = 7;
            label4.Text = "Don't have an account?";
            // 
            // txtUsername
            // 
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtUsername.Location = new Point(115, 160);
            txtUsername.Multiline = true;
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(246, 30);
            txtUsername.TabIndex = 8;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(42, 43, 46);
            ClientSize = new Size(478, 496);
            Controls.Add(txtUsername);
            Controls.Add(label4);
            Controls.Add(linkRegister);
            Controls.Add(label3);
            Controls.Add(txtPassword);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnLogin);
            Name = "LoginForm";
            Text = "LoginForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnLogin;
        private TextBox textBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox txtPassword;
        private LinkLabel linkRegister;
        private Label label4;
        private TextBox txtUsername;
    }
}