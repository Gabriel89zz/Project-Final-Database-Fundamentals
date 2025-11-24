namespace Project_Final_Database_Fundamentals
{
    partial class uc_AdministrationAndConfiguration
    {
        /// <summary> 
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            tabConfederation = new TabControl();
            Confederations = new TabPage();
            label4 = new Label();
            btnClearConfederation = new Button();
            btnDeleteConfederation = new Button();
            panel2 = new Panel();
            dgvConfederations = new DataGridView();
            btnAddConfederation = new Button();
            panel1 = new Panel();
            txtFoundationYear = new TextBox();
            label3 = new Label();
            txtAcronym = new TextBox();
            label2 = new Label();
            txtNameConfederation = new TextBox();
            label1 = new Label();
            btnUpdateConfederation = new Button();
            tabCountries = new TabPage();
            label5 = new Label();
            btnClearCountry = new Button();
            btnDeleteCountry = new Button();
            panel3 = new Panel();
            dgvCountries = new DataGridView();
            btnAddCountry = new Button();
            panel4 = new Panel();
            cmbConfederation = new ComboBox();
            label6 = new Label();
            txtIsoCode = new TextBox();
            label7 = new Label();
            txtNameCountry = new TextBox();
            label8 = new Label();
            btnUpdateCountry = new Button();
            tabCity = new TabPage();
            label9 = new Label();
            btnClearCity = new Button();
            btnDeleteCity = new Button();
            panel5 = new Panel();
            dgvCity = new DataGridView();
            btnAddCity = new Button();
            panel6 = new Panel();
            cmbCountryCity = new ComboBox();
            label11 = new Label();
            txtNameCity = new TextBox();
            label12 = new Label();
            btnUpdateCity = new Button();
            tabStadium = new TabPage();
            dgvStadiums = new DataGridView();
            btnClearStadium = new Button();
            btnDeleteStadium = new Button();
            btnAddStadium = new Button();
            btnUpdateStadium = new Button();
            panel7 = new Panel();
            txtCapacity = new TextBox();
            label25 = new Label();
            cmbStadiumCity = new ComboBox();
            label16 = new Label();
            txtNameStadium = new TextBox();
            label24 = new Label();
            label10 = new Label();
            tabPage4 = new TabPage();
            label17 = new Label();
            btnClearAward = new Button();
            btnDeleteAward = new Button();
            panel9 = new Panel();
            dataGridView5 = new DataGridView();
            btnAddAward = new Button();
            panel10 = new Panel();
            txtScope = new TextBox();
            Scope = new Label();
            txtNameAward = new TextBox();
            label21 = new Label();
            btnUpdateAward = new Button();
            tabPage5 = new TabPage();
            label18 = new Label();
            button21 = new Button();
            button22 = new Button();
            panel11 = new Panel();
            dataGridView6 = new DataGridView();
            button23 = new Button();
            panel12 = new Panel();
            textBox17 = new TextBox();
            label23 = new Label();
            button24 = new Button();
            tabPage6 = new TabPage();
            label19 = new Label();
            button25 = new Button();
            button26 = new Button();
            panel13 = new Panel();
            dataGridView7 = new DataGridView();
            button27 = new Button();
            panel14 = new Panel();
            textBox14 = new TextBox();
            label22 = new Label();
            textBox13 = new TextBox();
            label20 = new Label();
            button28 = new Button();
            tabPage7 = new TabPage();
            tabPage8 = new TabPage();
            tabPage9 = new TabPage();
            label15 = new Label();
            label13 = new Label();
            label14 = new Label();
            tabConfederation.SuspendLayout();
            Confederations.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvConfederations).BeginInit();
            panel1.SuspendLayout();
            tabCountries.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCountries).BeginInit();
            panel4.SuspendLayout();
            tabCity.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvCity).BeginInit();
            panel6.SuspendLayout();
            tabStadium.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStadiums).BeginInit();
            panel7.SuspendLayout();
            tabPage4.SuspendLayout();
            panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView5).BeginInit();
            panel10.SuspendLayout();
            tabPage5.SuspendLayout();
            panel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView6).BeginInit();
            panel12.SuspendLayout();
            tabPage6.SuspendLayout();
            panel13.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView7).BeginInit();
            panel14.SuspendLayout();
            SuspendLayout();
            // 
            // tabConfederation
            // 
            tabConfederation.Controls.Add(Confederations);
            tabConfederation.Controls.Add(tabCountries);
            tabConfederation.Controls.Add(tabCity);
            tabConfederation.Controls.Add(tabStadium);
            tabConfederation.Controls.Add(tabPage4);
            tabConfederation.Controls.Add(tabPage5);
            tabConfederation.Controls.Add(tabPage6);
            tabConfederation.Controls.Add(tabPage7);
            tabConfederation.Controls.Add(tabPage8);
            tabConfederation.Controls.Add(tabPage9);
            tabConfederation.Dock = DockStyle.Fill;
            tabConfederation.Font = new Font("Microsoft Sans Serif", 8.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tabConfederation.Location = new Point(0, 0);
            tabConfederation.Name = "tabConfederation";
            tabConfederation.SelectedIndex = 0;
            tabConfederation.Size = new Size(773, 546);
            tabConfederation.TabIndex = 1;
            tabConfederation.SelectedIndexChanged += tabConfederation_SelectedIndexChanged;
            tabConfederation.Click += tabConfederation_Click;
            // 
            // Confederations
            // 
            Confederations.BackColor = Color.Transparent;
            Confederations.Controls.Add(label4);
            Confederations.Controls.Add(btnClearConfederation);
            Confederations.Controls.Add(btnDeleteConfederation);
            Confederations.Controls.Add(panel2);
            Confederations.Controls.Add(btnAddConfederation);
            Confederations.Controls.Add(panel1);
            Confederations.Controls.Add(btnUpdateConfederation);
            Confederations.Location = new Point(4, 24);
            Confederations.Name = "Confederations";
            Confederations.Padding = new Padding(3);
            Confederations.Size = new Size(765, 518);
            Confederations.TabIndex = 0;
            Confederations.Text = "Confederations";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(269, 11);
            label4.Name = "label4";
            label4.Size = new Size(213, 31);
            label4.TabIndex = 10;
            label4.Text = "Confederations";
            // 
            // btnClearConfederation
            // 
            btnClearConfederation.BackColor = Color.FromArgb(77, 161, 103);
            btnClearConfederation.FlatAppearance.BorderSize = 0;
            btnClearConfederation.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnClearConfederation.FlatStyle = FlatStyle.Flat;
            btnClearConfederation.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnClearConfederation.ForeColor = SystemColors.ControlLightLight;
            btnClearConfederation.Location = new Point(634, 189);
            btnClearConfederation.Name = "btnClearConfederation";
            btnClearConfederation.Size = new Size(91, 32);
            btnClearConfederation.TabIndex = 9;
            btnClearConfederation.Text = "Clear";
            btnClearConfederation.UseVisualStyleBackColor = false;
            btnClearConfederation.Click += btnClearConfederation_Click;
            // 
            // btnDeleteConfederation
            // 
            btnDeleteConfederation.BackColor = Color.FromArgb(77, 161, 103);
            btnDeleteConfederation.FlatAppearance.BorderSize = 0;
            btnDeleteConfederation.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnDeleteConfederation.FlatStyle = FlatStyle.Flat;
            btnDeleteConfederation.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnDeleteConfederation.ForeColor = SystemColors.ControlLightLight;
            btnDeleteConfederation.Location = new Point(634, 145);
            btnDeleteConfederation.Name = "btnDeleteConfederation";
            btnDeleteConfederation.Size = new Size(91, 32);
            btnDeleteConfederation.TabIndex = 8;
            btnDeleteConfederation.Text = "Delete";
            btnDeleteConfederation.UseVisualStyleBackColor = false;
            btnDeleteConfederation.Click += btnDeleteConfederation_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(dgvConfederations);
            panel2.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel2.Location = new Point(18, 241);
            panel2.Name = "panel2";
            panel2.Size = new Size(729, 247);
            panel2.TabIndex = 6;
            // 
            // dgvConfederations
            // 
            dgvConfederations.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvConfederations.Location = new Point(0, 0);
            dgvConfederations.Name = "dgvConfederations";
            dgvConfederations.Size = new Size(729, 247);
            dgvConfederations.TabIndex = 0;
            dgvConfederations.CellClick += dgvConfederations_CellClick;
            // 
            // btnAddConfederation
            // 
            btnAddConfederation.BackColor = Color.FromArgb(77, 161, 103);
            btnAddConfederation.FlatAppearance.BorderSize = 0;
            btnAddConfederation.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnAddConfederation.FlatStyle = FlatStyle.Flat;
            btnAddConfederation.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnAddConfederation.ForeColor = SystemColors.ControlLightLight;
            btnAddConfederation.Location = new Point(634, 57);
            btnAddConfederation.Name = "btnAddConfederation";
            btnAddConfederation.Size = new Size(91, 32);
            btnAddConfederation.TabIndex = 6;
            btnAddConfederation.Text = "Add";
            btnAddConfederation.UseVisualStyleBackColor = false;
            btnAddConfederation.Click += btnAddConfederation_Click_1;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(txtFoundationYear);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(txtAcronym);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(txtNameConfederation);
            panel1.Controls.Add(label1);
            panel1.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel1.Location = new Point(18, 57);
            panel1.Name = "panel1";
            panel1.Size = new Size(589, 167);
            panel1.TabIndex = 0;
            // 
            // txtFoundationYear
            // 
            txtFoundationYear.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtFoundationYear.Location = new Point(388, 47);
            txtFoundationYear.Name = "txtFoundationYear";
            txtFoundationYear.Size = new Size(148, 24);
            txtFoundationYear.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(388, 24);
            label3.Name = "label3";
            label3.Size = new Size(136, 18);
            label3.TabIndex = 4;
            label3.Text = "Foundation Year:";
            // 
            // txtAcronym
            // 
            txtAcronym.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtAcronym.Location = new Point(202, 47);
            txtAcronym.Name = "txtAcronym";
            txtAcronym.Size = new Size(148, 24);
            txtAcronym.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(202, 24);
            label2.Name = "label2";
            label2.Size = new Size(79, 18);
            label2.TabIndex = 2;
            label2.Text = "Acronym:";
            // 
            // txtNameConfederation
            // 
            txtNameConfederation.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNameConfederation.Location = new Point(16, 47);
            txtNameConfederation.Name = "txtNameConfederation";
            txtNameConfederation.Size = new Size(148, 24);
            txtNameConfederation.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 24);
            label1.Name = "label1";
            label1.Size = new Size(57, 18);
            label1.TabIndex = 0;
            label1.Text = "Name:";
            // 
            // btnUpdateConfederation
            // 
            btnUpdateConfederation.BackColor = Color.FromArgb(77, 161, 103);
            btnUpdateConfederation.FlatAppearance.BorderSize = 0;
            btnUpdateConfederation.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnUpdateConfederation.FlatStyle = FlatStyle.Flat;
            btnUpdateConfederation.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnUpdateConfederation.ForeColor = SystemColors.ControlLightLight;
            btnUpdateConfederation.Location = new Point(634, 101);
            btnUpdateConfederation.Name = "btnUpdateConfederation";
            btnUpdateConfederation.Size = new Size(91, 32);
            btnUpdateConfederation.TabIndex = 7;
            btnUpdateConfederation.Text = "Update";
            btnUpdateConfederation.UseVisualStyleBackColor = false;
            btnUpdateConfederation.Click += btnUpdateConfederation_Click;
            // 
            // tabCountries
            // 
            tabCountries.BackColor = Color.Transparent;
            tabCountries.Controls.Add(label5);
            tabCountries.Controls.Add(btnClearCountry);
            tabCountries.Controls.Add(btnDeleteCountry);
            tabCountries.Controls.Add(panel3);
            tabCountries.Controls.Add(btnAddCountry);
            tabCountries.Controls.Add(panel4);
            tabCountries.Controls.Add(btnUpdateCountry);
            tabCountries.Location = new Point(4, 24);
            tabCountries.Name = "tabCountries";
            tabCountries.Padding = new Padding(3);
            tabCountries.Size = new Size(765, 518);
            tabCountries.TabIndex = 1;
            tabCountries.Text = "Country";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label5.Location = new Point(295, 16);
            label5.Name = "label5";
            label5.Size = new Size(140, 31);
            label5.TabIndex = 17;
            label5.Text = "Countries";
            // 
            // btnClearCountry
            // 
            btnClearCountry.BackColor = Color.FromArgb(77, 161, 103);
            btnClearCountry.FlatAppearance.BorderSize = 0;
            btnClearCountry.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnClearCountry.FlatStyle = FlatStyle.Flat;
            btnClearCountry.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnClearCountry.ForeColor = SystemColors.ControlLightLight;
            btnClearCountry.Location = new Point(634, 193);
            btnClearCountry.Name = "btnClearCountry";
            btnClearCountry.Size = new Size(91, 32);
            btnClearCountry.TabIndex = 16;
            btnClearCountry.Text = "Clear";
            btnClearCountry.UseVisualStyleBackColor = false;
            btnClearCountry.Click += btnClearCountry_Click;
            // 
            // btnDeleteCountry
            // 
            btnDeleteCountry.BackColor = Color.FromArgb(77, 161, 103);
            btnDeleteCountry.FlatAppearance.BorderSize = 0;
            btnDeleteCountry.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnDeleteCountry.FlatStyle = FlatStyle.Flat;
            btnDeleteCountry.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnDeleteCountry.ForeColor = SystemColors.ControlLightLight;
            btnDeleteCountry.Location = new Point(634, 149);
            btnDeleteCountry.Name = "btnDeleteCountry";
            btnDeleteCountry.Size = new Size(91, 32);
            btnDeleteCountry.TabIndex = 15;
            btnDeleteCountry.Text = "Delete";
            btnDeleteCountry.UseVisualStyleBackColor = false;
            btnDeleteCountry.Click += btnDeleteCountry_Click;
            // 
            // panel3
            // 
            panel3.BackColor = Color.White;
            panel3.Controls.Add(dgvCountries);
            panel3.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel3.Location = new Point(18, 250);
            panel3.Name = "panel3";
            panel3.Size = new Size(729, 247);
            panel3.TabIndex = 12;
            // 
            // dgvCountries
            // 
            dgvCountries.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCountries.Location = new Point(0, 0);
            dgvCountries.Name = "dgvCountries";
            dgvCountries.Size = new Size(729, 247);
            dgvCountries.TabIndex = 0;
            dgvCountries.CellClick += dgvCountries_CellClick;
            // 
            // btnAddCountry
            // 
            btnAddCountry.BackColor = Color.FromArgb(77, 161, 103);
            btnAddCountry.FlatAppearance.BorderSize = 0;
            btnAddCountry.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnAddCountry.FlatStyle = FlatStyle.Flat;
            btnAddCountry.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnAddCountry.ForeColor = SystemColors.ControlLightLight;
            btnAddCountry.Location = new Point(634, 61);
            btnAddCountry.Name = "btnAddCountry";
            btnAddCountry.Size = new Size(91, 32);
            btnAddCountry.TabIndex = 13;
            btnAddCountry.Text = "Add";
            btnAddCountry.UseVisualStyleBackColor = false;
            btnAddCountry.Click += btnAddCountry_Click;
            // 
            // panel4
            // 
            panel4.BackColor = Color.White;
            panel4.Controls.Add(cmbConfederation);
            panel4.Controls.Add(label6);
            panel4.Controls.Add(txtIsoCode);
            panel4.Controls.Add(label7);
            panel4.Controls.Add(txtNameCountry);
            panel4.Controls.Add(label8);
            panel4.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel4.Location = new Point(18, 57);
            panel4.Name = "panel4";
            panel4.Size = new Size(589, 167);
            panel4.TabIndex = 11;
            // 
            // cmbConfederation
            // 
            cmbConfederation.BackColor = SystemColors.Window;
            cmbConfederation.FlatStyle = FlatStyle.System;
            cmbConfederation.Font = new Font("Microsoft Sans Serif", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            cmbConfederation.FormattingEnabled = true;
            cmbConfederation.Location = new Point(388, 47);
            cmbConfederation.Name = "cmbConfederation";
            cmbConfederation.Size = new Size(145, 23);
            cmbConfederation.TabIndex = 5;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(388, 24);
            label6.Name = "label6";
            label6.Size = new Size(119, 18);
            label6.TabIndex = 4;
            label6.Text = "Confederation:";
            // 
            // txtIsoCode
            // 
            txtIsoCode.BackColor = SystemColors.Window;
            txtIsoCode.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtIsoCode.Location = new Point(202, 47);
            txtIsoCode.Name = "txtIsoCode";
            txtIsoCode.Size = new Size(148, 24);
            txtIsoCode.TabIndex = 3;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(202, 24);
            label7.Name = "label7";
            label7.Size = new Size(86, 18);
            label7.TabIndex = 2;
            label7.Text = "ISO Code:";
            // 
            // txtNameCountry
            // 
            txtNameCountry.BackColor = SystemColors.Window;
            txtNameCountry.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNameCountry.Location = new Point(16, 47);
            txtNameCountry.Name = "txtNameCountry";
            txtNameCountry.Size = new Size(148, 24);
            txtNameCountry.TabIndex = 1;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(16, 24);
            label8.Name = "label8";
            label8.Size = new Size(57, 18);
            label8.TabIndex = 0;
            label8.Text = "Name:";
            // 
            // btnUpdateCountry
            // 
            btnUpdateCountry.BackColor = Color.FromArgb(77, 161, 103);
            btnUpdateCountry.FlatAppearance.BorderSize = 0;
            btnUpdateCountry.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnUpdateCountry.FlatStyle = FlatStyle.Flat;
            btnUpdateCountry.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnUpdateCountry.ForeColor = SystemColors.ControlLightLight;
            btnUpdateCountry.Location = new Point(634, 105);
            btnUpdateCountry.Name = "btnUpdateCountry";
            btnUpdateCountry.Size = new Size(91, 32);
            btnUpdateCountry.TabIndex = 14;
            btnUpdateCountry.Text = "Update";
            btnUpdateCountry.UseVisualStyleBackColor = false;
            btnUpdateCountry.Click += btnUpdateCountry_Click;
            // 
            // tabCity
            // 
            tabCity.Controls.Add(label9);
            tabCity.Controls.Add(btnClearCity);
            tabCity.Controls.Add(btnDeleteCity);
            tabCity.Controls.Add(panel5);
            tabCity.Controls.Add(btnAddCity);
            tabCity.Controls.Add(panel6);
            tabCity.Controls.Add(btnUpdateCity);
            tabCity.Location = new Point(4, 24);
            tabCity.Name = "tabCity";
            tabCity.Size = new Size(765, 518);
            tabCity.TabIndex = 2;
            tabCity.Text = "Cities";
            tabCity.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label9.Location = new Point(326, 17);
            label9.Name = "label9";
            label9.Size = new Size(98, 31);
            label9.TabIndex = 24;
            label9.Text = "Cities:";
            // 
            // btnClearCity
            // 
            btnClearCity.BackColor = Color.FromArgb(77, 161, 103);
            btnClearCity.FlatAppearance.BorderSize = 0;
            btnClearCity.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnClearCity.FlatStyle = FlatStyle.Flat;
            btnClearCity.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnClearCity.ForeColor = SystemColors.ControlLightLight;
            btnClearCity.Location = new Point(644, 200);
            btnClearCity.Name = "btnClearCity";
            btnClearCity.Size = new Size(91, 32);
            btnClearCity.TabIndex = 23;
            btnClearCity.Text = "Clear";
            btnClearCity.UseVisualStyleBackColor = false;
            btnClearCity.Click += btnClearCity_Click;
            // 
            // btnDeleteCity
            // 
            btnDeleteCity.BackColor = Color.FromArgb(77, 161, 103);
            btnDeleteCity.FlatAppearance.BorderSize = 0;
            btnDeleteCity.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnDeleteCity.FlatStyle = FlatStyle.Flat;
            btnDeleteCity.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnDeleteCity.ForeColor = SystemColors.ControlLightLight;
            btnDeleteCity.Location = new Point(644, 156);
            btnDeleteCity.Name = "btnDeleteCity";
            btnDeleteCity.Size = new Size(91, 32);
            btnDeleteCity.TabIndex = 22;
            btnDeleteCity.Text = "Delete";
            btnDeleteCity.UseVisualStyleBackColor = false;
            btnDeleteCity.Click += btnDeleteCity_Click;
            // 
            // panel5
            // 
            panel5.BackColor = Color.White;
            panel5.Controls.Add(dgvCity);
            panel5.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel5.Location = new Point(18, 252);
            panel5.Name = "panel5";
            panel5.Size = new Size(729, 247);
            panel5.TabIndex = 19;
            // 
            // dgvCity
            // 
            dgvCity.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCity.Location = new Point(0, 0);
            dgvCity.Name = "dgvCity";
            dgvCity.Size = new Size(729, 247);
            dgvCity.TabIndex = 0;
            dgvCity.CellClick += dgvCity_CellClick;
            // 
            // btnAddCity
            // 
            btnAddCity.BackColor = Color.FromArgb(77, 161, 103);
            btnAddCity.FlatAppearance.BorderSize = 0;
            btnAddCity.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnAddCity.FlatStyle = FlatStyle.Flat;
            btnAddCity.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnAddCity.ForeColor = SystemColors.ControlLightLight;
            btnAddCity.Location = new Point(644, 68);
            btnAddCity.Name = "btnAddCity";
            btnAddCity.Size = new Size(91, 32);
            btnAddCity.TabIndex = 20;
            btnAddCity.Text = "Add";
            btnAddCity.UseVisualStyleBackColor = false;
            btnAddCity.Click += btnAddCity_Click;
            // 
            // panel6
            // 
            panel6.BackColor = Color.White;
            panel6.Controls.Add(cmbCountryCity);
            panel6.Controls.Add(label11);
            panel6.Controls.Add(txtNameCity);
            panel6.Controls.Add(label12);
            panel6.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel6.Location = new Point(28, 68);
            panel6.Name = "panel6";
            panel6.Size = new Size(589, 167);
            panel6.TabIndex = 18;
            // 
            // cmbCountryCity
            // 
            cmbCountryCity.FormattingEnabled = true;
            cmbCountryCity.Location = new Point(202, 47);
            cmbCountryCity.Name = "cmbCountryCity";
            cmbCountryCity.Size = new Size(139, 26);
            cmbCountryCity.TabIndex = 3;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(202, 24);
            label11.Name = "label11";
            label11.Size = new Size(72, 18);
            label11.TabIndex = 2;
            label11.Text = "Country:";
            // 
            // txtNameCity
            // 
            txtNameCity.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNameCity.Location = new Point(16, 47);
            txtNameCity.Name = "txtNameCity";
            txtNameCity.Size = new Size(148, 24);
            txtNameCity.TabIndex = 1;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(16, 24);
            label12.Name = "label12";
            label12.Size = new Size(57, 18);
            label12.TabIndex = 0;
            label12.Text = "Name:";
            // 
            // btnUpdateCity
            // 
            btnUpdateCity.BackColor = Color.FromArgb(77, 161, 103);
            btnUpdateCity.FlatAppearance.BorderSize = 0;
            btnUpdateCity.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnUpdateCity.FlatStyle = FlatStyle.Flat;
            btnUpdateCity.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnUpdateCity.ForeColor = SystemColors.ControlLightLight;
            btnUpdateCity.Location = new Point(644, 112);
            btnUpdateCity.Name = "btnUpdateCity";
            btnUpdateCity.Size = new Size(91, 32);
            btnUpdateCity.TabIndex = 21;
            btnUpdateCity.Text = "Update";
            btnUpdateCity.UseVisualStyleBackColor = false;
            btnUpdateCity.Click += btnUpdateCity_Click;
            // 
            // tabStadium
            // 
            tabStadium.Controls.Add(dgvStadiums);
            tabStadium.Controls.Add(btnClearStadium);
            tabStadium.Controls.Add(btnDeleteStadium);
            tabStadium.Controls.Add(btnAddStadium);
            tabStadium.Controls.Add(btnUpdateStadium);
            tabStadium.Controls.Add(panel7);
            tabStadium.Controls.Add(label10);
            tabStadium.Location = new Point(4, 24);
            tabStadium.Name = "tabStadium";
            tabStadium.Size = new Size(765, 518);
            tabStadium.TabIndex = 3;
            tabStadium.Text = "Stadiums";
            tabStadium.UseVisualStyleBackColor = true;
            // 
            // dgvStadiums
            // 
            dgvStadiums.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvStadiums.Location = new Point(25, 253);
            dgvStadiums.Name = "dgvStadiums";
            dgvStadiums.Size = new Size(729, 247);
            dgvStadiums.TabIndex = 37;
            dgvStadiums.CellClick += dgvStadiums_CellClick;
            // 
            // btnClearStadium
            // 
            btnClearStadium.BackColor = Color.FromArgb(77, 161, 103);
            btnClearStadium.FlatAppearance.BorderSize = 0;
            btnClearStadium.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnClearStadium.FlatStyle = FlatStyle.Flat;
            btnClearStadium.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnClearStadium.ForeColor = SystemColors.ControlLightLight;
            btnClearStadium.Location = new Point(641, 196);
            btnClearStadium.Name = "btnClearStadium";
            btnClearStadium.Size = new Size(91, 32);
            btnClearStadium.TabIndex = 36;
            btnClearStadium.Text = "Clear";
            btnClearStadium.UseVisualStyleBackColor = false;
            btnClearStadium.Click += btnClearStadium_Click;
            // 
            // btnDeleteStadium
            // 
            btnDeleteStadium.BackColor = Color.FromArgb(77, 161, 103);
            btnDeleteStadium.FlatAppearance.BorderSize = 0;
            btnDeleteStadium.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnDeleteStadium.FlatStyle = FlatStyle.Flat;
            btnDeleteStadium.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnDeleteStadium.ForeColor = SystemColors.ControlLightLight;
            btnDeleteStadium.Location = new Point(641, 152);
            btnDeleteStadium.Name = "btnDeleteStadium";
            btnDeleteStadium.Size = new Size(91, 32);
            btnDeleteStadium.TabIndex = 35;
            btnDeleteStadium.Text = "Delete";
            btnDeleteStadium.UseVisualStyleBackColor = false;
            btnDeleteStadium.Click += btnDeleteStadium_Click;
            // 
            // btnAddStadium
            // 
            btnAddStadium.BackColor = Color.FromArgb(77, 161, 103);
            btnAddStadium.FlatAppearance.BorderSize = 0;
            btnAddStadium.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnAddStadium.FlatStyle = FlatStyle.Flat;
            btnAddStadium.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnAddStadium.ForeColor = SystemColors.ControlLightLight;
            btnAddStadium.Location = new Point(641, 64);
            btnAddStadium.Name = "btnAddStadium";
            btnAddStadium.Size = new Size(91, 32);
            btnAddStadium.TabIndex = 33;
            btnAddStadium.Text = "Add";
            btnAddStadium.UseVisualStyleBackColor = false;
            btnAddStadium.Click += btnAddStadium_Click;
            // 
            // btnUpdateStadium
            // 
            btnUpdateStadium.BackColor = Color.FromArgb(77, 161, 103);
            btnUpdateStadium.FlatAppearance.BorderSize = 0;
            btnUpdateStadium.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnUpdateStadium.FlatStyle = FlatStyle.Flat;
            btnUpdateStadium.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnUpdateStadium.ForeColor = SystemColors.ControlLightLight;
            btnUpdateStadium.Location = new Point(641, 108);
            btnUpdateStadium.Name = "btnUpdateStadium";
            btnUpdateStadium.Size = new Size(91, 32);
            btnUpdateStadium.TabIndex = 34;
            btnUpdateStadium.Text = "Update";
            btnUpdateStadium.UseVisualStyleBackColor = false;
            btnUpdateStadium.Click += btnUpdateStadium_Click;
            // 
            // panel7
            // 
            panel7.BackColor = Color.White;
            panel7.Controls.Add(txtCapacity);
            panel7.Controls.Add(label25);
            panel7.Controls.Add(cmbStadiumCity);
            panel7.Controls.Add(label16);
            panel7.Controls.Add(txtNameStadium);
            panel7.Controls.Add(label24);
            panel7.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel7.Location = new Point(25, 61);
            panel7.Name = "panel7";
            panel7.Size = new Size(589, 167);
            panel7.TabIndex = 32;
            // 
            // txtCapacity
            // 
            txtCapacity.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtCapacity.Location = new Point(390, 47);
            txtCapacity.Name = "txtCapacity";
            txtCapacity.Size = new Size(148, 24);
            txtCapacity.TabIndex = 5;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(390, 24);
            label25.Name = "label25";
            label25.Size = new Size(78, 18);
            label25.TabIndex = 4;
            label25.Text = "Capacity:";
            // 
            // cmbStadiumCity
            // 
            cmbStadiumCity.FormattingEnabled = true;
            cmbStadiumCity.Location = new Point(202, 47);
            cmbStadiumCity.Name = "cmbStadiumCity";
            cmbStadiumCity.Size = new Size(139, 26);
            cmbStadiumCity.TabIndex = 3;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(202, 24);
            label16.Name = "label16";
            label16.Size = new Size(42, 18);
            label16.TabIndex = 2;
            label16.Text = "City:";
            // 
            // txtNameStadium
            // 
            txtNameStadium.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNameStadium.Location = new Point(16, 47);
            txtNameStadium.Name = "txtNameStadium";
            txtNameStadium.Size = new Size(148, 24);
            txtNameStadium.TabIndex = 1;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(16, 24);
            label24.Name = "label24";
            label24.Size = new Size(57, 18);
            label24.TabIndex = 0;
            label24.Text = "Name:";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label10.Location = new Point(310, 17);
            label10.Name = "label10";
            label10.Size = new Size(135, 31);
            label10.TabIndex = 31;
            label10.Text = "Stadiums";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(label17);
            tabPage4.Controls.Add(btnClearAward);
            tabPage4.Controls.Add(btnDeleteAward);
            tabPage4.Controls.Add(panel9);
            tabPage4.Controls.Add(btnAddAward);
            tabPage4.Controls.Add(panel10);
            tabPage4.Controls.Add(btnUpdateAward);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Size = new Size(765, 518);
            tabPage4.TabIndex = 4;
            tabPage4.Text = "Awards";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label17.Location = new Point(342, 16);
            label17.Name = "label17";
            label17.Size = new Size(111, 31);
            label17.TabIndex = 38;
            label17.Text = "Awards";
            // 
            // btnClearAward
            // 
            btnClearAward.BackColor = Color.FromArgb(77, 161, 103);
            btnClearAward.FlatAppearance.BorderSize = 0;
            btnClearAward.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnClearAward.FlatStyle = FlatStyle.Flat;
            btnClearAward.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnClearAward.ForeColor = SystemColors.ControlLightLight;
            btnClearAward.Location = new Point(634, 200);
            btnClearAward.Name = "btnClearAward";
            btnClearAward.Size = new Size(91, 32);
            btnClearAward.TabIndex = 37;
            btnClearAward.Text = "Clear";
            btnClearAward.UseVisualStyleBackColor = false;
            // 
            // btnDeleteAward
            // 
            btnDeleteAward.BackColor = Color.FromArgb(77, 161, 103);
            btnDeleteAward.FlatAppearance.BorderSize = 0;
            btnDeleteAward.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnDeleteAward.FlatStyle = FlatStyle.Flat;
            btnDeleteAward.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnDeleteAward.ForeColor = SystemColors.ControlLightLight;
            btnDeleteAward.Location = new Point(634, 156);
            btnDeleteAward.Name = "btnDeleteAward";
            btnDeleteAward.Size = new Size(91, 32);
            btnDeleteAward.TabIndex = 36;
            btnDeleteAward.Text = "Delete";
            btnDeleteAward.UseVisualStyleBackColor = false;
            // 
            // panel9
            // 
            panel9.BackColor = Color.White;
            panel9.Controls.Add(dataGridView5);
            panel9.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel9.Location = new Point(18, 252);
            panel9.Name = "panel9";
            panel9.Size = new Size(729, 247);
            panel9.TabIndex = 33;
            // 
            // dataGridView5
            // 
            dataGridView5.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView5.Location = new Point(0, 0);
            dataGridView5.Name = "dataGridView5";
            dataGridView5.Size = new Size(729, 247);
            dataGridView5.TabIndex = 0;
            // 
            // btnAddAward
            // 
            btnAddAward.BackColor = Color.FromArgb(77, 161, 103);
            btnAddAward.FlatAppearance.BorderSize = 0;
            btnAddAward.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnAddAward.FlatStyle = FlatStyle.Flat;
            btnAddAward.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnAddAward.ForeColor = SystemColors.ControlLightLight;
            btnAddAward.Location = new Point(634, 68);
            btnAddAward.Name = "btnAddAward";
            btnAddAward.Size = new Size(91, 32);
            btnAddAward.TabIndex = 34;
            btnAddAward.Text = "Add";
            btnAddAward.UseVisualStyleBackColor = false;
            // 
            // panel10
            // 
            panel10.BackColor = Color.White;
            panel10.Controls.Add(txtScope);
            panel10.Controls.Add(Scope);
            panel10.Controls.Add(txtNameAward);
            panel10.Controls.Add(label21);
            panel10.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel10.Location = new Point(18, 68);
            panel10.Name = "panel10";
            panel10.Size = new Size(589, 167);
            panel10.TabIndex = 32;
            // 
            // txtScope
            // 
            txtScope.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtScope.Location = new Point(210, 48);
            txtScope.Name = "txtScope";
            txtScope.Size = new Size(148, 24);
            txtScope.TabIndex = 3;
            // 
            // Scope
            // 
            Scope.AutoSize = true;
            Scope.Location = new Point(210, 25);
            Scope.Name = "Scope";
            Scope.Size = new Size(61, 18);
            Scope.TabIndex = 2;
            Scope.Text = "Scope:";
            // 
            // txtNameAward
            // 
            txtNameAward.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtNameAward.Location = new Point(16, 47);
            txtNameAward.Name = "txtNameAward";
            txtNameAward.Size = new Size(148, 24);
            txtNameAward.TabIndex = 1;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(16, 24);
            label21.Name = "label21";
            label21.Size = new Size(57, 18);
            label21.TabIndex = 0;
            label21.Text = "Name:";
            // 
            // btnUpdateAward
            // 
            btnUpdateAward.BackColor = Color.FromArgb(77, 161, 103);
            btnUpdateAward.FlatAppearance.BorderSize = 0;
            btnUpdateAward.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            btnUpdateAward.FlatStyle = FlatStyle.Flat;
            btnUpdateAward.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            btnUpdateAward.ForeColor = SystemColors.ControlLightLight;
            btnUpdateAward.Location = new Point(634, 112);
            btnUpdateAward.Name = "btnUpdateAward";
            btnUpdateAward.Size = new Size(91, 32);
            btnUpdateAward.TabIndex = 35;
            btnUpdateAward.Text = "Update";
            btnUpdateAward.UseVisualStyleBackColor = false;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(label18);
            tabPage5.Controls.Add(button21);
            tabPage5.Controls.Add(button22);
            tabPage5.Controls.Add(panel11);
            tabPage5.Controls.Add(button23);
            tabPage5.Controls.Add(panel12);
            tabPage5.Controls.Add(button24);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Size = new Size(765, 518);
            tabPage5.TabIndex = 5;
            tabPage5.Text = "Event Types";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label18.Location = new Point(297, 16);
            label18.Name = "label18";
            label18.Size = new Size(177, 31);
            label18.TabIndex = 38;
            label18.Text = "Event Types";
            // 
            // button21
            // 
            button21.BackColor = Color.FromArgb(77, 161, 103);
            button21.FlatAppearance.BorderSize = 0;
            button21.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button21.FlatStyle = FlatStyle.Flat;
            button21.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button21.ForeColor = SystemColors.ControlLightLight;
            button21.Location = new Point(634, 200);
            button21.Name = "button21";
            button21.Size = new Size(91, 32);
            button21.TabIndex = 37;
            button21.Text = "Clear";
            button21.UseVisualStyleBackColor = false;
            // 
            // button22
            // 
            button22.BackColor = Color.FromArgb(77, 161, 103);
            button22.FlatAppearance.BorderSize = 0;
            button22.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button22.FlatStyle = FlatStyle.Flat;
            button22.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button22.ForeColor = SystemColors.ControlLightLight;
            button22.Location = new Point(634, 156);
            button22.Name = "button22";
            button22.Size = new Size(91, 32);
            button22.TabIndex = 36;
            button22.Text = "Delete";
            button22.UseVisualStyleBackColor = false;
            // 
            // panel11
            // 
            panel11.BackColor = Color.White;
            panel11.Controls.Add(dataGridView6);
            panel11.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel11.Location = new Point(18, 252);
            panel11.Name = "panel11";
            panel11.Size = new Size(729, 247);
            panel11.TabIndex = 33;
            // 
            // dataGridView6
            // 
            dataGridView6.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView6.Location = new Point(0, 0);
            dataGridView6.Name = "dataGridView6";
            dataGridView6.Size = new Size(729, 247);
            dataGridView6.TabIndex = 0;
            // 
            // button23
            // 
            button23.BackColor = Color.FromArgb(77, 161, 103);
            button23.FlatAppearance.BorderSize = 0;
            button23.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button23.FlatStyle = FlatStyle.Flat;
            button23.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button23.ForeColor = SystemColors.ControlLightLight;
            button23.Location = new Point(634, 68);
            button23.Name = "button23";
            button23.Size = new Size(91, 32);
            button23.TabIndex = 34;
            button23.Text = "Add";
            button23.UseVisualStyleBackColor = false;
            // 
            // panel12
            // 
            panel12.BackColor = Color.White;
            panel12.Controls.Add(textBox17);
            panel12.Controls.Add(label23);
            panel12.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel12.Location = new Point(18, 68);
            panel12.Name = "panel12";
            panel12.Size = new Size(589, 167);
            panel12.TabIndex = 32;
            // 
            // textBox17
            // 
            textBox17.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox17.Location = new Point(16, 47);
            textBox17.Name = "textBox17";
            textBox17.Size = new Size(148, 24);
            textBox17.TabIndex = 1;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(16, 24);
            label23.Name = "label23";
            label23.Size = new Size(57, 18);
            label23.TabIndex = 0;
            label23.Text = "Name:";
            // 
            // button24
            // 
            button24.BackColor = Color.FromArgb(77, 161, 103);
            button24.FlatAppearance.BorderSize = 0;
            button24.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button24.FlatStyle = FlatStyle.Flat;
            button24.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button24.ForeColor = SystemColors.ControlLightLight;
            button24.Location = new Point(634, 112);
            button24.Name = "button24";
            button24.Size = new Size(91, 32);
            button24.TabIndex = 35;
            button24.Text = "Update";
            button24.UseVisualStyleBackColor = false;
            // 
            // tabPage6
            // 
            tabPage6.Controls.Add(label19);
            tabPage6.Controls.Add(button25);
            tabPage6.Controls.Add(button26);
            tabPage6.Controls.Add(panel13);
            tabPage6.Controls.Add(button27);
            tabPage6.Controls.Add(panel14);
            tabPage6.Controls.Add(button28);
            tabPage6.Location = new Point(4, 24);
            tabPage6.Name = "tabPage6";
            tabPage6.Size = new Size(765, 518);
            tabPage6.TabIndex = 6;
            tabPage6.Text = "Agencies";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Font = new Font("Microsoft Sans Serif", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label19.Location = new Point(312, 15);
            label19.Name = "label19";
            label19.Size = new Size(134, 31);
            label19.TabIndex = 45;
            label19.Text = "Agencies";
            // 
            // button25
            // 
            button25.BackColor = Color.FromArgb(77, 161, 103);
            button25.FlatAppearance.BorderSize = 0;
            button25.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button25.FlatStyle = FlatStyle.Flat;
            button25.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button25.ForeColor = SystemColors.ControlLightLight;
            button25.Location = new Point(634, 201);
            button25.Name = "button25";
            button25.Size = new Size(91, 32);
            button25.TabIndex = 44;
            button25.Text = "Clear";
            button25.UseVisualStyleBackColor = false;
            // 
            // button26
            // 
            button26.BackColor = Color.FromArgb(77, 161, 103);
            button26.FlatAppearance.BorderSize = 0;
            button26.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button26.FlatStyle = FlatStyle.Flat;
            button26.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button26.ForeColor = SystemColors.ControlLightLight;
            button26.Location = new Point(634, 157);
            button26.Name = "button26";
            button26.Size = new Size(91, 32);
            button26.TabIndex = 43;
            button26.Text = "Delete";
            button26.UseVisualStyleBackColor = false;
            // 
            // panel13
            // 
            panel13.BackColor = Color.White;
            panel13.Controls.Add(dataGridView7);
            panel13.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel13.Location = new Point(18, 253);
            panel13.Name = "panel13";
            panel13.Size = new Size(729, 247);
            panel13.TabIndex = 40;
            // 
            // dataGridView7
            // 
            dataGridView7.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView7.Location = new Point(0, 0);
            dataGridView7.Name = "dataGridView7";
            dataGridView7.Size = new Size(729, 247);
            dataGridView7.TabIndex = 0;
            // 
            // button27
            // 
            button27.BackColor = Color.FromArgb(77, 161, 103);
            button27.FlatAppearance.BorderSize = 0;
            button27.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button27.FlatStyle = FlatStyle.Flat;
            button27.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button27.ForeColor = SystemColors.ControlLightLight;
            button27.Location = new Point(634, 69);
            button27.Name = "button27";
            button27.Size = new Size(91, 32);
            button27.TabIndex = 41;
            button27.Text = "Add";
            button27.UseVisualStyleBackColor = false;
            // 
            // panel14
            // 
            panel14.BackColor = Color.White;
            panel14.Controls.Add(textBox14);
            panel14.Controls.Add(label22);
            panel14.Controls.Add(textBox13);
            panel14.Controls.Add(label20);
            panel14.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
            panel14.Location = new Point(18, 69);
            panel14.Name = "panel14";
            panel14.Size = new Size(589, 167);
            panel14.TabIndex = 39;
            // 
            // textBox14
            // 
            textBox14.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox14.Location = new Point(204, 47);
            textBox14.Name = "textBox14";
            textBox14.Size = new Size(148, 24);
            textBox14.TabIndex = 3;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(204, 24);
            label22.Name = "label22";
            label22.Size = new Size(72, 18);
            label22.TabIndex = 2;
            label22.Text = "Country:";
            // 
            // textBox13
            // 
            textBox13.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox13.Location = new Point(16, 47);
            textBox13.Name = "textBox13";
            textBox13.Size = new Size(148, 24);
            textBox13.TabIndex = 1;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(16, 24);
            label20.Name = "label20";
            label20.Size = new Size(57, 18);
            label20.TabIndex = 0;
            label20.Text = "Name:";
            // 
            // button28
            // 
            button28.BackColor = Color.FromArgb(77, 161, 103);
            button28.FlatAppearance.BorderSize = 0;
            button28.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 43, 46);
            button28.FlatStyle = FlatStyle.Flat;
            button28.Font = new Font("Microsoft Sans Serif", 11.9999981F, FontStyle.Bold);
            button28.ForeColor = SystemColors.ControlLightLight;
            button28.Location = new Point(634, 113);
            button28.Name = "button28";
            button28.Size = new Size(91, 32);
            button28.TabIndex = 42;
            button28.Text = "Update";
            button28.UseVisualStyleBackColor = false;
            // 
            // tabPage7
            // 
            tabPage7.Location = new Point(4, 24);
            tabPage7.Name = "tabPage7";
            tabPage7.Size = new Size(765, 518);
            tabPage7.TabIndex = 7;
            tabPage7.Text = "Sponsorship Types";
            tabPage7.UseVisualStyleBackColor = true;
            // 
            // tabPage8
            // 
            tabPage8.Location = new Point(4, 24);
            tabPage8.Name = "tabPage8";
            tabPage8.Padding = new Padding(3);
            tabPage8.Size = new Size(765, 518);
            tabPage8.TabIndex = 8;
            tabPage8.Text = "Sponsors";
            tabPage8.UseVisualStyleBackColor = true;
            // 
            // tabPage9
            // 
            tabPage9.Location = new Point(4, 24);
            tabPage9.Name = "tabPage9";
            tabPage9.Padding = new Padding(3);
            tabPage9.Size = new Size(765, 518);
            tabPage9.TabIndex = 9;
            tabPage9.Text = "Social Media Platforms";
            tabPage9.UseVisualStyleBackColor = true;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(418, 24);
            label15.Name = "label15";
            label15.Size = new Size(78, 18);
            label15.TabIndex = 4;
            label15.Text = "Capacity:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(217, 24);
            label13.Name = "label13";
            label13.Size = new Size(42, 18);
            label13.TabIndex = 2;
            label13.Text = "City:";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(16, 24);
            label14.Name = "label14";
            label14.Size = new Size(57, 18);
            label14.TabIndex = 0;
            label14.Text = "Name:";
            // 
            // uc_AdministrationAndConfiguration
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tabConfederation);
            Name = "uc_AdministrationAndConfiguration";
            Size = new Size(773, 546);
            tabConfederation.ResumeLayout(false);
            Confederations.ResumeLayout(false);
            Confederations.PerformLayout();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvConfederations).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tabCountries.ResumeLayout(false);
            tabCountries.PerformLayout();
            panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCountries).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            tabCity.ResumeLayout(false);
            tabCity.PerformLayout();
            panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvCity).EndInit();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            tabStadium.ResumeLayout(false);
            tabStadium.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStadiums).EndInit();
            panel7.ResumeLayout(false);
            panel7.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            panel9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView5).EndInit();
            panel10.ResumeLayout(false);
            panel10.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            panel11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView6).EndInit();
            panel12.ResumeLayout(false);
            panel12.PerformLayout();
            tabPage6.ResumeLayout(false);
            tabPage6.PerformLayout();
            panel13.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView7).EndInit();
            panel14.ResumeLayout(false);
            panel14.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private TabControl tabConfederation;
        private TabPage Confederations;
        private TabPage tabCountries;
        private TabPage tabCity;
        private TabPage tabStadium;
        private TabPage tabPage4;
        private TabPage tabPage5;
        private TabPage tabPage6;
        private TabPage tabPage7;
        private Panel panel1;
        private TextBox txtNameConfederation;
        private Label label1;
        private Panel panel2;
        private Button btnDeleteConfederation;
        private Button btnUpdateConfederation;
        private Button btnAddConfederation;
        private TextBox txtFoundationYear;
        private Label label3;
        private TextBox txtAcronym;
        private Label label2;
        private Button btnClearConfederation;
        private DataGridView dgvConfederations;
        private Label label4;
        private Label label5;
        private Button btnClearCountry;
        private Button btnDeleteCountry;
        private Panel panel3;
        private DataGridView dgvCountries;
        private Button btnAddCountry;
        private Panel panel4;
        private Label label6;
        private TextBox txtIsoCode;
        private Label label7;
        private TextBox txtNameCountry;
        private Label label8;
        private Button btnUpdateCountry;
        private Label label9;
        private Button btnClearCity;
        private Button btnDeleteCity;
        private Panel panel5;
        private DataGridView dgvCity;
        private Button btnAddCity;
        private Panel panel6;
        private Label label11;
        private TextBox txtNameCity;
        private Label label12;
        private Button btnUpdateCity;
        private Label label10;
        private Button button13;
        private Button button14;
        private DataGridView dataGridView4;
        private Button button15;
        private TextBox textBox12;
        private TextBox textBox11;
        private Label label15;
        private TextBox textBox7;
        private Label label13;
        //private TextBox txtNameStadium;
        private Label label14;
        private Label label17;
        private Button btnClearAward;
        private Button btnDeleteAward;
        private Panel panel9;
        private DataGridView dataGridView5;
        private Button btnAddAward;
        private Panel panel10;
        private TextBox txtNameAward;
        private Label label21;
        private Button btnUpdateAward;
        private TabPage tabPage8;
        private TabPage tabPage9;
        private ComboBox cmbConfederation;
        private Label label18;
        private Button button21;
        private Button button22;
        private Panel panel11;
        private DataGridView dataGridView6;
        private Button button23;
        private Panel panel12;
        private TextBox textBox17;
        private Label label23;
        private Button button24;
        private Label label19;
        private Button button25;
        private Button button26;
        private Panel panel13;
        private DataGridView dataGridView7;
        private Button button27;
        private Panel panel14;
        private TextBox textBox14;
        private Label label22;
        private TextBox textBox13;
        private Label label20;
        private Button button28;
        private ComboBox cmbCountryCity;
       // private ComboBox cmbStadiumCity;
        private Panel panel7;
        private ComboBox cmbStadiumCity;
        private Label label16;
        private TextBox txtNameStadium;
        private Label label24;
        private DataGridView dgvStadiums;
        private Button btnClearStadium;
        private Button btnDeleteStadium;
        private Button btnAddStadium;
        private Button btnUpdateStadium;
        private TextBox txtCapacity;
        private Label label25;
        private TextBox txtScope;
        private Label Scope;
    }
}
