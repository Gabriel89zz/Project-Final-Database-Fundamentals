namespace Project_Final_Database_Fundamentals
{
    partial class RegisterForm
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
            label3 = new Label();
            txtPasswordReg = new TextBox();
            label2 = new Label();
            label1 = new Label();
            label4 = new Label();
            txtLastName = new TextBox();
            label5 = new Label();
            txtEmail = new TextBox();
            label7 = new Label();
            txtUsernameReg = new TextBox();
            txtFirstName = new TextBox();
            btnSignUp = new Button();
            SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Instrument Sans", 11.25F, FontStyle.Bold);
            label3.ForeColor = SystemColors.ControlLight;
            label3.Location = new Point(99, 313);
            label3.Name = "label3";
            label3.Size = new Size(80, 20);
            label3.TabIndex = 11;
            label3.Text = "Password";
            // 
            // txtPasswordReg
            // 
            txtPasswordReg.BorderStyle = BorderStyle.FixedSingle;
            txtPasswordReg.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPasswordReg.Location = new Point(99, 342);
            txtPasswordReg.Multiline = true;
            txtPasswordReg.Name = "txtPasswordReg";
            txtPasswordReg.Size = new Size(282, 30);
            txtPasswordReg.TabIndex = 10;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Instrument Sans", 11.25F, FontStyle.Bold);
            label2.ForeColor = SystemColors.ControlLight;
            label2.Location = new Point(99, 110);
            label2.Name = "label2";
            label2.Size = new Size(88, 20);
            label2.TabIndex = 9;
            label2.Text = "First Name";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Instrument Sans", 27F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.FromArgb(77, 161, 103);
            label1.Location = new Point(156, 22);
            label1.Name = "label1";
            label1.Size = new Size(153, 48);
            label1.TabIndex = 8;
            label1.Text = "Sign Up";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Instrument Sans", 11.25F, FontStyle.Bold);
            label4.ForeColor = SystemColors.ControlLight;
            label4.Location = new Point(248, 110);
            label4.Name = "label4";
            label4.Size = new Size(87, 20);
            label4.TabIndex = 13;
            label4.Text = "Last Name";
            // 
            // txtLastName
            // 
            txtLastName.BorderStyle = BorderStyle.FixedSingle;
            txtLastName.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtLastName.Location = new Point(248, 139);
            txtLastName.Multiline = true;
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(133, 30);
            txtLastName.TabIndex = 12;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Instrument Sans", 11.25F, FontStyle.Bold);
            label5.ForeColor = SystemColors.ControlLight;
            label5.Location = new Point(99, 236);
            label5.Name = "label5";
            label5.Size = new Size(49, 20);
            label5.TabIndex = 15;
            label5.Text = "Email";
            // 
            // txtEmail
            // 
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtEmail.Location = new Point(99, 265);
            txtEmail.Multiline = true;
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(282, 30);
            txtEmail.TabIndex = 14;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Instrument Sans", 11.25F, FontStyle.Bold);
            label7.ForeColor = SystemColors.ControlLight;
            label7.Location = new Point(99, 174);
            label7.Name = "label7";
            label7.Size = new Size(83, 20);
            label7.TabIndex = 19;
            label7.Text = "Username";
            // 
            // txtUsernameReg
            // 
            txtUsernameReg.BorderStyle = BorderStyle.FixedSingle;
            txtUsernameReg.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtUsernameReg.Location = new Point(99, 203);
            txtUsernameReg.Multiline = true;
            txtUsernameReg.Name = "txtUsernameReg";
            txtUsernameReg.Size = new Size(282, 30);
            txtUsernameReg.TabIndex = 18;
            // 
            // txtFirstName
            // 
            txtFirstName.BorderStyle = BorderStyle.FixedSingle;
            txtFirstName.Font = new Font("Instrument Sans Medium", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtFirstName.Location = new Point(99, 139);
            txtFirstName.Multiline = true;
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(133, 30);
            txtFirstName.TabIndex = 20;
            // 
            // btnSignUp
            // 
            btnSignUp.BackColor = Color.FromArgb(77, 161, 103);
            btnSignUp.FlatAppearance.BorderSize = 0;
            btnSignUp.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnSignUp.FlatStyle = FlatStyle.Flat;
            btnSignUp.Font = new Font("Instrument Sans", 11.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSignUp.ForeColor = SystemColors.ControlLight;
            btnSignUp.Location = new Point(174, 404);
            btnSignUp.Name = "btnSignUp";
            btnSignUp.Size = new Size(109, 38);
            btnSignUp.TabIndex = 21;
            btnSignUp.Text = "Sign Up";
            btnSignUp.UseVisualStyleBackColor = false;
            btnSignUp.Click += btnSignUp_Click;
            // 
            // RegisterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(42, 43, 46);
            ClientSize = new Size(478, 496);
            Controls.Add(btnSignUp);
            Controls.Add(txtFirstName);
            Controls.Add(label7);
            Controls.Add(txtUsernameReg);
            Controls.Add(label5);
            Controls.Add(txtEmail);
            Controls.Add(label4);
            Controls.Add(txtLastName);
            Controls.Add(label3);
            Controls.Add(txtPasswordReg);
            Controls.Add(label2);
            Controls.Add(label1);
            Name = "RegisterForm";
            Text = "RegisterForm";
            Load += RegisterForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label3;
        private TextBox txtPasswordReg;
        private TextBox textBox2;
        private Label label2;
        private Label label1;
        private Button button1;
        private Label label4;
        private TextBox txtLastName;
        private TextBox textBox3;
        private Label label5;
        private TextBox txtEmail;
        private Label label6;
        private TextBox textBox5;
        private Label label7;
        private TextBox txtUsernameReg;
        private TextBox txtFirstName;
        private Button btnSignUp;
    }
}