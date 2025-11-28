namespace Project_Final_Database_Fundamentals
{
    partial class MainAppForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            lblWelcome = new Label();
            btnPlayerPerfomance = new Button();
            btnLineupsAndFormations = new Button();
            btnTeamsAndClubs = new Button();
            btnHumanResources = new Button();
            btnSystemAdmnistration = new Button();
            btnCompetitionsAndSeasons = new Button();
            btnGameMatchesAndEvents = new Button();
            panelContenido = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(42, 43, 46);
            panel1.Controls.Add(lblWelcome);
            panel1.Controls.Add(btnPlayerPerfomance);
            panel1.Controls.Add(btnLineupsAndFormations);
            panel1.Controls.Add(btnTeamsAndClubs);
            panel1.Controls.Add(btnHumanResources);
            panel1.Controls.Add(btnSystemAdmnistration);
            panel1.Controls.Add(btnCompetitionsAndSeasons);
            panel1.Controls.Add(btnGameMatchesAndEvents);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(233, 546);
            panel1.TabIndex = 0;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(70, 55);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new Size(38, 15);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "label1";
            // 
            // btnPlayerPerfomance
            // 
            btnPlayerPerfomance.FlatAppearance.BorderSize = 0;
            btnPlayerPerfomance.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnPlayerPerfomance.FlatStyle = FlatStyle.Flat;
            btnPlayerPerfomance.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnPlayerPerfomance.ForeColor = Color.WhiteSmoke;
            btnPlayerPerfomance.Location = new Point(0, 375);
            btnPlayerPerfomance.Name = "btnPlayerPerfomance";
            btnPlayerPerfomance.Size = new Size(233, 37);
            btnPlayerPerfomance.TabIndex = 14;
            btnPlayerPerfomance.Text = "Player Performance And Health";
            btnPlayerPerfomance.UseVisualStyleBackColor = true;
            btnPlayerPerfomance.Click += btnPlayerPerfomance_Click;
            // 
            // btnLineupsAndFormations
            // 
            btnLineupsAndFormations.FlatAppearance.BorderSize = 0;
            btnLineupsAndFormations.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnLineupsAndFormations.FlatStyle = FlatStyle.Flat;
            btnLineupsAndFormations.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLineupsAndFormations.ForeColor = Color.WhiteSmoke;
            btnLineupsAndFormations.Location = new Point(0, 289);
            btnLineupsAndFormations.Name = "btnLineupsAndFormations";
            btnLineupsAndFormations.Size = new Size(233, 37);
            btnLineupsAndFormations.TabIndex = 12;
            btnLineupsAndFormations.Text = "Lineups and Formations";
            btnLineupsAndFormations.UseVisualStyleBackColor = true;
            btnLineupsAndFormations.Click += btnLineupsAndFormations_Click;
            // 
            // btnTeamsAndClubs
            // 
            btnTeamsAndClubs.FlatAppearance.BorderSize = 0;
            btnTeamsAndClubs.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnTeamsAndClubs.FlatStyle = FlatStyle.Flat;
            btnTeamsAndClubs.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnTeamsAndClubs.ForeColor = Color.WhiteSmoke;
            btnTeamsAndClubs.Location = new Point(0, 203);
            btnTeamsAndClubs.Name = "btnTeamsAndClubs";
            btnTeamsAndClubs.Size = new Size(233, 37);
            btnTeamsAndClubs.TabIndex = 10;
            btnTeamsAndClubs.Text = "Teams And Clubs";
            btnTeamsAndClubs.UseVisualStyleBackColor = true;
            btnTeamsAndClubs.Click += btnTeamsAndClubs_Click;
            // 
            // btnHumanResources
            // 
            btnHumanResources.FlatAppearance.BorderSize = 0;
            btnHumanResources.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnHumanResources.FlatStyle = FlatStyle.Flat;
            btnHumanResources.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnHumanResources.ForeColor = Color.WhiteSmoke;
            btnHumanResources.Location = new Point(0, 246);
            btnHumanResources.Name = "btnHumanResources";
            btnHumanResources.Size = new Size(233, 37);
            btnHumanResources.TabIndex = 11;
            btnHumanResources.Text = "Human Resources";
            btnHumanResources.UseVisualStyleBackColor = true;
            btnHumanResources.Click += btnHumanResources_Click;
            // 
            // btnSystemAdmnistration
            // 
            btnSystemAdmnistration.FlatAppearance.BorderSize = 0;
            btnSystemAdmnistration.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnSystemAdmnistration.FlatStyle = FlatStyle.Flat;
            btnSystemAdmnistration.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSystemAdmnistration.ForeColor = Color.WhiteSmoke;
            btnSystemAdmnistration.Location = new Point(0, 117);
            btnSystemAdmnistration.Name = "btnSystemAdmnistration";
            btnSystemAdmnistration.Size = new Size(233, 37);
            btnSystemAdmnistration.TabIndex = 8;
            btnSystemAdmnistration.Text = "System Administration";
            btnSystemAdmnistration.UseVisualStyleBackColor = true;
            btnSystemAdmnistration.Click += btnSystemAdmnistration_Click;
            // 
            // btnCompetitionsAndSeasons
            // 
            btnCompetitionsAndSeasons.FlatAppearance.BorderSize = 0;
            btnCompetitionsAndSeasons.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnCompetitionsAndSeasons.FlatStyle = FlatStyle.Flat;
            btnCompetitionsAndSeasons.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnCompetitionsAndSeasons.ForeColor = Color.WhiteSmoke;
            btnCompetitionsAndSeasons.Location = new Point(0, 160);
            btnCompetitionsAndSeasons.Name = "btnCompetitionsAndSeasons";
            btnCompetitionsAndSeasons.Size = new Size(233, 37);
            btnCompetitionsAndSeasons.TabIndex = 9;
            btnCompetitionsAndSeasons.Text = "Competitions And Seasons";
            btnCompetitionsAndSeasons.UseVisualStyleBackColor = true;
            btnCompetitionsAndSeasons.Click += btnCompetitionsAndSeasons_Click;
            // 
            // btnGameMatchesAndEvents
            // 
            btnGameMatchesAndEvents.FlatAppearance.BorderSize = 0;
            btnGameMatchesAndEvents.FlatAppearance.MouseOverBackColor = Color.FromArgb(77, 161, 103);
            btnGameMatchesAndEvents.FlatStyle = FlatStyle.Flat;
            btnGameMatchesAndEvents.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnGameMatchesAndEvents.ForeColor = Color.WhiteSmoke;
            btnGameMatchesAndEvents.Location = new Point(0, 332);
            btnGameMatchesAndEvents.Name = "btnGameMatchesAndEvents";
            btnGameMatchesAndEvents.Size = new Size(233, 37);
            btnGameMatchesAndEvents.TabIndex = 13;
            btnGameMatchesAndEvents.Text = "Game Matches And Events";
            btnGameMatchesAndEvents.UseVisualStyleBackColor = true;
            btnGameMatchesAndEvents.Click += btnGameMatchesAndEvents_Click;
            // 
            // panelContenido
            // 
            panelContenido.Dock = DockStyle.Fill;
            panelContenido.Location = new Point(233, 0);
            panelContenido.Name = "panelContenido";
            panelContenido.Size = new Size(766, 546);
            panelContenido.TabIndex = 1;
            // 
            // MainAppForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(999, 546);
            Controls.Add(panelContenido);
            Controls.Add(panel1);
            Name = "MainAppForm";
            Text = "Form1";
            FormClosed += MainAppForm_FormClosed;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panelContenido;
        private Button btnPlayerPerfomance;
        private Button btnLineupsAndFormations;
        private Button btnTeamsAndClubs;
        private Button btnHumanResources;
        private Button btnSystemAdmnistration;
        private Button btnCompetitionsAndSeasons;
        private Button btnGameMatchesAndEvents;
        private Label lblWelcome;
    }
}
