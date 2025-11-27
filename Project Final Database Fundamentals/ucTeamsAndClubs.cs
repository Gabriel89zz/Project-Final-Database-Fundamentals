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
    public partial class ucTeamsAndClubs : UserControl
    {
        private readonly int _adminUserId;
        private int _selectedTeamId = 0;
        private int _selectedTeamKitId = 0;
        private int _selectedKitSponsorId = 0;
        private int _selectedTeamSponsorshipId = 0;
        private int _selectedTeamAwardId = 0;
        private int _selectedTeamSocialMediaId = 0;
        private int _selectedAcademyId=0;
        public ucTeamsAndClubs(int adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
        }


        // 1. Load Countries into ComboBox
        private async Task LoadCountriesForTeamComboAsync()
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

                        cmbTeamCountry.DataSource = dt;
                        cmbTeamCountry.DisplayMember = "name";
                        cmbTeamCountry.ValueMember = "country_id";
                        cmbTeamCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        // 2. Load Stadiums into ComboBox
        private async Task LoadStadiumsForTeamComboAsync()
        {
            string query = @"SELECT stadium_id, name FROM stadium WHERE is_active = true ORDER BY name";
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
                        row["stadium_id"] = -1;
                        row["name"] = "-- Select Stadium --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTeamStadium.DataSource = dt;
                        cmbTeamStadium.DisplayMember = "name";
                        cmbTeamStadium.ValueMember = "stadium_id";
                        cmbTeamStadium.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stadiums: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadTeamsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            t.team_id, 
            t.name, 
            t.short_name,
            t.foundation_date,
            t.country_id,
            c.name AS country_name,
            t.home_stadium_id,
            s.name AS stadium_name
        FROM team t
        INNER JOIN ""country"" c ON t.country_id = c.country_id
        INNER JOIN stadium s ON t.home_stadium_id = s.stadium_id
        WHERE t.is_active = true
        ORDER BY t.team_id DESC";

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

                        dgvTeam.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "team_id", "country_id", "home_stadium_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTeam.Columns[col] != null)
                                dgvTeam.Columns[col].Visible = false;
                        }

                        // Format Date Column
                        if (dgvTeam.Columns["foundation_date"] != null)
                            dgvTeam.Columns["foundation_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddTeam_Click(object sender, EventArgs e)
        {
            string name = txtNameTeam.Text.Trim();
            string shortName = txtShorNameTeam.Text.Trim();
            DateTime foundation = dtpTeamFoundationYear.Value;

            // 1. Validate Combos
            if ((int)cmbTeamCountry.SelectedValue == -1 || (int)cmbTeamStadium.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Country and a Stadium.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int countryId = (int)cmbTeamCountry.SelectedValue;
            int stadiumId = (int)cmbTeamStadium.SelectedValue;

            // 2. Validate Text
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(shortName))
            {
                MessageBox.Show("Name and Short Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO team (name, short_name, country_id, home_stadium_id, foundation_date, created_by) 
        VALUES (@name, @short, @countryId, @stadiumId, @foundation, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@short", shortName);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@stadiumId", stadiumId);
                    command.Parameters.AddWithValue("@foundation", foundation); // Storing Date
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Team added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamsAsync();
                        btnClearTeam_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTeam_Click(object sender, EventArgs e)
        {
            if (_selectedTeamId == 0)
            {
                MessageBox.Show("Please select a team to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameTeam.Text.Trim();
            string shortName = txtShorNameTeam.Text.Trim();
            DateTime foundation = dtpTeamFoundationYear.Value;

            if ((int)cmbTeamCountry.SelectedValue == -1 ||
                (int)cmbTeamStadium.SelectedValue == -1 ||
                string.IsNullOrEmpty(name))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int countryId = (int)cmbTeamCountry.SelectedValue;
            int stadiumId = (int)cmbTeamStadium.SelectedValue;

            string query = @"
        UPDATE team SET 
            name = @name, 
            short_name = @short,
            country_id = @countryId,
            home_stadium_id = @stadiumId,
            foundation_date = @foundation,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            team_id = @teamId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@short", shortName);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@stadiumId", stadiumId);
                    command.Parameters.AddWithValue("@foundation", foundation);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@teamId", _selectedTeamId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Team updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamsAsync();
                        btnClearTeam_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating team: " + ex.Message); }
        }

        private async void btnDeleteTeam_Click(object sender, EventArgs e)
        {
            if (_selectedTeamId == 0)
            {
                MessageBox.Show("Please select a team to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE team SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            team_id = @teamId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@teamId", _selectedTeamId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Team deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTeamsAsync();
                            btnClearTeam_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting team: " + ex.Message); }
            }
        }

        private void btnClearTeam_Click(object sender, EventArgs e)
        {
            _selectedTeamId = 0;
            txtNameTeam.Clear();
            txtShorNameTeam.Clear();
            dtpTeamFoundationYear.Value = DateTime.Now;

            if (cmbTeamCountry.Items.Count > 0) cmbTeamCountry.SelectedIndex = 0;
            if (cmbTeamStadium.Items.Count > 0) cmbTeamStadium.SelectedIndex = 0;

            dgvTeam.ClearSelection();
        }

        private void dgvTeam_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvTeam.Rows[e.RowIndex];

                _selectedTeamId = Convert.ToInt32(row.Cells["team_id"].Value);

                txtNameTeam.Text = row.Cells["name"].Value.ToString();
                txtShorNameTeam.Text = row.Cells["short_name"].Value.ToString();

                // Combos
                if (row.Cells["country_id"].Value != DBNull.Value)
                    cmbTeamCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                if (row.Cells["home_stadium_id"].Value != DBNull.Value)
                    cmbTeamStadium.SelectedValue = Convert.ToInt32(row.Cells["home_stadium_id"].Value);

                // DateTimePicker
                if (row.Cells["foundation_date"].Value != DBNull.Value)
                {
                    dtpTeamFoundationYear.Value = Convert.ToDateTime(row.Cells["foundation_date"].Value);
                }
            }
        }

        private async void tabContolTeamsAndClubs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabContolTeamsAndClubs.SelectedTab == tabTeam)
            {
                // Load Grid + 2 Combos in parallel
                var tGrid = LoadTeamsAsync();
                var tCountry = LoadCountriesForTeamComboAsync();
                var tStadium = LoadStadiumsForTeamComboAsync();

                await Task.WhenAll(tGrid, tCountry, tStadium);
            }
            else if (tabContolTeamsAndClubs.SelectedTab == tabTeamKits)
            {
                // 1. Init Fixed Combo (Sync)
                InitializeKitTypeCombo();

                // 2. Load Grid + 2 DB Combos in Parallel
                var tGrid = LoadTeamKitsAsync();
                var tTeam = LoadTeamsForKitComboAsync();

                await Task.WhenAll(tGrid, tTeam);
            }
            else if (tabContolTeamsAndClubs.SelectedTab == tabKitSponsor)
            {
                // 1. Init Fixed Placement Combo (Sync)
                InitializePlacementCombo();

                // 2. Load Grid + 2 DB Combos in Parallel
                var tGrid = LoadKitSponsorsAsync();
                var tKit = LoadKitsForKitSponsorComboAsync();
                var tSponsor = LoadSponsorsForKitSponsorComboAsync();

                await Task.WhenAll(tGrid, tKit, tSponsor);
            }
            else if (tabContolTeamsAndClubs.SelectedTab == tabTeamSponsorship)
            {
                // Parallel Loading (Grid + 3 Combos)
                var tGrid = LoadTeamSponsorshipsAsync();
                var tTeam = LoadTeamsForSponsorshipComboAsync();
                var tSponsor = LoadSponsorsForSponsorshipComboAsync();
                var tType = LoadTypesForSponsorshipComboAsync();

                await Task.WhenAll(tGrid, tTeam, tSponsor, tType);
            }
            else if (tabContolTeamsAndClubs.SelectedTab == tabTeamAward)
            {
                // Parallel Loading
                var tGrid = LoadTeamAwardsAsync();
                var tAward = LoadAwardsForTeamAwardComboAsync();
                var tTeam = LoadTeamsForTeamAwardComboAsync();
                var tSeason = LoadSeasonsForTeamAwardComboAsync();

                await Task.WhenAll(tGrid, tAward, tTeam, tSeason);
            }
            else if (tabContolTeamsAndClubs.SelectedTab == tabTeamSocialMedia)
            {
                // Parallel Loading
                var tGrid = LoadTeamSocialMediaAsync();
                var tTeam = LoadTeamsForSocialMediaComboAsync();
                var tPlatform = LoadPlatformsForSocialMediaComboAsync();

                await Task.WhenAll(tGrid, tTeam, tPlatform);
            }
            else if (tabContolTeamsAndClubs.SelectedTab == tabAcademy)
            {
                // Parallel Loading (Grid + 2 Combos)
                var tGrid = LoadAcademiesAsync();
                var tTeam = LoadTeamsForAcademyComboAsync();
                var tCity = LoadCitiesForAcademyComboAsync();

                await Task.WhenAll(tGrid, tTeam, tCity);
            }
        }






        // 1. Initialize Kit Type ComboBox (Fixed Options)
        private void InitializeKitTypeCombo()
        {
            cmbTeamKitType.Items.Clear();
            // Using standard English terms, or use "Local", "Visitante" if preferred
            cmbTeamKitType.Items.Add("Home");
            cmbTeamKitType.Items.Add("Away");
            cmbTeamKitType.Items.Add("Third");

            cmbTeamKitType.SelectedIndex = 0;
        }

        // 2. Load Teams into ComboBox
        private async Task LoadTeamsForKitComboAsync()
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

                        cmbTeamKitTeam.DataSource = dt;
                        cmbTeamKitTeam.DisplayMember = "name";
                        cmbTeamKitTeam.ValueMember = "team_id";
                        cmbTeamKitTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Main Grid
        private async Task LoadTeamKitsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            tk.kit_id, 
            tk.team_id,
            t.name AS team_name,
            tk.kit_type
        FROM team_kit tk
        INNER JOIN team t ON tk.team_id = t.team_id
        WHERE tk.is_active = true
        ORDER BY t.name, tk.kit_type";

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

                        dgvTeamKit.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "kit_id", "team_id", "manufacturer_sponsor_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTeamKit.Columns[col] != null)
                                dgvTeamKit.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading kits: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddTeamKit_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbTeamKitTeam.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbTeamKitType.SelectedItem == null)
            {
                MessageBox.Show("Please select a Kit Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbTeamKitTeam.SelectedValue;
            string kitType = cmbTeamKitType.SelectedItem.ToString();

            string query = @"
        INSERT INTO team_kit (team_id, kit_type, created_by) 
        VALUES (@teamId, @kitType, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@kitType", kitType);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Team Kit added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamKitsAsync();
                        btnClearTeamKit_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Kit Type already exists for this Team.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTeamKit_Click(object sender, EventArgs e)
        {
            if (_selectedTeamKitId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbTeamKitTeam.SelectedValue == -1 ||
                cmbTeamKitType.SelectedItem == null)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbTeamKitTeam.SelectedValue;
            string kitType = cmbTeamKitType.SelectedItem.ToString();

            string query = @"
        UPDATE team_kit SET 
            team_id = @teamId, 
            kit_type = @kitType,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            kit_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@kitType", kitType);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedTeamKitId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Team Kit updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamKitsAsync();
                        btnClearTeamKit_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating kit: " + ex.Message); }
        }

        private async void btnDeleteTeamKit_Click(object sender, EventArgs e)
        {
            if (_selectedTeamKitId == 0)
            {
                MessageBox.Show("Please select a kit to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE team_kit SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            kit_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedTeamKitId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Team Kit deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTeamKitsAsync();
                            btnClearTeamKit_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting kit: " + ex.Message); }
            }
        }

        private void dgvTeamKit_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvTeamKit.Rows[e.RowIndex];

                _selectedTeamKitId = Convert.ToInt32(row.Cells["kit_id"].Value);

                // Map Team
                if (row.Cells["team_id"].Value != DBNull.Value)
                    cmbTeamKitTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                // Map Kit Type (String)
                if (row.Cells["kit_type"].Value != DBNull.Value)
                    cmbTeamKitType.SelectedItem = row.Cells["kit_type"].Value.ToString();
            }
        }

        private void btnClearTeamKit_Click(object sender, EventArgs e)
        {
            _selectedTeamKitId = 0;

            // Reset Combos
            if (cmbTeamKitTeam.Items.Count > 0) cmbTeamKitTeam.SelectedIndex = 0;
            if (cmbTeamKitType.Items.Count > 0) cmbTeamKitType.SelectedIndex = 0;

            dgvTeamKit.ClearSelection();
        }








        // 1. Initialize Placement ComboBox (Fixed Options)
        private void InitializePlacementCombo()
        {
            cmbKitSponsorPlacement.Items.Clear();

            // Common jersey placements
            cmbKitSponsorPlacement.Items.Add("Front Chest");
            cmbKitSponsorPlacement.Items.Add("Back Top");
            cmbKitSponsorPlacement.Items.Add("Back Bottom");
            cmbKitSponsorPlacement.Items.Add("Left Sleeve");
            cmbKitSponsorPlacement.Items.Add("Right Sleeve");
            cmbKitSponsorPlacement.Items.Add("Shorts");

            cmbKitSponsorPlacement.SelectedIndex = 0;
        }

        // 2. Load Team Kits (With friendly name: "Real Madrid - Home")
        private async Task LoadKitsForKitSponsorComboAsync()
        {
            string query = @"
        SELECT 
            tk.kit_id, 
            t.name || ' - ' || tk.kit_type AS display_name
        FROM team_kit tk
        INNER JOIN team t ON tk.team_id = t.team_id
        WHERE tk.is_active = true
        ORDER BY t.name";

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
                        row["kit_id"] = -1;
                        row["display_name"] = "-- Select Team Kit --";
                        dt.Rows.InsertAt(row, 0);

                        cmbKitSponsorKit.DataSource = dt;
                        cmbKitSponsorKit.DisplayMember = "display_name";
                        cmbKitSponsorKit.ValueMember = "kit_id";
                        cmbKitSponsorKit.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading kits: " + ex.Message); }
        }

        // 3. Load Sponsors
        private async Task LoadSponsorsForKitSponsorComboAsync()
        {
            string query = @"SELECT sponsor_id, name FROM sponsor WHERE is_active = true ORDER BY name";

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
                        row["sponsor_id"] = -1;
                        row["name"] = "-- Select Sponsor --";
                        dt.Rows.InsertAt(row, 0);

                        cmbKitSponsorSponsor.DataSource = dt;
                        cmbKitSponsorSponsor.DisplayMember = "name";
                        cmbKitSponsorSponsor.ValueMember = "sponsor_id";
                        cmbKitSponsorSponsor.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading sponsors: " + ex.Message); }
        }

        // 4. Load Main Grid
        private async Task LoadKitSponsorsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            ks.id, 
            ks.kit_id,
            (t.name || ' - ' || tk.kit_type) AS kit_display_name,
            ks.sponsor_id,
            s.name AS sponsor_name,
            ks.placement,
            ks.is_primary
        FROM kit_sponsor ks
        INNER JOIN team_kit tk ON ks.kit_id = tk.kit_id
        INNER JOIN team t ON tk.team_id = t.team_id
        INNER JOIN sponsor s ON ks.sponsor_id = s.sponsor_id
        WHERE ks.is_active = true
        ORDER BY t.name, ks.placement";

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

                        dgvKitSponsor.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "id", "kit_id", "sponsor_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvKitSponsor.Columns[col] != null)
                                dgvKitSponsor.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading kit sponsors: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddKitSponsor_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbKitSponsorKit.SelectedValue == -1 ||
                (int)cmbKitSponsorSponsor.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Kit and a Sponsor.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbKitSponsorPlacement.SelectedItem == null)
            {
                MessageBox.Show("Please select a Placement.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int kitId = (int)cmbKitSponsorKit.SelectedValue;
            int sponsorId = (int)cmbKitSponsorSponsor.SelectedValue;
            string placement = cmbKitSponsorPlacement.SelectedItem.ToString();
            bool isPrimary = chkbIsPrimary.Checked;

            string query = @"
        INSERT INTO kit_sponsor (kit_id, sponsor_id, placement, is_primary, created_by) 
        VALUES (@kitId, @sponsorId, @placement, @isPrimary, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@kitId", kitId);
                    command.Parameters.AddWithValue("@sponsorId", sponsorId);
                    command.Parameters.AddWithValue("@placement", placement);
                    command.Parameters.AddWithValue("@isPrimary", isPrimary);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Kit Sponsor added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadKitSponsorsAsync();
                        btnClearKitSponsor_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                // Example unique constraint: Same kit, same placement
                if (ex.SqlState == "23505")
                    MessageBox.Show("This placement is already taken for this kit.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateKitSponsor_Click(object sender, EventArgs e)
        {
            if (_selectedKitSponsorId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbKitSponsorKit.SelectedValue == -1 ||
                (int)cmbKitSponsorSponsor.SelectedValue == -1 ||
                cmbKitSponsorPlacement.SelectedItem == null)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int kitId = (int)cmbKitSponsorKit.SelectedValue;
            int sponsorId = (int)cmbKitSponsorSponsor.SelectedValue;
            string placement = cmbKitSponsorPlacement.SelectedItem.ToString();
            bool isPrimary = chkbIsPrimary.Checked;

            string query = @"
        UPDATE kit_sponsor SET 
            kit_id = @kitId, 
            sponsor_id = @sponsorId,
            placement = @placement,
            is_primary = @isPrimary,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@kitId", kitId);
                    command.Parameters.AddWithValue("@sponsorId", sponsorId);
                    command.Parameters.AddWithValue("@placement", placement);
                    command.Parameters.AddWithValue("@isPrimary", isPrimary);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedKitSponsorId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadKitSponsorsAsync();
                        btnClearKitSponsor_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteKitSponsor_Click(object sender, EventArgs e)
        {
            if (_selectedKitSponsorId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE kit_sponsor SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedKitSponsorId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadKitSponsorsAsync();
                            btnClearKitSponsor_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvKitSponsor_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvKitSponsor.Rows[e.RowIndex];

                _selectedKitSponsorId = Convert.ToInt32(row.Cells["id"].Value);

                // Map Combos
                if (row.Cells["kit_id"].Value != DBNull.Value)
                    cmbKitSponsorKit.SelectedValue = Convert.ToInt32(row.Cells["kit_id"].Value);

                if (row.Cells["sponsor_id"].Value != DBNull.Value)
                    cmbKitSponsorSponsor.SelectedValue = Convert.ToInt32(row.Cells["sponsor_id"].Value);

                // Map Placement (String)
                if (row.Cells["placement"].Value != DBNull.Value)
                    cmbKitSponsorPlacement.SelectedItem = row.Cells["placement"].Value.ToString();

                // Map Checkbox
                if (row.Cells["is_primary"].Value != DBNull.Value)
                    chkbIsPrimary.Checked = Convert.ToBoolean(row.Cells["is_primary"].Value);
                else
                    chkbIsPrimary.Checked = false;
            }
        }

        private void btnClearKitSponsor_Click(object sender, EventArgs e)
        {
            _selectedKitSponsorId = 0;

            // Reset Controls
            if (cmbKitSponsorKit.Items.Count > 0) cmbKitSponsorKit.SelectedIndex = 0;
            if (cmbKitSponsorSponsor.Items.Count > 0) cmbKitSponsorSponsor.SelectedIndex = 0;
            if (cmbKitSponsorPlacement.Items.Count > 0) cmbKitSponsorPlacement.SelectedIndex = 0;

            chkbIsPrimary.Checked = false;

            dgvKitSponsor.ClearSelection();
        }





        // 1. Load Teams
        private async Task LoadTeamsForSponsorshipComboAsync()
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

                        cmbTeamSponsorshipTeam.DataSource = dt;
                        cmbTeamSponsorshipTeam.DisplayMember = "name";
                        cmbTeamSponsorshipTeam.ValueMember = "team_id";
                        cmbTeamSponsorshipTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 2. Load Sponsors
        private async Task LoadSponsorsForSponsorshipComboAsync()
        {
            string query = @"SELECT sponsor_id, name FROM sponsor WHERE is_active = true ORDER BY name";
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
                        row["sponsor_id"] = -1;
                        row["name"] = "-- Select Sponsor --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTeamSponsorshipSponsor.DataSource = dt;
                        cmbTeamSponsorshipSponsor.DisplayMember = "name";
                        cmbTeamSponsorshipSponsor.ValueMember = "sponsor_id";
                        cmbTeamSponsorshipSponsor.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading sponsors: " + ex.Message); }
        }

        // 3. Load Sponsorship Types
        private async Task LoadTypesForSponsorshipComboAsync()
        {
            // Assuming table 'sponsorship_type' with column 'type_name'
            string query = @"SELECT sponsorship_type_id, type_name FROM sponsorship_type WHERE is_active = true ORDER BY type_name";
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
                        row["sponsorship_type_id"] = -1;
                        row["type_name"] = "-- Select Type --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTeamSponsorshipType.DataSource = dt;
                        cmbTeamSponsorshipType.DisplayMember = "type_name";
                        cmbTeamSponsorshipType.ValueMember = "sponsorship_type_id";
                        cmbTeamSponsorshipType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading types: " + ex.Message); }
        }

        // 4. Load Main Grid
        private async Task LoadTeamSponsorshipsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            ts.team_sponsorship_id, 
            ts.team_id,
            t.name AS team_name,
            ts.sponsor_id,
            s.name AS sponsor_name,
            ts.sponsorship_type_id,
            st.type_name AS type_name,
            ts.deal_value_eur
        FROM team_sponsorship ts
        INNER JOIN team t ON ts.team_id = t.team_id
        INNER JOIN sponsor s ON ts.sponsor_id = s.sponsor_id
        INNER JOIN sponsorship_type st ON ts.sponsorship_type_id = st.sponsorship_type_id
        WHERE ts.is_active = true
        ORDER BY t.name, ts.deal_value_eur DESC";

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

                        dgvTeamSponsorship.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "team_sponsorship_id", "team_id", "sponsor_id", "sponsorship_type_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTeamSponsorship.Columns[col] != null)
                                dgvTeamSponsorship.Columns[col].Visible = false;
                        }

                        // Format Currency Column
                        if (dgvTeamSponsorship.Columns["deal_value_eur"] != null)
                            dgvTeamSponsorship.Columns["deal_value_eur"].DefaultCellStyle.Format = "C2"; // Currency format
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddTeamSponsorship_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbTeamSponsorshipTeam.SelectedValue == -1 ||
                (int)cmbTeamSponsorshipSponsor.SelectedValue == -1 ||
                (int)cmbTeamSponsorshipType.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Team, Sponsor, and Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbTeamSponsorshipTeam.SelectedValue;
            int sponsorId = (int)cmbTeamSponsorshipSponsor.SelectedValue;
            int typeId = (int)cmbTeamSponsorshipType.SelectedValue;
            decimal dealValue = nudDealValue.Value;

            string query = @"
        INSERT INTO team_sponsorship (team_id, sponsor_id, sponsorship_type_id, deal_value_eur, created_by) 
        VALUES (@teamId, @sponsorId, @typeId, @value, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@sponsorId", sponsorId);
                    command.Parameters.AddWithValue("@typeId", typeId);
                    command.Parameters.AddWithValue("@value", dealValue);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Sponsorship added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamSponsorshipsAsync();
                        btnClearTeamSponsorship_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Sponsorship record already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTeamSponsorship_Click(object sender, EventArgs e)
        {
            if (_selectedTeamSponsorshipId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbTeamSponsorshipTeam.SelectedValue == -1 ||
                (int)cmbTeamSponsorshipSponsor.SelectedValue == -1 ||
                (int)cmbTeamSponsorshipType.SelectedValue == -1)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbTeamSponsorshipTeam.SelectedValue;
            int sponsorId = (int)cmbTeamSponsorshipSponsor.SelectedValue;
            int typeId = (int)cmbTeamSponsorshipType.SelectedValue;
            decimal dealValue = nudDealValue.Value;

            string query = @"
        UPDATE team_sponsorship SET 
            team_id = @teamId, 
            sponsor_id = @sponsorId,
            sponsorship_type_id = @typeId,
            deal_value_eur = @value,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            team_sponsorship_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@sponsorId", sponsorId);
                    command.Parameters.AddWithValue("@typeId", typeId);
                    command.Parameters.AddWithValue("@value", dealValue);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedTeamSponsorshipId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamSponsorshipsAsync();
                        btnClearTeamSponsorship_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteTeamSponsorship_Click(object sender, EventArgs e)
        {
            if (_selectedTeamSponsorshipId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE team_sponsorship SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            team_sponsorship_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedTeamSponsorshipId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTeamSponsorshipsAsync();
                            btnClearTeamSponsorship_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void btnClearTeamSponsorship_Click(object sender, EventArgs e)
        {
            _selectedTeamSponsorshipId = 0;

            // Reset Combos
            if (cmbTeamSponsorshipTeam.Items.Count > 0) cmbTeamSponsorshipTeam.SelectedIndex = 0;
            if (cmbTeamSponsorshipSponsor.Items.Count > 0) cmbTeamSponsorshipSponsor.SelectedIndex = 0;
            if (cmbTeamSponsorshipType.Items.Count > 0) cmbTeamSponsorshipType.SelectedIndex = 0;

            nudDealValue.Value = 0;

            dgvTeamSponsorship.ClearSelection();
        }

        private void dgvTeamSponsorship_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Validar que el índice sea válido (evita error al clicar encabezados)
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvTeamSponsorship.Rows[e.RowIndex];

                    // 2. IMPORTANTE: Evitar error al clicar la fila de "Nuevo Registro" (la fila vacía del final)
                    if (row.IsNewRow) return;

                    // 3. Validar que la celda del ID tenga valor antes de convertir
                    if (row.Cells["team_sponsorship_id"].Value == null ||
                        row.Cells["team_sponsorship_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedTeamSponsorshipId = Convert.ToInt32(row.Cells["team_sponsorship_id"].Value);

                    // --- Map Combos (Con validación extra de nulidad) ---

                    // Team
                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                    {
                        cmbTeamSponsorshipTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);
                    }

                    // Sponsor
                    if (row.Cells["sponsor_id"].Value != null && row.Cells["sponsor_id"].Value != DBNull.Value)
                    {
                        cmbTeamSponsorshipSponsor.SelectedValue = Convert.ToInt32(row.Cells["sponsor_id"].Value);
                    }

                    // Type
                    if (row.Cells["sponsorship_type_id"].Value != null && row.Cells["sponsorship_type_id"].Value != DBNull.Value)
                    {
                        cmbTeamSponsorshipType.SelectedValue = Convert.ToInt32(row.Cells["sponsorship_type_id"].Value);
                    }

                    // --- Map Numeric ---
                    if (row.Cells["deal_value_eur"].Value != null && row.Cells["deal_value_eur"].Value != DBNull.Value)
                    {
                        // Usamos try-parse o convert seguro para evitar errores de formato de moneda
                        string valStr = row.Cells["deal_value_eur"].Value.ToString();

                        // Limpiamos caracteres de moneda si el grid ya los tiene (ej: "$1,000.00" -> "1000.00")
                        // Esto es opcional, pero Convert.ToDecimal suele manejarlo bien si es tipo numérico en la BD
                        nudDealValue.Value = Convert.ToDecimal(row.Cells["deal_value_eur"].Value);
                    }
                    else
                    {
                        nudDealValue.Value = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Esto evita que el programa se cierre y te avisa qué pasó
                MessageBox.Show("Error al seleccionar el registro: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }




        // 1. Load Awards
        private async Task LoadAwardsForTeamAwardComboAsync()
        {
            string query = @"SELECT award_id, name FROM award WHERE is_active = true ORDER BY name";
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
                        row["award_id"] = -1;
                        row["name"] = "-- Select Award --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTeamAwardAward.DataSource = dt;
                        cmbTeamAwardAward.DisplayMember = "name";
                        cmbTeamAwardAward.ValueMember = "award_id";
                        cmbTeamAwardAward.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading awards: " + ex.Message); }
        }

        // 2. Load Teams
        private async Task LoadTeamsForTeamAwardComboAsync()
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

                        cmbTeamAwardTeam.DataSource = dt;
                        cmbTeamAwardTeam.DisplayMember = "name";
                        cmbTeamAwardTeam.ValueMember = "team_id";
                        cmbTeamAwardTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Seasons
        private async Task LoadSeasonsForTeamAwardComboAsync()
        {
            string query = @"SELECT season_id, name FROM season WHERE is_active = true ORDER BY name DESC";
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
                        row["season_id"] = -1;
                        row["name"] = "-- Select Season --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTeamAwardSeason.DataSource = dt;
                        cmbTeamAwardSeason.DisplayMember = "name";
                        cmbTeamAwardSeason.ValueMember = "season_id";
                        cmbTeamAwardSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 4. Load Main Grid
        private async Task LoadTeamAwardsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            ta.team_award_id, 
            ta.award_id,
            a.name AS award_name,
            ta.team_id,
            t.name AS team_name,
            ta.season_id,
            s.name AS season_name
        FROM team_award ta
        INNER JOIN award a ON ta.award_id = a.award_id
        INNER JOIN team t ON ta.team_id = t.team_id
        INNER JOIN season s ON ta.season_id = s.season_id
        WHERE ta.is_active = true
        ORDER BY s.name DESC, t.name";

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

                        dgvTeamAward.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "team_award_id", "award_id", "team_id", "season_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTeamAward.Columns[col] != null)
                                dgvTeamAward.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading team awards: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddTeamAward_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbTeamAwardAward.SelectedValue == -1 ||
                (int)cmbTeamAwardTeam.SelectedValue == -1 ||
                (int)cmbTeamAwardSeason.SelectedValue == -1)
            {
                MessageBox.Show("All fields are required (Award, Team, Season).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int awardId = (int)cmbTeamAwardAward.SelectedValue;
            int teamId = (int)cmbTeamAwardTeam.SelectedValue;
            int seasonId = (int)cmbTeamAwardSeason.SelectedValue;

            string query = @"
        INSERT INTO team_award (award_id, team_id, season_id, created_by) 
        VALUES (@awardId, @teamId, @seasonId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@awardId", awardId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@seasonId", seasonId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Team Award added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamAwardsAsync();
                        btnClearTeamAward_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team already has this Award for this Season.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTeamAward_Click(object sender, EventArgs e)
        {
            if (_selectedTeamAwardId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbTeamAwardAward.SelectedValue == -1 ||
                (int)cmbTeamAwardTeam.SelectedValue == -1 ||
                (int)cmbTeamAwardSeason.SelectedValue == -1)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int awardId = (int)cmbTeamAwardAward.SelectedValue;
            int teamId = (int)cmbTeamAwardTeam.SelectedValue;
            int seasonId = (int)cmbTeamAwardSeason.SelectedValue;

            string query = @"
        UPDATE team_award SET 
            award_id = @awardId, 
            team_id = @teamId,
            season_id = @seasonId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            team_award_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@awardId", awardId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@seasonId", seasonId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedTeamAwardId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamAwardsAsync();
                        btnClearTeamAward_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteTeamAward_Click(object sender, EventArgs e)
        {
            if (_selectedTeamAwardId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE team_award SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            team_award_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedTeamAwardId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTeamAwardsAsync();
                            btnClearTeamAward_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvTeamAward_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Validate valid row index
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvTeamAward.Rows[e.RowIndex];

                    // 2. Validate it is not the "New Row" placeholder
                    if (row.IsNewRow) return;

                    // 3. Validate ID cell is not null
                    if (row.Cells["team_award_id"].Value == null || row.Cells["team_award_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedTeamAwardId = Convert.ToInt32(row.Cells["team_award_id"].Value);

                    // Map Combos (With null checks)
                    if (row.Cells["award_id"].Value != null && row.Cells["award_id"].Value != DBNull.Value)
                    {
                        cmbTeamAwardAward.SelectedValue = Convert.ToInt32(row.Cells["award_id"].Value);
                    }

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                    {
                        cmbTeamAwardTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);
                    }

                    if (row.Cells["season_id"].Value != null && row.Cells["season_id"].Value != DBNull.Value)
                    {
                        cmbTeamAwardSeason.SelectedValue = Convert.ToInt32(row.Cells["season_id"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearTeamAward_Click(object sender, EventArgs e)
        {
            _selectedTeamAwardId = 0;

            if (cmbTeamAwardAward.Items.Count > 0) cmbTeamAwardAward.SelectedIndex = 0;
            if (cmbTeamAwardTeam.Items.Count > 0) cmbTeamAwardTeam.SelectedIndex = 0;
            if (cmbTeamAwardSeason.Items.Count > 0) cmbTeamAwardSeason.SelectedIndex = 0;

            dgvTeamAward.ClearSelection();
        }





        // 1. Load Teams
        private async Task LoadTeamsForSocialMediaComboAsync()
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

                        cmbTeamSocialMediaTeam.DataSource = dt;
                        cmbTeamSocialMediaTeam.DisplayMember = "name";
                        cmbTeamSocialMediaTeam.ValueMember = "team_id";
                        cmbTeamSocialMediaTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 2. Load Social Media Platforms
        private async Task LoadPlatformsForSocialMediaComboAsync()
        {
            // Assuming the table created earlier is 'social_media_platform'
            string query = @"SELECT social_media_platform_id, name FROM social_media_platform WHERE is_active = true ORDER BY name";
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
                        row["social_media_platform_id"] = -1;
                        row["name"] = "-- Select Platform --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTeamSocialMediaPlatform.DataSource = dt;
                        cmbTeamSocialMediaPlatform.DisplayMember = "name";
                        cmbTeamSocialMediaPlatform.ValueMember = "social_media_platform_id";
                        cmbTeamSocialMediaPlatform.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading platforms: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadTeamSocialMediaAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            tsm.team_social_media_id, 
            tsm.team_id,
            t.name AS team_name,
            tsm.platform_id,
            p.name AS platform_name,
            tsm.handle
        FROM team_social_media tsm
        INNER JOIN team t ON tsm.team_id = t.team_id
        INNER JOIN social_media_platform p ON tsm.platform_id = p.social_media_platform_id
        WHERE tsm.is_active = true
        ORDER BY t.name, p.name";

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

                        dgvTeamSocialMedia.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "team_social_media_id", "team_id", "social_media_platform_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTeamSocialMedia.Columns[col] != null)
                                dgvTeamSocialMedia.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading team social media: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddTeamSocialMedia_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbTeamSocialMediaTeam.SelectedValue == -1 ||
                (int)cmbTeamSocialMediaPlatform.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Team and a Platform.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbTeamSocialMediaTeam.SelectedValue;
            int platformId = (int)cmbTeamSocialMediaPlatform.SelectedValue;
            string handle = txtHandle.Text.Trim();

            // 2. Validate Handle
            if (string.IsNullOrEmpty(handle))
            {
                MessageBox.Show("Handle is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO team_social_media (team_id, platform_id, handle, created_by) 
        VALUES (@teamId, @platformId, @handle, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@platformId", platformId);
                    command.Parameters.AddWithValue("@handle", handle);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Social Media added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamSocialMediaAsync();
                        btnClearTeamSocialMedia_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team already has this Platform linked.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTeamSocialMedia_Click(object sender, EventArgs e)
        {
            if (_selectedTeamSocialMediaId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbTeamSocialMediaTeam.SelectedValue == -1 ||
                (int)cmbTeamSocialMediaPlatform.SelectedValue == -1)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbTeamSocialMediaTeam.SelectedValue;
            int platformId = (int)cmbTeamSocialMediaPlatform.SelectedValue;
            string handle = txtHandle.Text.Trim();

            if (string.IsNullOrEmpty(handle))
            {
                MessageBox.Show("Handle is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE team_social_media SET 
            team_id = @teamId, 
            platform_id = @platformId,
            handle = @handle,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            team_social_media_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@platformId", platformId);
                    command.Parameters.AddWithValue("@handle", handle);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedTeamSocialMediaId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamSocialMediaAsync();
                        btnClearTeamSocialMedia_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteTeamSocialMedia_Click(object sender, EventArgs e)
        {
            if (_selectedTeamSocialMediaId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE team_social_media SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            team_social_media_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedTeamSocialMediaId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTeamSocialMediaAsync();
                            btnClearTeamSocialMedia_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvTeamSocialMedia_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Check valid Row Index
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvTeamSocialMedia.Rows[e.RowIndex];

                    // 2. Check if it's the "New Row" placeholder
                    if (row.IsNewRow) return;

                    // 3. Check if ID cell is null
                    if (row.Cells["team_social_media_id"].Value == null ||
                        row.Cells["team_social_media_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedTeamSocialMediaId = Convert.ToInt32(row.Cells["team_social_media_id"].Value);

                    // Map Team Combo (Null Check)
                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                    {
                        cmbTeamSocialMediaTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);
                    }

                    // Map Platform Combo (Null Check)
                    if (row.Cells["social_media_platform_id"].Value != null && row.Cells["social_media_platform_id"].Value != DBNull.Value)
                    {
                        cmbTeamSocialMediaPlatform.SelectedValue = Convert.ToInt32(row.Cells["social_media_platform_id"].Value);
                    }

                    // Map Handle (Null Check)
                    if (row.Cells["handle"].Value != null && row.Cells["handle"].Value != DBNull.Value)
                    {
                        txtHandle.Text = row.Cells["handle"].Value.ToString();
                    }
                    else
                    {
                        txtHandle.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearTeamSocialMedia_Click(object sender, EventArgs e)
        {
            _selectedTeamSocialMediaId = 0;
            txtHandle.Clear();

            if (cmbTeamSocialMediaTeam.Items.Count > 0) cmbTeamSocialMediaTeam.SelectedIndex = 0;
            if (cmbTeamSocialMediaPlatform.Items.Count > 0) cmbTeamSocialMediaPlatform.SelectedIndex = 0;

            dgvTeamSocialMedia.ClearSelection();
        }






        // 1. Load Teams into ComboBox
        private async Task LoadTeamsForAcademyComboAsync()
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

                        cmbAcademyTeam.DataSource = dt;
                        cmbAcademyTeam.DisplayMember = "name";
                        cmbAcademyTeam.ValueMember = "team_id";
                        cmbAcademyTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 2. Load Cities into ComboBox
        private async Task LoadCitiesForAcademyComboAsync()
        {
            string query = @"SELECT city_id, name FROM city WHERE is_active = true ORDER BY name";
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
                        row["city_id"] = -1;
                        row["name"] = "-- Select City --";
                        dt.Rows.InsertAt(row, 0);

                        cmbAcademyCity.DataSource = dt;
                        cmbAcademyCity.DisplayMember = "name";
                        cmbAcademyCity.ValueMember = "city_id";
                        cmbAcademyCity.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading cities: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadAcademiesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            a.academy_id, 
            a.name AS academy_name,
            a.team_id,
            t.name AS team_name,
            a.city_id,
            c.name AS city_name
        FROM academy a
        INNER JOIN team t ON a.team_id = t.team_id
        INNER JOIN city c ON a.city_id = c.city_id
        WHERE a.is_active = true
        ORDER BY t.name, a.name";

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

                        dgvAcademy.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "academy_id", "team_id", "city_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvAcademy.Columns[col] != null)
                                dgvAcademy.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading academies: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddAcademy_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbAcademyTeam.SelectedValue == -1 ||
                (int)cmbAcademyCity.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Team and a City.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbAcademyTeam.SelectedValue;
            int cityId = (int)cmbAcademyCity.SelectedValue;
            string name = txtAcademyName.Text.Trim();

            // 2. Validate Name
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Academy Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO academy (team_id, city_id, name, created_by) 
        VALUES (@teamId, @cityId, @name, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@cityId", cityId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Academy added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAcademiesAsync();
                        btnClearAcademy_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Academy name already exists for this team.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateAcademy_Click(object sender, EventArgs e)
        {
            if (_selectedAcademyId == 0)
            {
                MessageBox.Show("Please select an academy to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbAcademyTeam.SelectedValue == -1 ||
                (int)cmbAcademyCity.SelectedValue == -1)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbAcademyTeam.SelectedValue;
            int cityId = (int)cmbAcademyCity.SelectedValue;
            string name = txtAcademyName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Academy Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE academy SET 
            team_id = @teamId, 
            city_id = @cityId,
            name = @name,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            academy_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@cityId", cityId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedAcademyId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Academy updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAcademiesAsync();
                        btnClearAcademy_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating academy: " + ex.Message); }
        }

        private async void btnDeleteAcademy_Click(object sender, EventArgs e)
        {
            if (_selectedAcademyId == 0)
            {
                MessageBox.Show("Please select an academy to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE academy SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            academy_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedAcademyId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Academy deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadAcademiesAsync();
                            btnClearAcademy_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting academy: " + ex.Message); }
            }
        }

        private void dgvAcademy_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Valid Row Check
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvAcademy.Rows[e.RowIndex];

                    // 2. Ignore New Row Placeholder
                    if (row.IsNewRow) return;

                    // 3. Null ID Check
                    if (row.Cells["academy_id"].Value == null ||
                        row.Cells["academy_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedAcademyId = Convert.ToInt32(row.Cells["academy_id"].Value);

                    // Map Name
                    if (row.Cells["academy_name"].Value != null && row.Cells["academy_name"].Value != DBNull.Value)
                    {
                        txtAcademyName.Text = row.Cells["academy_name"].Value.ToString();
                    }
                    else
                    {
                        txtAcademyName.Clear();
                    }

                    // Map Team Combo
                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                    {
                        cmbAcademyTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);
                    }

                    // Map City Combo
                    if (row.Cells["city_id"].Value != null && row.Cells["city_id"].Value != DBNull.Value)
                    {
                        cmbAcademyCity.SelectedValue = Convert.ToInt32(row.Cells["city_id"].Value);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        

        private void btnClearAcademy_Click(object sender, EventArgs e)
        {
            _selectedAcademyId = 0;
            txtAcademyName.Clear();

            if (cmbAcademyTeam.Items.Count > 0) cmbAcademyTeam.SelectedIndex = 0;
            if (cmbAcademyCity.Items.Count > 0) cmbAcademyCity.SelectedIndex = 0;

            dgvAcademy.ClearSelection();
        }
    }
}
