using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Project_Final_Database_Fundamentals
{
    public partial class ucHumanResources : UserControl
    {
        private readonly int _currentUser;
        private int _selectedPlayerId = 0;
        private int _selectedCoachId = 0;
        private int _selectedRefereeId = 0;
        private int _selectedAgentId = 0;
        private int _selectedScoutId = 0;
        private int _selectedStaffId = 0;
        private int _selectedStaffRoleId = 0;
        public ucHumanResources(int currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private async void tabControlHumanResources_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlHumanResources.SelectedTab == tabPagePlayer)
            {
                // 1. Init Fixed Combo (Sync)
                InitializePlayerFootCombo();

                // 2. Load Grid + 2 DB Combos in Parallel
                var tGrid = LoadPlayersAsync();
                var tPos = LoadPositionsForPlayerComboAsync();
                var tCountry = LoadCountriesForPlayerComboAsync();

                await Task.WhenAll(tGrid, tPos, tCountry);
            }
            else if (tabControlHumanResources.SelectedTab == tabPageCoach)
            {
                // Parallel Loading
                var tGrid = LoadCoachesAsync();
                var tCountry = LoadCountriesForCoachComboAsync();

                await Task.WhenAll(tGrid, tCountry);
            }
            else if (tabControlHumanResources.SelectedTab == tabPageReferee)
            {
                // Parallel Loading
                var tGrid = LoadRefereesAsync();
                var tCountry = LoadCountriesForRefereeComboAsync();
                await Task.WhenAll(tGrid, tCountry);
            }
            else if (tabControlHumanResources.SelectedTab == tabPageAgent)
            {
                // Parallel Loading
                var tGrid = LoadAgentsAsync();
                var tAgency = LoadAgenciesForAgentComboAsync();
                var tCountry = LoadCountriesForAgentComboAsync();

                await Task.WhenAll(tGrid, tAgency, tCountry);
            }
            else if (tabControlHumanResources.SelectedTab == tabPageScout)
            {
                // 1. Init fixed Region combo (Sync)
                InitializeScoutRegionCombo();

                // 2. Parallel Loading (Grid + 2 DB Combos)
                var tGrid = LoadScoutsAsync();
                var tTeam = LoadTeamsForScoutComboAsync();
                var tCountry = LoadCountriesForScoutComboAsync();

                await Task.WhenAll(tGrid, tTeam, tCountry);
            }
            else if (tabControlHumanResources.SelectedTab == tabPageStaffMember)
            {
                // Parallel Loading (Grid + 2 Combos)
                var tGrid = LoadStaffMembersAsync();
                var tRole = LoadRolesForStaffComboAsync();
                var tCountry = LoadCountriesForStaffComboAsync();

                await Task.WhenAll(tGrid, tRole, tCountry);
            }
            else if (tabControlHumanResources.SelectedTab == tabPageStaffRole)
            {
                // Load Staff Roles Grid
                await LoadStaffRolesAsync();
            }
        }




        // 1. Initialize Preferred Foot ComboBox (Fixed Options)
        private void InitializePlayerFootCombo()
        {
            cmbPlayerFoot.Items.Clear();
            cmbPlayerFoot.Items.Add("Right");
            cmbPlayerFoot.Items.Add("Left");
            cmbPlayerFoot.Items.Add("Ambidextrous");

            cmbPlayerFoot.SelectedIndex = 0;
        }

        // 2. Load Positions into ComboBox (Assuming table 'position' exists)
        private async Task LoadPositionsForPlayerComboAsync()
        {
            // Adjust column names if your table uses 'position_name' instead of 'name'
            string query = @"SELECT position_id, name FROM position WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["position_id"] = -1;
                        row["name"] = "-- Select Position --";
                        dt.Rows.InsertAt(row, 0);

                        cmbPlayerPosition.DataSource = dt;
                        cmbPlayerPosition.DisplayMember = "name";
                        cmbPlayerPosition.ValueMember = "position_id";
                        cmbPlayerPosition.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading positions: " + ex.Message); }
        }

        // 3. Load Countries into ComboBox
        private async Task LoadCountriesForPlayerComboAsync()
        {
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        cmbPlayerCountry.DataSource = dt;
                        cmbPlayerCountry.DisplayMember = "name";
                        cmbPlayerCountry.ValueMember = "country_id";
                        cmbPlayerCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        // 4. Load Main Grid
        private async Task LoadPlayersAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
    SELECT 
        p.player_id,
        p.first_name,
        p.last_name,
        p.date_of_birth,
        p.primary_position_id,
        pos.name AS position_name,
        p.preferred_foot,
        p.height,
        p.weight,
        p.country_id,
        c.name AS country_name
    FROM player p
    INNER JOIN position pos ON p.primary_position_id = pos.position_id
    INNER JOIN ""country"" c ON p.country_id = c.country_id
    WHERE p.is_active = true
    ORDER BY player_id";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvPlayer.DataSource = dt;

                        FormatPlayerGrid(dgvPlayer);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading players: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatPlayerGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "player_id", "ID" },
        { "first_name", "First Name" },
        { "last_name", "Last Name" },
        { "date_of_birth", "Date of Birth" },
        { "position_name", "Position" },
        { "preferred_foot", "Foot" },
        { "height", "Height" },
        { "weight", "Weight" },
        { "country_name", "Country" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Hide foreign keys
            string[] hiddenCols = { "primary_position_id", "country_id" };
            foreach (var colName in hiddenCols)
            {
                if (dgv.Columns.Contains(colName))
                    dgv.Columns[colName].Visible = false;
            }

            // Specific formatting for the Main ID
            if (dgv.Columns.Contains("player_id"))
            {
                dgv.Columns["player_id"].Visible = true;
                dgv.Columns["player_id"].DisplayIndex = 0;
                dgv.Columns["player_id"].Width = 60;
            }

            // Format Date
            if (dgv.Columns.Contains("date_of_birth"))
            {
                dgv.Columns["date_of_birth"].DefaultCellStyle.Format = "d"; // Short date
            }

            // Optimize width for small numerical columns
            string[] smallCols = { "height", "weight", "preferred_foot" };
            foreach (var col in smallCols)
            {
                if (dgv.Columns.Contains(col))
                    dgv.Columns[col].Width = 60;
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }

        private async void btnAddPlayer_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbPlayerPosition.SelectedValue == -1 ||
                (int)cmbPlayerCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Position and a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbPlayerFoot.SelectedItem == null)
            {
                MessageBox.Show("Please select a Preferred Foot.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validate Text
            string firstName = txtPlayerFirstName.Text.Trim();
            string lastName = txtPlayerLastName.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("First and Last Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Get Values
            int positionId = (int)cmbPlayerPosition.SelectedValue;
            int countryId = (int)cmbPlayerCountry.SelectedValue;
            string foot = cmbPlayerFoot.SelectedItem.ToString();
            decimal height = nudPlayerHeigth.Value;
            decimal weight = nudPlayerWeight.Value;
            DateTime dob = dtpPlayerBirth.Value;

            string query = @"
        INSERT INTO player 
        (first_name, last_name, date_of_birth, primary_position_id, preferred_foot, height, weight, country_id, created_by) 
        VALUES 
        (@fname, @lname, @dob, @posId, @foot, @height, @weight, @countryId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@dob", dob);
                    command.Parameters.AddWithValue("@posId", positionId);
                    command.Parameters.AddWithValue("@foot", foot);
                    command.Parameters.AddWithValue("@height", height);
                    command.Parameters.AddWithValue("@weight", weight);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Player added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayersAsync();
                        btnClearPlayer_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdatePlayer_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerId == 0)
            {
                MessageBox.Show("Please select a player to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbPlayerPosition.SelectedValue == -1 ||
                (int)cmbPlayerCountry.SelectedValue == -1 ||
                cmbPlayerFoot.SelectedItem == null)
            {
                MessageBox.Show("All selections are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName = txtPlayerFirstName.Text.Trim();
            string lastName = txtPlayerLastName.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("Names are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE player SET 
            first_name = @fname, 
            last_name = @lname, 
            date_of_birth = @dob, 
            primary_position_id = @posId, 
            preferred_foot = @foot, 
            height = @height, 
            weight = @weight, 
            country_id = @countryId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            player_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@dob", dtpPlayerBirth.Value);
                    command.Parameters.AddWithValue("@posId", (int)cmbPlayerPosition.SelectedValue);
                    command.Parameters.AddWithValue("@foot", cmbPlayerFoot.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@height", nudPlayerHeigth.Value);
                    command.Parameters.AddWithValue("@weight", nudPlayerWeight.Value);
                    command.Parameters.AddWithValue("@countryId", (int)cmbPlayerCountry.SelectedValue);
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedPlayerId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Player updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayersAsync();
                        btnClearPlayer_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating player: " + ex.Message); }
        }

        private async void btnDeletePlayer_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerId == 0)
            {
                MessageBox.Show("Please select a player to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE player SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            player_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedPlayerId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Player deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadPlayersAsync();
                            btnClearPlayer_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting player: " + ex.Message); }
            }
        }

        private void dgvPlayer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvPlayer.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["player_id"].Value == null || row.Cells["player_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedPlayerId = Convert.ToInt32(row.Cells["player_id"].Value);

                    // Map Texts
                    txtPlayerFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
                    txtPlayerLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";

                    // Map Combos
                    if (row.Cells["primary_position_id"].Value != DBNull.Value)
                        cmbPlayerPosition.SelectedValue = Convert.ToInt32(row.Cells["primary_position_id"].Value);

                    if (row.Cells["country_id"].Value != DBNull.Value)
                        cmbPlayerCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                    if (row.Cells["preferred_foot"].Value != DBNull.Value)
                        cmbPlayerFoot.SelectedItem = row.Cells["preferred_foot"].Value.ToString();

                    // Map Numerics
                    if (row.Cells["height"].Value != DBNull.Value)
                        nudPlayerHeigth.Value = Convert.ToDecimal(row.Cells["height"].Value);

                    if (row.Cells["weight"].Value != DBNull.Value)
                        nudPlayerWeight.Value = Convert.ToDecimal(row.Cells["weight"].Value);

                    // Map Date
                    if (row.Cells["date_of_birth"].Value != DBNull.Value)
                        dtpPlayerBirth.Value = Convert.ToDateTime(row.Cells["date_of_birth"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting player: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearPlayer_Click(object sender, EventArgs e)
        {
            _selectedPlayerId = 0;
            txtPlayerFirstName.Clear();
            txtPlayerLastName.Clear();
            nudPlayerHeigth.Value = 0;
            nudPlayerWeight.Value = 0;
            dtpPlayerBirth.Value = DateTime.Now;

            if (cmbPlayerPosition.Items.Count > 0) cmbPlayerPosition.SelectedIndex = 0;
            if (cmbPlayerCountry.Items.Count > 0) cmbPlayerCountry.SelectedIndex = 0;
            if (cmbPlayerFoot.Items.Count > 0) cmbPlayerFoot.SelectedIndex = 0;

            dgvPlayer.ClearSelection();
        }







        // 1. Load Countries into ComboBox
        private async Task LoadCountriesForCoachComboAsync()
        {
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        cmbCoachCountry.DataSource = dt;
                        cmbCoachCountry.DisplayMember = "name";
                        cmbCoachCountry.ValueMember = "country_id";
                        cmbCoachCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        // 2. Load Main Grid
        private async Task LoadCoachesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
    SELECT 
        c.coach_id, 
        c.first_name, 
        c.last_name, 
        c.license_level,
        c.date_of_birth,
        c.country_id,
        co.name AS country_name
    FROM coach c
    INNER JOIN ""country"" co ON c.country_id = co.country_id
    WHERE c.is_active = true
    ORDER BY c.last_name, c.first_name"; // Alphabetical sort is better for people

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvCoach.DataSource = dt;

                        FormatCoachGrid(dgvCoach);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading coaches: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatCoachGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "coach_id", "ID" },
        { "first_name", "First Name" },
        { "last_name", "Last Name" },
        { "license_level", "License" },
        { "date_of_birth", "Date of Birth" },
        { "country_name", "Country" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Hide foreign keys
            if (dgv.Columns.Contains("country_id"))
                dgv.Columns["country_id"].Visible = false;

            // Specific formatting for the Main ID
            if (dgv.Columns.Contains("coach_id"))
            {
                dgv.Columns["coach_id"].Visible = true;
                dgv.Columns["coach_id"].DisplayIndex = 0;
                dgv.Columns["coach_id"].Width = 60;
            }

            // Format Date
            if (dgv.Columns.Contains("date_of_birth"))
            {
                dgv.Columns["date_of_birth"].DefaultCellStyle.Format = "d"; // Short date
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }
        private async void btnAddCoach_Click(object sender, EventArgs e)
        {
            string firstName = txtCoachFirstName.Text.Trim();
            string lastName = txtCoachLastName.Text.Trim();
            string license = txtCoachLicenseLevel.Text.Trim();
            DateTime dob = dtpCoachBirth.Value;

            // 1. Validate ComboBox
            if ((int)cmbCoachCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int countryId = (int)cmbCoachCountry.SelectedValue;

            // 2. Validate Text
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(license))
            {
                MessageBox.Show("First Name, Last Name, and License Level are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO coach 
        (first_name, last_name, license_level, date_of_birth, country_id, created_by) 
        VALUES 
        (@fname, @lname, @license, @dob, @countryId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@license", license);
                    command.Parameters.AddWithValue("@dob", dob);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Coach added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCoachesAsync();
                        btnClearCoach_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateCoach_Click(object sender, EventArgs e)
        {
            if (_selectedCoachId == 0)
            {
                MessageBox.Show("Please select a coach to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbCoachCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName = txtCoachFirstName.Text.Trim();
            string lastName = txtCoachLastName.Text.Trim();
            string license = txtCoachLicenseLevel.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(license))
            {
                MessageBox.Show("All text fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE coach SET 
            first_name = @fname, 
            last_name = @lname, 
            license_level = @license,
            date_of_birth = @dob, 
            country_id = @countryId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            coach_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@license", license);
                    command.Parameters.AddWithValue("@dob", dtpCoachBirth.Value);
                    command.Parameters.AddWithValue("@countryId", (int)cmbCoachCountry.SelectedValue);
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedCoachId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Coach updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCoachesAsync();
                        btnClearCoach_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating coach: " + ex.Message); }
        }

        private async void btnDeleteCoach_Click(object sender, EventArgs e)
        {
            if (_selectedCoachId == 0)
            {
                MessageBox.Show("Please select a coach to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE coach SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            coach_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedCoachId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Coach deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCoachesAsync();
                            btnClearCoach_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting coach: " + ex.Message); }
            }
        }

        private void dgvCoach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvCoach.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["coach_id"].Value == null || row.Cells["coach_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedCoachId = Convert.ToInt32(row.Cells["coach_id"].Value);

                    // Map Texts (with null checks)
                    txtCoachFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
                    txtCoachLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";
                    txtCoachLicenseLevel.Text = row.Cells["license_level"].Value?.ToString() ?? "";

                    // Map Combo
                    if (row.Cells["country_id"].Value != DBNull.Value)
                        cmbCoachCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                    // Map Date
                    if (row.Cells["date_of_birth"].Value != DBNull.Value)
                        dtpCoachBirth.Value = Convert.ToDateTime(row.Cells["date_of_birth"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting coach: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearCoach_Click(object sender, EventArgs e)
        {
            _selectedCoachId = 0;
            txtCoachFirstName.Clear();
            txtCoachLastName.Clear();
            txtCoachLicenseLevel.Clear();
            dtpCoachBirth.Value = DateTime.Now;

            if (cmbCoachCountry.Items.Count > 0) cmbCoachCountry.SelectedIndex = 0;

            dgvCoach.ClearSelection();
        }





        // 1. Load Countries into ComboBox
        private async Task LoadCountriesForRefereeComboAsync()
        {
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        cmbRefereeCountry.DataSource = dt;
                        cmbRefereeCountry.DisplayMember = "name";
                        cmbRefereeCountry.ValueMember = "country_id";
                        cmbRefereeCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        // 2. Load Main Grid
        private async Task LoadRefereesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
    SELECT 
        r.referee_id, 
        r.first_name, 
        r.last_name, 
        r.certification_level,
        r.date_of_birth,
        r.country_id,
        c.name AS country_name
    FROM referee r
    INNER JOIN ""country"" c ON r.country_id = c.country_id
    WHERE r.is_active = true
    ORDER BY r.last_name, r.first_name"; // Keep alphabetical sort

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvReferee.DataSource = dt;

                        FormatRefereeGrid(dgvReferee);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading referees: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatRefereeGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "referee_id", "ID" },
        { "first_name", "First Name" },
        { "last_name", "Last Name" },
        { "certification_level", "Certification" },
        { "date_of_birth", "Date of Birth" },
        { "country_name", "Country" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Hide foreign keys
            if (dgv.Columns.Contains("country_id"))
                dgv.Columns["country_id"].Visible = false;

            // Specific formatting for the Main ID
            if (dgv.Columns.Contains("referee_id"))
            {
                dgv.Columns["referee_id"].Visible = true;
                dgv.Columns["referee_id"].DisplayIndex = 0;
                dgv.Columns["referee_id"].Width = 60;
            }

            // Format Date
            if (dgv.Columns.Contains("date_of_birth"))
            {
                dgv.Columns["date_of_birth"].DefaultCellStyle.Format = "d"; // Short date
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }
        private async void btnAddReferee_Click(object sender, EventArgs e)
        {
            string firstName = txtRefereeFirstName.Text.Trim();
            string lastName = txtRefereeLastName.Text.Trim();
            string level = txtRefereeCertification.Text.Trim();
            DateTime dob = dtpRefereeBirth.Value;

            // 1. Validate ComboBox
            if ((int)cmbRefereeCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int countryId = (int)cmbRefereeCountry.SelectedValue;

            // 2. Validate Text
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(level))
            {
                MessageBox.Show("First Name, Last Name, and Certification Level are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO referee 
        (first_name, last_name, certification_level, date_of_birth, country_id, created_by) 
        VALUES 
        (@fname, @lname, @level, @dob, @countryId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@level", level);
                    command.Parameters.AddWithValue("@dob", dob);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Referee added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadRefereesAsync();
                        btnClearReferee_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateReferee_Click(object sender, EventArgs e)
        {
            if (_selectedRefereeId == 0)
            {
                MessageBox.Show("Please select a referee to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbRefereeCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName = txtRefereeFirstName.Text.Trim();
            string lastName = txtRefereeLastName.Text.Trim();
            string level = txtRefereeCertification.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(level))
            {
                MessageBox.Show("All text fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE referee SET 
            first_name = @fname, 
            last_name = @lname, 
            certification_level = @level,
            date_of_birth = @dob, 
            country_id = @countryId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            referee_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@level", level);
                    command.Parameters.AddWithValue("@dob", dtpRefereeBirth.Value);
                    command.Parameters.AddWithValue("@countryId", (int)cmbRefereeCountry.SelectedValue);
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedRefereeId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Referee updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadRefereesAsync();
                        btnClearReferee_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating referee: " + ex.Message); }
        }

        private async void btnDeleteReferee_Click(object sender, EventArgs e)
        {
            if (_selectedRefereeId == 0)
            {
                MessageBox.Show("Please select a referee to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE referee SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            referee_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedRefereeId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Referee deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadRefereesAsync();
                            btnClearReferee_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting referee: " + ex.Message); }
            }
        }

        private void dgvReferee_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Valid Row Check
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvReferee.Rows[e.RowIndex];

                    // 2. New Row Check
                    if (row.IsNewRow) return;

                    // 3. Null ID Check
                    if (row.Cells["referee_id"].Value == null || row.Cells["referee_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedRefereeId = Convert.ToInt32(row.Cells["referee_id"].Value);

                    // Map Texts (Null Safe)
                    txtRefereeFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
                    txtRefereeLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";
                    txtRefereeCertification.Text = row.Cells["certification_level"].Value?.ToString() ?? "";

                    // Map Combo
                    if (row.Cells["country_id"].Value != DBNull.Value)
                        cmbRefereeCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                    // Map Date
                    if (row.Cells["date_of_birth"].Value != DBNull.Value)
                        dtpRefereeBirth.Value = Convert.ToDateTime(row.Cells["date_of_birth"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting referee: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearReferee_Click(object sender, EventArgs e)
        {
            _selectedRefereeId = 0;
            txtRefereeFirstName.Clear();
            txtRefereeLastName.Clear();
            txtRefereeCertification.Clear();
            dtpRefereeBirth.Value = DateTime.Now;

            if (cmbRefereeCountry.Items.Count > 0) cmbRefereeCountry.SelectedIndex = 0;

            dgvReferee.ClearSelection();
        }






        // 1. Load Agencies into ComboBox
        private async Task LoadAgenciesForAgentComboAsync()
        {
            string query = @"SELECT agency_id, name FROM agency WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["agency_id"] = -1;
                        row["name"] = "-- Select Agency --";
                        dt.Rows.InsertAt(row, 0);

                        cmbAgentAgency.DataSource = dt;
                        cmbAgentAgency.DisplayMember = "name";
                        cmbAgentAgency.ValueMember = "agency_id";
                        cmbAgentAgency.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading agencies: " + ex.Message); }
        }

        // 2. Load Countries into ComboBox
        private async Task LoadCountriesForAgentComboAsync()
        {
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        cmbAgentCountry.DataSource = dt;
                        cmbAgentCountry.DisplayMember = "name";
                        cmbAgentCountry.ValueMember = "country_id";
                        cmbAgentCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadAgentsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
    SELECT 
        a.agent_id, 
        a.first_name, 
        a.last_name, 
        a.license_number,
        a.date_of_birth,
        a.agency_id,
        ag.name AS agency_name,
        a.country_id,
        c.name AS country_name
    FROM agent a
    INNER JOIN agency ag ON a.agency_id = ag.agency_id
    INNER JOIN ""country"" c ON a.country_id = c.country_id
    WHERE a.is_active = true
    ORDER BY a.last_name, a.first_name"; // Keep alphabetical sort

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvAgent.DataSource = dt;

                        FormatAgentGrid(dgvAgent);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading agents: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatAgentGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "agent_id", "ID" },
        { "first_name", "First Name" },
        { "last_name", "Last Name" },
        { "license_number", "License No." },
        { "date_of_birth", "Date of Birth" },
        { "agency_name", "Agency" },
        { "country_name", "Country" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Hide foreign keys
            string[] hiddenCols = { "agency_id", "country_id" };
            foreach (var colName in hiddenCols)
            {
                if (dgv.Columns.Contains(colName))
                    dgv.Columns[colName].Visible = false;
            }

            // Specific formatting for the Main ID
            if (dgv.Columns.Contains("agent_id"))
            {
                dgv.Columns["agent_id"].Visible = true;
                dgv.Columns["agent_id"].DisplayIndex = 0;
                dgv.Columns["agent_id"].Width = 60;
            }

            // Format Date
            if (dgv.Columns.Contains("date_of_birth"))
            {
                dgv.Columns["date_of_birth"].DefaultCellStyle.Format = "d"; // Short date
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }
        private async void btnAddAgent_Click(object sender, EventArgs e)
        {
            string firstName = txtAgentFirstName.Text.Trim();
            string lastName = txtAgentLastName.Text.Trim();
            string license = txtAgentLicense.Text.Trim();
            DateTime dob = dtpAgentBirth.Value;

            // 1. Validate ComboBoxes
            if ((int)cmbAgentAgency.SelectedValue == -1 || (int)cmbAgentCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select an Agency and a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int agencyId = (int)cmbAgentAgency.SelectedValue;
            int countryId = (int)cmbAgentCountry.SelectedValue;

            // 2. Validate Text
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(license))
            {
                MessageBox.Show("First Name, Last Name, and License Number are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO agent 
        (agency_id, license_number, country_id, first_name, last_name, date_of_birth, created_by) 
        VALUES 
        (@agencyId, @license, @countryId, @fname, @lname, @dob, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@agencyId", agencyId);
                    command.Parameters.AddWithValue("@license", license);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@dob", dob);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Agent added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAgentsAsync();
                        btnClearAgent_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This License Number already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateAgent_Click(object sender, EventArgs e)
        {
            if (_selectedAgentId == 0)
            {
                MessageBox.Show("Please select an agent to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbAgentAgency.SelectedValue == -1 || (int)cmbAgentCountry.SelectedValue == -1)
            {
                MessageBox.Show("Combobox selections are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName = txtAgentFirstName.Text.Trim();
            string lastName = txtAgentLastName.Text.Trim();
            string license = txtAgentLicense.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(license))
            {
                MessageBox.Show("All text fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE agent SET 
            agency_id = @agencyId, 
            license_number = @license, 
            country_id = @countryId, 
            first_name = @fname, 
            last_name = @lname, 
            date_of_birth = @dob,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            agent_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@agencyId", (int)cmbAgentAgency.SelectedValue);
                    command.Parameters.AddWithValue("@license", license);
                    command.Parameters.AddWithValue("@countryId", (int)cmbAgentCountry.SelectedValue);
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@dob", dtpAgentBirth.Value);
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedAgentId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Agent updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAgentsAsync();
                        btnClearAgent_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating agent: " + ex.Message); }
        }

        private async void btnDeleteAgent_Click(object sender, EventArgs e)
        {
            if (_selectedAgentId == 0)
            {
                MessageBox.Show("Please select an agent to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE agent SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            agent_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedAgentId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Agent deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadAgentsAsync();
                            btnClearAgent_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting agent: " + ex.Message); }
            }
        }

        private void dgvAgent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Valid Row Check
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvAgent.Rows[e.RowIndex];

                    // 2. Ignore New Row
                    if (row.IsNewRow) return;

                    // 3. Null ID Check
                    if (row.Cells["agent_id"].Value == null || row.Cells["agent_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedAgentId = Convert.ToInt32(row.Cells["agent_id"].Value);

                    // Map Texts (Null Safe)
                    txtAgentFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
                    txtAgentLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";
                    txtAgentLicense.Text = row.Cells["license_number"].Value?.ToString() ?? "";

                    // Map Combos
                    if (row.Cells["agency_id"].Value != null && row.Cells["agency_id"].Value != DBNull.Value)
                        cmbAgentAgency.SelectedValue = Convert.ToInt32(row.Cells["agency_id"].Value);

                    if (row.Cells["country_id"].Value != null && row.Cells["country_id"].Value != DBNull.Value)
                        cmbAgentCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                    // Map Date
                    if (row.Cells["date_of_birth"].Value != null && row.Cells["date_of_birth"].Value != DBNull.Value)
                        dtpAgentBirth.Value = Convert.ToDateTime(row.Cells["date_of_birth"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting agent: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearAgent_Click(object sender, EventArgs e)
        {
            _selectedAgentId = 0;
            txtAgentFirstName.Clear();
            txtAgentLastName.Clear();
            txtAgentLicense.Clear();
            dtpAgentBirth.Value = DateTime.Now;

            if (cmbAgentAgency.Items.Count > 0) cmbAgentAgency.SelectedIndex = 0;
            if (cmbAgentCountry.Items.Count > 0) cmbAgentCountry.SelectedIndex = 0;

            dgvAgent.ClearSelection();
        }






        // 1. Initialize Region ComboBox (Fixed Options)
        private void InitializeScoutRegionCombo()
        {
            cmbScoutRegion.Items.Clear();

            // Standard Scouting Regions
            cmbScoutRegion.Items.Add("Europe");
            cmbScoutRegion.Items.Add("South America");
            cmbScoutRegion.Items.Add("North America");
            cmbScoutRegion.Items.Add("Africa");
            cmbScoutRegion.Items.Add("Asia");
            cmbScoutRegion.Items.Add("Oceania");
            cmbScoutRegion.Items.Add("Global"); // For head scouts

            cmbScoutRegion.SelectedIndex = 0;
        }

        // 2. Load Teams (Employing Team)
        private async Task LoadTeamsForScoutComboAsync()
        {
            string query = @"SELECT team_id, name FROM team WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["team_id"] = -1;
                        row["name"] = "-- Select Team --";
                        dt.Rows.InsertAt(row, 0);

                        cmbScoutTeam.DataSource = dt;
                        cmbScoutTeam.DisplayMember = "name";
                        cmbScoutTeam.ValueMember = "team_id";
                        cmbScoutTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Countries
        private async Task LoadCountriesForScoutComboAsync()
        {
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        cmbScoutCountry.DataSource = dt;
                        cmbScoutCountry.DisplayMember = "name";
                        cmbScoutCountry.ValueMember = "country_id";
                        cmbScoutCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        private async Task LoadScoutsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // Se agregó s.date_of_birth a la consulta
            string query = @"
    SELECT 
        s.scout_id, 
        s.first_name, 
        s.last_name, 
        s.date_of_birth, 
        s.region,
        s.employing_team_id,
        t.name AS team_name,
        s.country_id,
        c.name AS country_name
    FROM scout s
    INNER JOIN team t ON s.employing_team_id = t.team_id
    INNER JOIN ""country"" c ON s.country_id = c.country_id
    WHERE s.is_active = true
    ORDER BY s.last_name, s.first_name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvScout.DataSource = dt;

                        FormatScoutGrid(dgvScout);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading scouts: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatScoutGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "scout_id", "ID" },
        { "first_name", "First Name" },
        { "last_name", "Last Name" },
        { "date_of_birth", "Birth Date" }, // <--- Nuevo Header
        { "region", "Region" },
        { "team_name", "Team" },
        { "country_name", "Country" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Ocultar llaves foráneas
            string[] hiddenCols = { "employing_team_id", "country_id" };
            foreach (var colName in hiddenCols)
            {
                if (dgv.Columns.Contains(colName))
                    dgv.Columns[colName].Visible = false;
            }

            // Configuración específica ID
            if (dgv.Columns.Contains("scout_id"))
            {
                dgv.Columns["scout_id"].Visible = true;
                dgv.Columns["scout_id"].DisplayIndex = 0;
                dgv.Columns["scout_id"].Width = 60;
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }
        private async void btnAddScout_Click(object sender, EventArgs e)
        {
            string firstName = txtScoutFirstName.Text.Trim();
            string lastName = txtScoutLastName.Text.Trim();
            DateTime dob = dtpScoutBirth.Value; // <--- Obtener Fecha

            // 1. Validate Combos
            if ((int)cmbScoutTeam.SelectedValue == -1 || (int)cmbScoutCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Team and a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbScoutRegion.SelectedItem == null)
            {
                MessageBox.Show("Please select a Region.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validate Text
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("First and Last Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Get Values
            int teamId = (int)cmbScoutTeam.SelectedValue;
            int countryId = (int)cmbScoutCountry.SelectedValue;
            string region = cmbScoutRegion.SelectedItem.ToString();

            // Query actualizado con date_of_birth
            string query = @"
    INSERT INTO scout 
    (employing_team_id, first_name, last_name, date_of_birth, country_id, region, created_by) 
    VALUES 
    (@teamId, @fname, @lname, @dob, @countryId, @region, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@dob", dob); // <--- Parámetro fecha
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@region", region);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Scout added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadScoutsAsync();
                        btnClearScout_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateScout_Click(object sender, EventArgs e)
        {
            if (_selectedScoutId == 0)
            {
                MessageBox.Show("Please select a scout to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbScoutTeam.SelectedValue == -1 ||
                (int)cmbScoutCountry.SelectedValue == -1 ||
                cmbScoutRegion.SelectedItem == null)
            {
                MessageBox.Show("All dropdowns are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName = txtScoutFirstName.Text.Trim();
            string lastName = txtScoutLastName.Text.Trim();
            DateTime dob = dtpScoutBirth.Value; // <--- Obtener Fecha

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("Names are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Query actualizado con date_of_birth
            string query = @"
    UPDATE scout SET 
        employing_team_id = @teamId, 
        first_name = @fname, 
        last_name = @lname, 
        date_of_birth = @dob,
        country_id = @countryId, 
        region = @region,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = @updaterId
    WHERE 
        scout_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", (int)cmbScoutTeam.SelectedValue);
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@dob", dob); // <--- Parámetro fecha
                    command.Parameters.AddWithValue("@countryId", (int)cmbScoutCountry.SelectedValue);
                    command.Parameters.AddWithValue("@region", cmbScoutRegion.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedScoutId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Scout updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadScoutsAsync();
                        btnClearScout_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating scout: " + ex.Message); }
        }

        private async void btnDeleteScout_Click(object sender, EventArgs e)
        {
            if (_selectedScoutId == 0)
            {
                MessageBox.Show("Please select a scout to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE scout SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            scout_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedScoutId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Scout deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadScoutsAsync();
                            btnClearScout_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting scout: " + ex.Message); }
            }
        }

        private void dgvScout_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Valid Row
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvScout.Rows[e.RowIndex];

                    // 2. Not New Row
                    if (row.IsNewRow) return;

                    // 3. Not Null ID
                    if (row.Cells["scout_id"].Value == null || row.Cells["scout_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedScoutId = Convert.ToInt32(row.Cells["scout_id"].Value);

                    // Map Texts (Safe)
                    txtScoutFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
                    txtScoutLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";

                    // --- MAPEAR FECHA ---
                    if (row.Cells["date_of_birth"].Value != null && row.Cells["date_of_birth"].Value != DBNull.Value)
                    {
                        dtpScoutBirth.Value = Convert.ToDateTime(row.Cells["date_of_birth"].Value);
                    }
                    else
                    {
                        dtpScoutBirth.Value = DateTime.Now; // Default si es nulo
                    }

                    // Map Region Combo (String)
                    if (row.Cells["region"].Value != null && row.Cells["region"].Value != DBNull.Value)
                    {
                        cmbScoutRegion.SelectedItem = row.Cells["region"].Value.ToString();
                    }

                    // Map Team Combo
                    if (row.Cells["employing_team_id"].Value != null && row.Cells["employing_team_id"].Value != DBNull.Value)
                    {
                        cmbScoutTeam.SelectedValue = Convert.ToInt32(row.Cells["employing_team_id"].Value);
                    }

                    // Map Country Combo
                    if (row.Cells["country_id"].Value != null && row.Cells["country_id"].Value != DBNull.Value)
                    {
                        cmbScoutCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting scout: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearScout_Click(object sender, EventArgs e)
        {
            _selectedScoutId = 0;
            txtScoutFirstName.Clear();
            txtScoutLastName.Clear();
            dtpScoutBirth.Value = DateTime.Now; 

            if (cmbScoutTeam.Items.Count > 0) cmbScoutTeam.SelectedIndex = 0;
            if (cmbScoutCountry.Items.Count > 0) cmbScoutCountry.SelectedIndex = 0;
            if (cmbScoutRegion.Items.Count > 0) cmbScoutRegion.SelectedIndex = 0;

            dgvScout.ClearSelection();
        }







        // 1. Load Roles into ComboBox (Assuming table 'role' exists)
        private async Task LoadRolesForStaffComboAsync()
        {
            // Change 'role' to 'staff_role' if that is your specific table name
            string query = @"SELECT role_id, role_name FROM staff_role WHERE is_active = true ORDER BY role_name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["role_id"] = -1;
                        row["role_name"] = "-- Select Role --";
                        dt.Rows.InsertAt(row, 0);

                        cmbStaffMemberRole.DataSource = dt;
                        cmbStaffMemberRole.DisplayMember = "role_name";
                        cmbStaffMemberRole.ValueMember = "role_id";
                        cmbStaffMemberRole.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading roles: " + ex.Message); }
        }

        // 2. Load Countries into ComboBox
        private async Task LoadCountriesForStaffComboAsync()
        {
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        cmbStaffMemberCountry.DataSource = dt;
                        cmbStaffMemberCountry.DisplayMember = "name";
                        cmbStaffMemberCountry.ValueMember = "country_id";
                        cmbStaffMemberCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadStaffMembersAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
    SELECT 
        sm.staff_member_id, 
        sm.first_name, 
        sm.last_name, 
        sm.date_of_birth,
        sm.role_id,
        r.role_name AS role_name,
        sm.country_id,
        c.name AS country_name
    FROM staff_member sm
    INNER JOIN staff_role r ON sm.role_id = r.role_id
    INNER JOIN ""country"" c ON sm.country_id = c.country_id
    WHERE sm.is_active = true
    ORDER BY sm.last_name, sm.first_name"; // Keep alphabetical sort

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvStaffMember.DataSource = dt;

                        FormatStaffMemberGrid(dgvStaffMember);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading staff: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatStaffMemberGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "staff_member_id", "ID" },
        { "first_name", "First Name" },
        { "last_name", "Last Name" },
        { "date_of_birth", "Date of Birth" },
        { "role_name", "Role" },
        { "country_name", "Country" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Hide foreign keys
            string[] hiddenCols = { "role_id", "country_id" };
            foreach (var colName in hiddenCols)
            {
                if (dgv.Columns.Contains(colName))
                    dgv.Columns[colName].Visible = false;
            }

            // Specific formatting for the Main ID
            if (dgv.Columns.Contains("staff_member_id"))
            {
                dgv.Columns["staff_member_id"].Visible = true;
                dgv.Columns["staff_member_id"].DisplayIndex = 0;
                dgv.Columns["staff_member_id"].Width = 60;
            }

            // Format Date
            if (dgv.Columns.Contains("date_of_birth"))
            {
                dgv.Columns["date_of_birth"].DefaultCellStyle.Format = "d"; // Short date
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }
        private async void btnAddStaffMember_Click(object sender, EventArgs e)
        {
            string firstName = txtStaffMemberFirstName.Text.Trim();
            string lastName = txtStaffMemberLastName.Text.Trim();
            DateTime dob = dtpStaffMemberBirth.Value;

            // 1. Validate Combos
            if ((int)cmbStaffMemberRole.SelectedValue == -1 || (int)cmbStaffMemberCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Role and a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int roleId = (int)cmbStaffMemberRole.SelectedValue;
            int countryId = (int)cmbStaffMemberCountry.SelectedValue;

            // 2. Validate Text
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("First and Last Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO staff_member 
        (role_id, first_name, last_name, country_id, date_of_birth, created_by) 
        VALUES 
        (@roleId, @fname, @lname, @countryId, @dob, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@roleId", roleId);
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@dob", dob);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Staff Member added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadStaffMembersAsync();
                        btnClearStaffMember_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateStaffMember_Click(object sender, EventArgs e)
        {
            if (_selectedStaffId == 0)
            {
                MessageBox.Show("Please select a staff member to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbStaffMemberRole.SelectedValue == -1 || (int)cmbStaffMemberCountry.SelectedValue == -1)
            {
                MessageBox.Show("Combobox selections are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string firstName = txtStaffMemberFirstName.Text.Trim();
            string lastName = txtStaffMemberLastName.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                MessageBox.Show("Names are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE staff_member SET 
            role_id = @roleId, 
            first_name = @fname, 
            last_name = @lname, 
            country_id = @countryId, 
            date_of_birth = @dob,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            staff_member_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@roleId", (int)cmbStaffMemberRole.SelectedValue);
                    command.Parameters.AddWithValue("@fname", firstName);
                    command.Parameters.AddWithValue("@lname", lastName);
                    command.Parameters.AddWithValue("@countryId", (int)cmbStaffMemberCountry.SelectedValue);
                    command.Parameters.AddWithValue("@dob", dtpStaffMemberBirth.Value);
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedStaffId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Staff Member updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadStaffMembersAsync();
                        btnClearStaffMember_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating staff: " + ex.Message); }
        }

        private async void btnDeleteStaffMember_Click(object sender, EventArgs e)
        {
            if (_selectedStaffId == 0)
            {
                MessageBox.Show("Please select a staff member to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE staff_member SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            staff_member_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedStaffId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Staff Member deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadStaffMembersAsync();
                            btnClearStaffMember_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting staff: " + ex.Message); }
            }
        }

        private void dgvStaffMember_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Valid Row
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvStaffMember.Rows[e.RowIndex];

                    // 2. Ignore New Row
                    if (row.IsNewRow) return;

                    // 3. Null ID Check
                    if (row.Cells["staff_member_id"].Value == null || row.Cells["staff_member_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedStaffId = Convert.ToInt32(row.Cells["staff_member_id"].Value);

                    // Map Texts
                    txtStaffMemberFirstName.Text = row.Cells["first_name"].Value?.ToString() ?? "";
                    txtStaffMemberLastName.Text = row.Cells["last_name"].Value?.ToString() ?? "";

                    // Map Combos
                    if (row.Cells["role_id"].Value != null && row.Cells["role_id"].Value != DBNull.Value)
                        cmbStaffMemberRole.SelectedValue = Convert.ToInt32(row.Cells["role_id"].Value);

                    if (row.Cells["country_id"].Value != null && row.Cells["country_id"].Value != DBNull.Value)
                        cmbStaffMemberCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                    // Map Date
                    if (row.Cells["date_of_birth"].Value != null && row.Cells["date_of_birth"].Value != DBNull.Value)
                        dtpStaffMemberBirth.Value = Convert.ToDateTime(row.Cells["date_of_birth"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting staff: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearStaffMember_Click(object sender, EventArgs e)
        {
            _selectedStaffId = 0;
            txtStaffMemberFirstName.Clear();
            txtStaffMemberLastName.Clear();
            dtpStaffMemberBirth.Value = DateTime.Now;

            if (cmbStaffMemberRole.Items.Count > 0) cmbStaffMemberRole.SelectedIndex = 0;
            if (cmbStaffMemberCountry.Items.Count > 0) cmbStaffMemberCountry.SelectedIndex = 0;

            dgvStaffMember.ClearSelection();
        }






        // --- ASYNC: Load Staff Roles ---
        private async Task LoadStaffRolesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
    SELECT 
        role_id, 
        role_name 
    FROM staff_role
    WHERE is_active = true
    ORDER BY role_name"; // Keep alphabetical sort

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvStaffRole.DataSource = dt;

                        FormatStaffRoleGrid(dgvStaffRole);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading staff roles: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FormatStaffRoleGrid(DataGridView dgv)
        {
            var headerMap = new Dictionary<string, string>
    {
        { "role_id", "ID" },
        { "role_name", "Role Name" }
    };

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (headerMap.ContainsKey(col.Name))
                {
                    col.HeaderText = headerMap[col.Name];
                }
            }

            // Specific formatting for the Main ID
            if (dgv.Columns.Contains("role_id"))
            {
                dgv.Columns["role_id"].Visible = true;
                dgv.Columns["role_id"].DisplayIndex = 0;
                dgv.Columns["role_id"].Width = 60;
            }

            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        }

        private async void btnAddStaffRole_Click(object sender, EventArgs e)
        {
            string roleName = txtStaffRoleName.Text.Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Role Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO staff_role (role_name, created_by) 
        VALUES (@name, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", roleName);
                    command.Parameters.AddWithValue("@creatorId", _currentUser);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Staff Role added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadStaffRolesAsync();
                        btnClearStaffRole_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Role Name already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateStaffRole_Click(object sender, EventArgs e)
        {
            if (_selectedStaffRoleId == 0)
            {
                MessageBox.Show("Please select a role to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string roleName = txtStaffRoleName.Text.Trim();

            if (string.IsNullOrEmpty(roleName))
            {
                MessageBox.Show("Role Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE staff_role SET 
            role_name = @name, 
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            role_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", roleName);
                    command.Parameters.AddWithValue("@updaterId", _currentUser);
                    command.Parameters.AddWithValue("@id", _selectedStaffRoleId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Staff Role updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadStaffRolesAsync();
                        btnClearStaffRole_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating role: " + ex.Message); }
        }

        private async void btnDeleteStaffRole_Click(object sender, EventArgs e)
        {
            if (_selectedStaffRoleId == 0)
            {
                MessageBox.Show("Please select a role to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE staff_role SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            role_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _currentUser);
                        command.Parameters.AddWithValue("@id", _selectedStaffRoleId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Staff Role deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadStaffRolesAsync();
                            btnClearStaffRole_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting role: " + ex.Message); }
            }
        }

        private void dgvStaffRole_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Check Valid Index
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvStaffRole.Rows[e.RowIndex];

                    // 2. Check New Row
                    if (row.IsNewRow) return;

                    // 3. Check Null ID
                    if (row.Cells["role_id"].Value == null || row.Cells["role_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedStaffRoleId = Convert.ToInt32(row.Cells["role_id"].Value);

                    // Map Text (Null Safe)
                    if (row.Cells["role_name"].Value != null && row.Cells["role_name"].Value != DBNull.Value)
                    {
                        txtStaffRoleName.Text = row.Cells["role_name"].Value.ToString();
                    }
                    else
                    {
                        txtStaffRoleName.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting role: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearStaffRole_Click(object sender, EventArgs e)
        {
            _selectedStaffRoleId = 0;
            txtStaffRoleName.Clear();
            dgvStaffRole.ClearSelection();
        }
    }
}
