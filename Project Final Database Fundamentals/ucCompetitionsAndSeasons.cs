using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Npgsql;

namespace Project_Final_Database_Fundamentals
{
    public partial class ucCompetitionsAndSeasons : UserControl
    {
        private readonly int _adminUserId;
        private int _selectedCompetitionId = 0;
        private int _selectedCompetitionSeasonId = 0;
        private int _selectedCompetitionTypeId = 0;
        private int _selectedCompetitionStageId = 0;
        private int _selectedSeasonId = 0;
        private int _selectedGroupId = 0;
        private int _selectedLeagueStandingId = 0;
        private int _selectedGroupStandingId = 0;
        private int _selectedCompSeasonTeamId = 0;

        public ucCompetitionsAndSeasons(int adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
        }




        private async Task LoadCountriesForCompetitionComboAsync()
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

                        cmbCompetitionCountry.DataSource = dt;
                        cmbCompetitionCountry.DisplayMember = "name";
                        cmbCompetitionCountry.ValueMember = "country_id";
                        cmbCompetitionCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading countries: " + ex.Message); }
        }

        private async Task LoadConfederationsForCompetitionComboAsync()
        {
            string query = @"SELECT confederation_id, name FROM ""confederation"" WHERE is_active = true ORDER BY name";
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
                        row["confederation_id"] = -1;
                        row["name"] = "-- Select Confederation --";
                        dt.Rows.InsertAt(row, 0);

                        cmbCompetitionConfederation.DataSource = dt;
                        cmbCompetitionConfederation.DisplayMember = "name";
                        cmbCompetitionConfederation.ValueMember = "confederation_id";
                        cmbCompetitionConfederation.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading confederations: " + ex.Message); }
        }

        private async Task LoadCompetitionTypesForComboAsync()
        {
            // Adjust 'competition_type_id' if your PK is named differently
            string query = @"SELECT type_id, type_name FROM competition_type WHERE is_active = true ORDER BY type_name";
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
                        row["type_id"] = -1;
                        row["type_name"] = "-- Select Type --";
                        dt.Rows.InsertAt(row, 0);

                        cmbCompetitionType.DataSource = dt;
                        cmbCompetitionType.DisplayMember = "type_name";
                        cmbCompetitionType.ValueMember = "type_id";
                        cmbCompetitionType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading types: " + ex.Message); }
        }

        private async Task LoadCompetitionsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // We need 3 JOINS to show names instead of IDs
            string query = @"
        SELECT 
            c.competition_id, 
            c.name, 
            c.country_id, 
            co.name AS country_name,
            c.confederation_id,
            conf.name AS confederation_name,
            c.competition_type_id,
            ct.type_name AS type_name
        FROM competition c
        INNER JOIN ""country"" co ON c.country_id = co.country_id
        INNER JOIN ""confederation"" conf ON c.confederation_id = conf.confederation_id
        INNER JOIN competition_type ct ON c.competition_type_id = ct.type_id
        WHERE c.is_active = true
        ORDER BY c.competition_id DESC";

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

                        dgvCompetition.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "competition_id", "country_id", "confederation_id", "competition_type_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvCompetition.Columns[col] != null)
                                dgvCompetition.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading competitions: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddCompetition_Click(object sender, EventArgs e)
        {
            string name = txtNameCompetition.Text.Trim();

            // 1. Validate Combos
            if (cmbCompetitionCountry.SelectedValue == null || (int)cmbCompetitionCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbCompetitionConfederation.SelectedValue == null || (int)cmbCompetitionConfederation.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Confederation.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbCompetitionType.SelectedValue == null || (int)cmbCompetitionType.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Competition Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get IDs
            int countryId = (int)cmbCompetitionCountry.SelectedValue;
            int confedId = (int)cmbCompetitionConfederation.SelectedValue;
            int typeId = (int)cmbCompetitionType.SelectedValue;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO competition (name, country_id, confederation_id, competition_type_id, created_by) 
        VALUES (@name, @countryId, @confedId, @typeId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@confedId", confedId);
                    command.Parameters.AddWithValue("@typeId", typeId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Competition added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionsAsync();
                        btnClearCompetition_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") MessageBox.Show("This Competition already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateCompetiton_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionId == 0)
            {
                MessageBox.Show("Please select a competition to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameCompetition.Text.Trim();

            // Validation
            if ((int)cmbCompetitionCountry.SelectedValue == -1 ||
                (int)cmbCompetitionConfederation.SelectedValue == -1 ||
                (int)cmbCompetitionType.SelectedValue == -1 ||
                string.IsNullOrEmpty(name))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int countryId = (int)cmbCompetitionCountry.SelectedValue;
            int confedId = (int)cmbCompetitionConfederation.SelectedValue;
            int typeId = (int)cmbCompetitionType.SelectedValue;

            string query = @"
        UPDATE competition SET 
            name = @name, 
            country_id = @countryId,
            confederation_id = @confedId,
            competition_type_id = @typeId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            competition_id = @compId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@confedId", confedId);
                    command.Parameters.AddWithValue("@typeId", typeId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@compId", _selectedCompetitionId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Competition updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionsAsync();
                        btnClearCompetition_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating competition: " + ex.Message); }
        }

        private async void btnDeleteCompetition_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionId == 0)
            {
                MessageBox.Show("Please select a competition to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE competition SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            competition_id = @compId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@compId", _selectedCompetitionId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Competition deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCompetitionsAsync();
                            btnClearCompetition_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting competition: " + ex.Message); }
            }
        }

        private void btnClearCompetition_Click(object sender, EventArgs e)
        {
            _selectedCompetitionId = 0;
            txtNameCompetition.Clear();

            if (cmbCompetitionCountry.Items.Count > 0) cmbCompetitionCountry.SelectedIndex = 0;
            if (cmbCompetitionConfederation.Items.Count > 0) cmbCompetitionConfederation.SelectedIndex = 0;
            if (cmbCompetitionType.Items.Count > 0) cmbCompetitionType.SelectedIndex = 0;

            dgvCompetition.ClearSelection();
        }

        private void dgvCompetition_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCompetition.Rows[e.RowIndex];

                _selectedCompetitionId = Convert.ToInt32(row.Cells["competition_id"].Value);
                txtNameCompetition.Text = row.Cells["name"].Value.ToString();

                // Set 3 ComboBoxes
                if (row.Cells["country_id"].Value != DBNull.Value)
                    cmbCompetitionCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);

                if (row.Cells["confederation_id"].Value != DBNull.Value)
                    cmbCompetitionConfederation.SelectedValue = Convert.ToInt32(row.Cells["confederation_id"].Value);

                if (row.Cells["competition_type_id"].Value != DBNull.Value)
                    cmbCompetitionType.SelectedValue = Convert.ToInt32(row.Cells["competition_type_id"].Value);
            }
        }

        private async void tabControlCompetitionsAndSeasons_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlCompetitionsAndSeasons.SelectedTab == tabCompetitions)
            {
                // Execute all 4 tasks in parallel
                var tGrid = LoadCompetitionsAsync();
                var tCountry = LoadCountriesForCompetitionComboAsync();
                var tConfed = LoadConfederationsForCompetitionComboAsync();
                var tType = LoadCompetitionTypesForComboAsync();

                await Task.WhenAll(tGrid, tCountry, tConfed, tType);
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabCompetitionSeasons)
            {
                var tGrid = LoadCompetitionSeasonsAsync();
                var tComp = LoadCompetitionsForSeasonComboAsync();
                var tSeason = LoadSeasonsForComboAsync();
                await Task.WhenAll(tGrid, tComp, tSeason);
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabCompetitionType)
            {
                await LoadCompetitionTypesAsync();
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabCompetitionStage)
            {
                // Load Grid and Combo in parallel
                var tGrid = LoadCompetitionStagesAsync();
                var tCombo = LoadCompSeasonsForStageComboAsync();
                await Task.WhenAll(tGrid, tCombo);
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabSeason)
            {
                await LoadSeasonsAsync();
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabGroups)
            {
                var tGrid = LoadGroupsAsync();
                var tStage = LoadStagesForGroupComboAsync();

                await Task.WhenAll(tGrid, tStage);
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabLeagueStandings)
            {
                // Parallel loading for maximum speed
                var tGrid = LoadLeagueStandingsAsync();
                var tSeason = LoadCompSeasonsForStandingComboAsync();
                var tTeam = LoadTeamsForStandingComboAsync();

                await Task.WhenAll(tGrid, tSeason, tTeam);
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabGroupsStandings)
            {
                // Parallel loading for speed
                var tGrid = LoadGroupStandingsAsync();
                var tGroup = LoadGroupsForStandingComboAsync();
                var tTeam = LoadTeamsForGroupStandingComboAsync();

                await Task.WhenAll(tGrid, tGroup, tTeam);
            }
            else if (tabControlCompetitionsAndSeasons.SelectedTab == tabCompetitionSeasonTeams)
            {
                // Load Grid + 2 Combos in parallel
                var tGrid = LoadCompSeasonTeamsAsync();
                var tSeason = LoadCompSeasonsForCSTComboAsync();
                var tTeam = LoadTeamsForCSTComboAsync();

                await Task.WhenAll(tGrid, tSeason, tTeam);
            }
        }





        private async Task LoadCompetitionsForSeasonComboAsync()
        {
            string query = @"SELECT competition_id, name FROM competition WHERE is_active = true ORDER BY name";
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
                        row["competition_id"] = -1;
                        row["name"] = "-- Select Competition --";
                        dt.Rows.InsertAt(row, 0);

                        // Ensure UI name matches
                        cmbCompetition.DataSource = dt;
                        cmbCompetition.DisplayMember = "name";
                        cmbCompetition.ValueMember = "competition_id";
                        cmbCompetition.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading competitions: " + ex.Message); }
        }

        private async Task LoadSeasonsForComboAsync()
        {
            // Adjust 'name' if your season table uses 'year' or another column
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

                        cmbSeason.DataSource = dt;
                        cmbSeason.DisplayMember = "name";
                        cmbSeason.ValueMember = "season_id";
                        cmbSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        private async Task LoadCompetitionSeasonsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // Double JOIN to get names
            string query = @"
        SELECT 
            cs.competition_season_id, 
            cs.competition_id, 
            c.name AS competition_name,
            cs.season_id,
            s.name AS season_name
        FROM competition_season cs
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        WHERE cs.is_active = true
        ORDER BY cs.competition_season_id DESC";

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

                        dgvCompetitionSeason.DataSource = dt;

                        // Hide internal IDs
                        if (dgvCompetitionSeason.Columns["competition_season_id"] != null)
                            dgvCompetitionSeason.Columns["competition_season_id"].Visible = false;
                        if (dgvCompetitionSeason.Columns["competition_id"] != null)
                            dgvCompetitionSeason.Columns["competition_id"].Visible = false;
                        if (dgvCompetitionSeason.Columns["season_id"] != null)
                            dgvCompetitionSeason.Columns["season_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading competition seasons: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddCompetitionSeason_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if (cmbCompetition.SelectedValue == null || (int)cmbCompetition.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Competition.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (cmbSeason.SelectedValue == null || (int)cmbSeason.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Season.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compId = (int)cmbCompetition.SelectedValue;
            int seasonId = (int)cmbSeason.SelectedValue;

            string query = @"
        INSERT INTO competition_season (competition_id, season_id, created_by) 
        VALUES (@compId, @seasonId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compId", compId);
                    command.Parameters.AddWithValue("@seasonId", seasonId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionSeasonsAsync();
                        btnClearCompetitionSeason_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") // Unique constraint (e.g., La Liga + 2024 already exists)
                    MessageBox.Show("This Competition-Season combination already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateCompetitionSeason_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionSeasonId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbCompetition.SelectedValue == -1 ||
                (int)cmbSeason.SelectedValue == -1)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compId = (int)cmbCompetition.SelectedValue;
            int seasonId = (int)cmbSeason.SelectedValue;

            string query = @"
        UPDATE competition_season SET 
            competition_id = @compId, 
            season_id = @seasonId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            competition_season_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compId", compId);
                    command.Parameters.AddWithValue("@seasonId", seasonId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedCompetitionSeasonId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionSeasonsAsync();
                        btnClearCompetitionSeason_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This combination already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteCompetitionSeason_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionSeasonId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE competition_season SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            competition_season_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedCompetitionSeasonId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCompetitionSeasonsAsync();
                            btnClearCompetitionSeason_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void btnClearCompetitionSeason_Click(object sender, EventArgs e)
        {
            _selectedCompetitionSeasonId = 0;

            if (cmbCompetition.Items.Count > 0)
            {
                cmbCompetition.SelectedIndex = 0;
            }

            if (cmbSeason.Items.Count > 0)
            {
                cmbSeason.SelectedIndex = 0;
            }

            dgvCompetitionSeason.ClearSelection();
        }

        private void dgvCompetitionSeason_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCompetitionSeason.Rows[e.RowIndex];

                _selectedCompetitionSeasonId = Convert.ToInt32(row.Cells["competition_season_id"].Value);

                // Set ComboBoxes
                if (row.Cells["competition_id"].Value != DBNull.Value)
                    cmbCompetition.SelectedValue = Convert.ToInt32(row.Cells["competition_id"].Value);

                if (row.Cells["season_id"].Value != DBNull.Value)
                    cmbSeason.SelectedValue = Convert.ToInt32(row.Cells["season_id"].Value);
            }
        }




        private async Task LoadCompetitionTypesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // Fetching ID and type_name
            string query = @"
        SELECT 
            type_id, 
            type_name 
        FROM competition_type
        WHERE is_active = true
        ORDER BY type_id DESC";

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

                        dgvCompetitionType.DataSource = dt;

                        // Hide the ID column
                        if (dgvCompetitionType.Columns["type_id"] != null)
                            dgvCompetitionType.Columns["type_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading competition types: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddCompetitionType_Click(object sender, EventArgs e)
        {
            string typeName = txtNameCompetitionType.Text.Trim(); // Ensure TextBox is named correctly

            if (string.IsNullOrEmpty(typeName))
            {
                MessageBox.Show("Type Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO competition_type (type_name, created_by) 
        VALUES (@typeName, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@typeName", typeName);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Competition Type added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionTypesAsync(); // Async refresh
                        btnClearCompetitionType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") // Unique violation
                    MessageBox.Show("This Competition Type already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateCompetitionType_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionTypeId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string typeName = txtNameCompetitionType.Text.Trim();

            if (string.IsNullOrEmpty(typeName))
            {
                MessageBox.Show("Type Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE competition_type SET 
            type_name = @typeName, 
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            type_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@typeName", typeName);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedCompetitionTypeId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Competition Type updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionTypesAsync();
                        btnClearCompetitionType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Competition Type already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating competition type: " + ex.Message);
            }
        }

        private async void btnDeleteCompetitionType_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionTypeId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this competition type?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE competition_type SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            type_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedCompetitionTypeId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Competition Type deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCompetitionTypesAsync();
                            btnClearCompetitionType_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting competition type: " + ex.Message);
                }
            }
        }

        private void btnClearCompetitionType_Click(object sender, EventArgs e)
        {
            _selectedCompetitionTypeId = 0;
            txtNameCompetitionType.Clear();
            dgvCompetitionType.ClearSelection();
        }

        private void dgvCompetitionType_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCompetitionType.Rows[e.RowIndex];

                _selectedCompetitionTypeId = Convert.ToInt32(row.Cells["type_id"].Value);
                txtNameCompetitionType.Text = row.Cells["type_name"].Value.ToString();
            }
        }




        // 1. Load Competition-Seasons into ComboBox (with friendly display name)
        private async Task LoadCompSeasonsForStageComboAsync()
        {
            // We concatenate Competition Name + Season Name to make it readable
            // E.g., "Premier League - 2024"
            string query = @"
        SELECT 
            cs.competition_season_id, 
            c.name || ' - ' || s.name AS display_name
        FROM competition_season cs
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        WHERE cs.is_active = true
        ORDER BY c.name, s.name";

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
                        row["competition_season_id"] = -1;
                        row["display_name"] = "-- Select Competition Season --";
                        dt.Rows.InsertAt(row, 0);

                        // Bind to ComboBox (Assumed name: cmbCompStage_CompSeason)
                        cmbCompStageCompSeason.DataSource = dt;
                        cmbCompStageCompSeason.DisplayMember = "display_name";
                        cmbCompStageCompSeason.ValueMember = "competition_season_id";
                        cmbCompStageCompSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading competition seasons: " + ex.Message); }
        }

        // 2. Load Main Grid for Stages
        private async Task LoadCompetitionStagesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            st.stage_id, 
            st.name, 
            st.stage_order,
            st.competition_season_id,
            (c.name || ' - ' || s.name) AS comp_season_name
        FROM competiton_stage st
        INNER JOIN competition_season cs ON st.competition_season_id = cs.competition_season_id
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        WHERE st.is_active = true
        ORDER BY st.competition_season_id, st.stage_order";

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

                        dgvCompetitionStage.DataSource = dt;

                        // Hide internal IDs
                        if (dgvCompetitionStage.Columns["competition_stage_id"] != null)
                            dgvCompetitionStage.Columns["competition_stage_id"].Visible = false;
                        if (dgvCompetitionStage.Columns["competition_season_id"] != null)
                            dgvCompetitionStage.Columns["competition_season_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stages: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddCompetitionStage_Click(object sender, EventArgs e)
        {
            string name = txtNameCompetitionStage.Text.Trim();
            int stageOrder = (int)nudStageOrder.Value; // Getting value from NumericUpDown

            // 1. Validate Combo
            if (cmbCompStageCompSeason.SelectedValue == null || (int)cmbCompStageCompSeason.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Competition Season.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int compSeasonId = (int)cmbCompStageCompSeason.SelectedValue;

            // 2. Validate Name
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Stage Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO competiton_stage (competition_season_id, name, stage_order, created_by) 
        VALUES (@compSeasonId, @name, @order, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@order", stageOrder);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stage added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionStagesAsync();
                        btnClearCompetitionStage_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Stage already exists for this Season.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateCompetitionStage_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionStageId == 0)
            {
                MessageBox.Show("Please select a stage to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameCompetitionStage.Text.Trim();
            int stageOrder = (int)nudStageOrder.Value;

            if ((int)cmbCompStageCompSeason.SelectedValue == -1 || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compSeasonId = (int)cmbCompStageCompSeason.SelectedValue;

            string query = @"
        UPDATE competiton_stage SET 
            competition_season_id = @compSeasonId, 
            name = @name,
            stage_order = @order,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            competition_stage_id = @stageId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@order", stageOrder);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@stageId", _selectedCompetitionStageId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stage updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompetitionStagesAsync();
                        btnClearCompetitionStage_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating stage: " + ex.Message); }
        }

        private async void btnDeleteCompetitionStage_Click(object sender, EventArgs e)
        {
            if (_selectedCompetitionStageId == 0)
            {
                MessageBox.Show("Please select a stage to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE competition_stage SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            competition_stage_id = @stageId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@stageId", _selectedCompetitionStageId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Stage deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCompetitionStagesAsync();
                            btnClearCompetitionStage_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting stage: " + ex.Message); }
            }
        }

        private void btnClearCompetitionStage_Click(object sender, EventArgs e)
        {
            _selectedCompetitionStageId = 0;
            txtNameCompetitionStage.Clear();
            nudStageOrder.Value = 0;

            if (cmbCompStageCompSeason.Items.Count > 0)
                cmbCompStageCompSeason.SelectedIndex = 0;

            dgvCompetitionStage.ClearSelection();
        }

        private void dgvCompetitionStage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCompetitionStage.Rows[e.RowIndex];

                _selectedCompetitionStageId = Convert.ToInt32(row.Cells["competition_stage_id"].Value);

                txtNameCompetitionStage.Text = row.Cells["name"].Value.ToString();

                // Handle NumericUpDown
                if (row.Cells["stage_order"].Value != DBNull.Value)
                    nudStageOrder.Value = Convert.ToDecimal(row.Cells["stage_order"].Value);
                else
                    nudStageOrder.Value = 0;

                // Handle ComboBox
                if (row.Cells["competition_season_id"].Value != DBNull.Value)
                    cmbCompStageCompSeason.SelectedValue = Convert.ToInt32(row.Cells["competition_season_id"].Value);
            }
        }



        // --- ASYNC: Load Seasons ---
        private async Task LoadSeasonsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            season_id, 
            name, 
            start_date, 
            end_date 
        FROM season
        WHERE is_active = true
        ORDER BY season_id DESC";

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

                        dgvSeason.DataSource = dt;

                        // Hide the ID column
                        if (dgvSeason.Columns["season_id"] != null)
                            dgvSeason.Columns["season_id"].Visible = false;

                        // Optional: Format date columns for better readability
                        if (dgvSeason.Columns["start_date"] != null)
                            dgvSeason.Columns["start_date"].DefaultCellStyle.Format = "d"; // Short date format

                        if (dgvSeason.Columns["end_date"] != null)
                            dgvSeason.Columns["end_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading seasons: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddSeason_Click(object sender, EventArgs e)
        {
            string name = txtNameSeason.Text.Trim();
            DateTime startDate = dtpStartDate.Value;
            DateTime endDate = dtpEndDate.Value;

            // 1. Validate Name
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Validate Dates
            if (endDate < startDate)
            {
                MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO season (name, start_date, end_date, created_by) 
        VALUES (@name, @start, @end, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@start", startDate);
                    command.Parameters.AddWithValue("@end", endDate);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Season added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSeasonsAsync(); // Async refresh
                        btnClearSeason_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") // Unique violation
                    MessageBox.Show("This Season Name already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateSeason_Click(object sender, EventArgs e)
        {
            if (_selectedSeasonId == 0)
            {
                MessageBox.Show("Please select a season to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameSeason.Text.Trim();
            DateTime startDate = dtpStartDate.Value;
            DateTime endDate = dtpEndDate.Value;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (endDate < startDate)
            {
                MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE season SET 
            name = @name, 
            start_date = @start,
            end_date = @end,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            season_id = @seasonId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@start", startDate);
                    command.Parameters.AddWithValue("@end", endDate);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@seasonId", _selectedSeasonId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Season updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSeasonsAsync();
                        btnClearSeason_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating season: " + ex.Message);
            }
        }

        private async void btnDeleteSeason_Click(object sender, EventArgs e)
        {
            if (_selectedSeasonId == 0)
            {
                MessageBox.Show("Please select a season to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this season?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE season SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            season_id = @seasonId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@seasonId", _selectedSeasonId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Season deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSeasonsAsync();
                            btnClearSeason_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting season: " + ex.Message);
                }
            }
        }

        private void btnClearSeason_Click(object sender, EventArgs e)
        {
            _selectedSeasonId = 0;
            txtNameSeason.Clear();

            // Reset dates to current date
            dtpStartDate.Value = DateTime.Now;
            dtpEndDate.Value = DateTime.Now;

            dgvSeason.ClearSelection();
        }

        private void dgvSeason_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSeason.Rows[e.RowIndex];

                _selectedSeasonId = Convert.ToInt32(row.Cells["season_id"].Value);
                txtNameSeason.Text = row.Cells["name"].Value.ToString();

                // Handle Date conversion safely
                if (row.Cells["start_date"].Value != DBNull.Value)
                    dtpStartDate.Value = Convert.ToDateTime(row.Cells["start_date"].Value);

                if (row.Cells["end_date"].Value != DBNull.Value)
                    dtpEndDate.Value = Convert.ToDateTime(row.Cells["end_date"].Value);
            }
        }





        private async Task LoadStagesForGroupComboAsync()
        {
            // We join with Competition to make the Stage name meaningful (e.g., "Champions League - Group Stage")
            string query = @"
        SELECT 
            st.stage_id, 
            c.name || ' - ' || st.name AS display_name
        FROM competiton_stage st
        INNER JOIN competition_season cs ON st.competition_season_id = cs.competition_season_id
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        WHERE st.is_active = true
        ORDER BY c.name, st.stage_order";

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
                        row["stage_id"] = -1;
                        row["display_name"] = "-- Select Stage --";
                        dt.Rows.InsertAt(row, 0);

                        cmbGroupStage.DataSource = dt;
                        cmbGroupStage.DisplayMember = "display_name";
                        cmbGroupStage.ValueMember = "stage_id";
                        cmbGroupStage.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stages: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadGroupsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // NOTE: 'group' must be in quotes
            string query = @"
        SELECT 
            g.group_id, 
            g.group_name, 
            g.qualification_slots,
            g.stage_id,
            st.name AS stage_name
        FROM ""group"" g
        INNER JOIN competiton_stage st ON g.stage_id = st.stage_id
        WHERE g.is_active = true
        ORDER BY g.group_id DESC";

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

                        dgvGroup.DataSource = dt;

                        // Hide internal IDs
                        if (dgvGroup.Columns["group_id"] != null) dgvGroup.Columns["group_id"].Visible = false;
                        if (dgvGroup.Columns["stage_id"] != null) dgvGroup.Columns["stage_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading groups: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddGroup_Click(object sender, EventArgs e)
        {
            string name = txtNameGroup.Text.Trim();

            // 1. Validate Stage Combo
            if (cmbGroupStage.SelectedValue == null || (int)cmbGroupStage.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Stage.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int stageId = (int)cmbGroupStage.SelectedValue;

            // 2. Validate Slots Combo
            if (cmbQualificationSlots.SelectedItem == null)
            {
                MessageBox.Show("Please select Qualification Slots (2 or 3).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int slots = int.Parse(cmbQualificationSlots.SelectedItem.ToString());

            // 3. Validate Name
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Group Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // NOTE: Using "group" in quotes
            string query = @"
        INSERT INTO ""group"" (stage_id, group_name, qualification_slots, created_by) 
        VALUES (@stageId, @name, @slots, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@stageId", stageId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@slots", slots);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Group added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadGroupsAsync();
                        btnClearGroup_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Group already exists in this stage.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateGroup_Click(object sender, EventArgs e)
        {
            if (_selectedGroupId == 0)
            {
                MessageBox.Show("Please select a group to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameGroup.Text.Trim();

            if ((int)cmbGroupStage.SelectedValue == -1 ||
                cmbQualificationSlots.SelectedItem == null ||
                string.IsNullOrEmpty(name))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int stageId = (int)cmbGroupStage.SelectedValue;
            int slots = int.Parse(cmbQualificationSlots.SelectedItem.ToString());

            string query = @"
        UPDATE ""group"" SET 
            stage_id = @stageId, 
            group_name = @name,
            qualification_slots = @slots,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            group_id = @groupId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@stageId", stageId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@slots", slots);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@groupId", _selectedGroupId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Group updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadGroupsAsync();
                        btnClearGroup_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating group: " + ex.Message); }
        }

        private async void btnDeleteGroup_Click(object sender, EventArgs e)
        {
            if (_selectedGroupId == 0)
            {
                MessageBox.Show("Please select a group to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE ""group"" SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            group_id = @groupId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@groupId", _selectedGroupId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Group deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadGroupsAsync();
                            btnClearGroup_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting group: " + ex.Message); }
            }
        }

        private void btnClearGroup_Click(object sender, EventArgs e)
        {
            _selectedGroupId = 0;
            txtNameGroup.Clear();

            // Reset Combos
            if (cmbGroupStage.Items.Count > 0) cmbGroupStage.SelectedIndex = 0;
            if (cmbQualificationSlots.Items.Count > 0) cmbQualificationSlots.SelectedIndex = 0;

            dgvGroup.ClearSelection();
        }

        private void dgvGroup_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvGroup.Rows[e.RowIndex];

                _selectedGroupId = Convert.ToInt32(row.Cells["group_id"].Value);

                txtNameGroup.Text = row.Cells["name"].Value.ToString();

                // Handle Stage Combo
                if (row.Cells["competition_stage_id"].Value != DBNull.Value)
                    cmbGroupStage.SelectedValue = Convert.ToInt32(row.Cells["competition_stage_id"].Value);

                // Handle Slots Combo (Fixed Values)
                if (row.Cells["qualification_slots"].Value != DBNull.Value)
                {
                    string slotsValue = row.Cells["qualification_slots"].Value.ToString();
                    cmbQualificationSlots.SelectedItem = slotsValue; // Matches "2" or "3"
                }
            }
        }







        // 1. Load Competition-Seasons (Context: "Premier League - 2023/2024")
        private async Task LoadCompSeasonsForStandingComboAsync()
        {
            string query = @"
        SELECT 
            cs.competition_season_id, 
            c.name || ' - ' || s.name AS display_name
        FROM competition_season cs
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        WHERE cs.is_active = true
        ORDER BY c.name, s.name";

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
                        row["competition_season_id"] = -1;
                        row["display_name"] = "-- Select Season --";
                        dt.Rows.InsertAt(row, 0);

                        cmbLeagueStanding_CompSeason.DataSource = dt;
                        cmbLeagueStanding_CompSeason.DisplayMember = "display_name";
                        cmbLeagueStanding_CompSeason.ValueMember = "competition_season_id";
                        cmbLeagueStanding_CompSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 2. Load Teams (Assuming table 'team' exists with team_id and name)
        private async Task LoadTeamsForStandingComboAsync()
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

                        cmbLeagueStanding_Team.DataSource = dt;
                        cmbLeagueStanding_Team.DisplayMember = "name";
                        cmbLeagueStanding_Team.ValueMember = "team_id";
                        cmbLeagueStanding_Team.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadLeagueStandingsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            ls.league_standing_id, 
            ls.competition_season_id,
            (c.name || ' - ' || s.name) AS comp_season_name,
            ls.team_id,
            t.name AS team_name,
            ls.rank, ls.played, ls.won, ls.drawn, ls.lost, 
            ls.goals_for, ls.goals_against, ls.goal_difference, ls.points
        FROM league_standing ls
        INNER JOIN competition_season cs ON ls.competition_season_id = cs.competition_season_id
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        INNER JOIN team t ON ls.team_id = t.team_id
        WHERE ls.is_active = true
        ORDER BY ls.competition_season_id, ls.rank";

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

                        dgvLeagueStanding.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "league_standing_id", "competition_season_id", "team_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvLeagueStanding.Columns[col] != null)
                                dgvLeagueStanding.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading standings: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddLeagueStanding_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbLeagueStanding_CompSeason.SelectedValue == -1 ||
                (int)cmbLeagueStanding_Team.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Competition-Season and a Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compSeasonId = (int)cmbLeagueStanding_CompSeason.SelectedValue;
            int teamId = (int)cmbLeagueStanding_Team.SelectedValue;

            // 2. Gather Numeric Data
            int rank = (int)numLeagueStanding_Rank.Value;
            int played = (int)numLeagueStanding_Played.Value;
            int won = (int)numLeagueStanding_Won.Value;
            int drawn = (int)numLeagueStanding_Drawn.Value;
            int lost = (int)numLeagueStanding_Lost.Value;
            int gf = (int)numLeagueStanding_GF.Value;
            int ga = (int)numLeagueStanding_GA.Value;
            int gd = (int)numLeagueStanding_GD.Value;
            int points = (int)numLeagueStanding_Points.Value;

            string query = @"
        INSERT INTO league_standing 
        (competition_season_id, team_id, rank, played, won, drawn, lost, goals_for, goals_against, goal_difference, points, created_by) 
        VALUES 
        (@compSeasonId, @teamId, @rank, @played, @won, @drawn, @lost, @gf, @ga, @gd, @points, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@rank", rank);
                    command.Parameters.AddWithValue("@played", played);
                    command.Parameters.AddWithValue("@won", won);
                    command.Parameters.AddWithValue("@drawn", drawn);
                    command.Parameters.AddWithValue("@lost", lost);
                    command.Parameters.AddWithValue("@gf", gf);
                    command.Parameters.AddWithValue("@ga", ga);
                    command.Parameters.AddWithValue("@gd", gd);
                    command.Parameters.AddWithValue("@points", points);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Standing added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadLeagueStandingsAsync();
                        btnClearLeagueStanding_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team is already registered in this Season.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateLeagueStanding_Click(object sender, EventArgs e)
        {
            if (_selectedLeagueStandingId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbLeagueStanding_CompSeason.SelectedValue == -1 || (int)cmbLeagueStanding_Team.SelectedValue == -1)
            {
                MessageBox.Show("Selection fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compSeasonId = (int)cmbLeagueStanding_CompSeason.SelectedValue;
            int teamId = (int)cmbLeagueStanding_Team.SelectedValue;

            string query = @"
        UPDATE league_standing SET 
            competition_season_id = @compSeasonId, 
            team_id = @teamId,
            rank = @rank, 
            played = @played, 
            won = @won, 
            drawn = @drawn, 
            lost = @lost, 
            goals_for = @gf, 
            goals_against = @ga, 
            goal_difference = @gd, 
            points = @points,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            league_standing_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@rank", (int)numLeagueStanding_Rank.Value);
                    command.Parameters.AddWithValue("@played", (int)numLeagueStanding_Played.Value);
                    command.Parameters.AddWithValue("@won", (int)numLeagueStanding_Won.Value);
                    command.Parameters.AddWithValue("@drawn", (int)numLeagueStanding_Drawn.Value);
                    command.Parameters.AddWithValue("@lost", (int)numLeagueStanding_Lost.Value);
                    command.Parameters.AddWithValue("@gf", (int)numLeagueStanding_GF.Value);
                    command.Parameters.AddWithValue("@ga", (int)numLeagueStanding_GA.Value);
                    command.Parameters.AddWithValue("@gd", (int)numLeagueStanding_GD.Value);
                    command.Parameters.AddWithValue("@points", (int)numLeagueStanding_Points.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedLeagueStandingId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Standing updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadLeagueStandingsAsync();
                        btnClearLeagueStanding_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating standing: " + ex.Message); }
        }

        private async void btnDeleteLeagueStanding_Click(object sender, EventArgs e)
        {
            if (_selectedLeagueStandingId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE league_standing SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            league_standing_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedLeagueStandingId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Standing deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadLeagueStandingsAsync();
                            btnClearLeagueStanding_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void btnClearLeagueStanding_Click(object sender, EventArgs e)
        {
            _selectedLeagueStandingId = 0;

            // Reset Combos
            if (cmbLeagueStanding_CompSeason.Items.Count > 0) cmbLeagueStanding_CompSeason.SelectedIndex = 0;
            if (cmbLeagueStanding_Team.Items.Count > 0) cmbLeagueStanding_Team.SelectedIndex = 0;

            // Reset Numerics
            numLeagueStanding_Rank.Value = 0;
            numLeagueStanding_Played.Value = 0;
            numLeagueStanding_Won.Value = 0;
            numLeagueStanding_Drawn.Value = 0;
            numLeagueStanding_Lost.Value = 0;
            numLeagueStanding_GF.Value = 0;
            numLeagueStanding_GA.Value = 0;
            numLeagueStanding_GD.Value = 0;
            numLeagueStanding_Points.Value = 0;

            dgvLeagueStanding.ClearSelection();
        }

        private void dgvLeagueStanding_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvLeagueStanding.Rows[e.RowIndex];

                _selectedLeagueStandingId = Convert.ToInt32(row.Cells["league_standing_id"].Value);

                // Map Combos
                if (row.Cells["competition_season_id"].Value != DBNull.Value)
                    cmbLeagueStanding_CompSeason.SelectedValue = Convert.ToInt32(row.Cells["competition_season_id"].Value);

                if (row.Cells["team_id"].Value != DBNull.Value)
                    cmbLeagueStanding_Team.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                // Map Numerics safely
                numLeagueStanding_Rank.Value = Convert.ToDecimal(row.Cells["rank"].Value);
                numLeagueStanding_Played.Value = Convert.ToDecimal(row.Cells["played"].Value);
                numLeagueStanding_Won.Value = Convert.ToDecimal(row.Cells["won"].Value);
                numLeagueStanding_Drawn.Value = Convert.ToDecimal(row.Cells["drawn"].Value);
                numLeagueStanding_Lost.Value = Convert.ToDecimal(row.Cells["lost"].Value);
                numLeagueStanding_GF.Value = Convert.ToDecimal(row.Cells["goals_for"].Value);
                numLeagueStanding_GA.Value = Convert.ToDecimal(row.Cells["goals_against"].Value);
                numLeagueStanding_GD.Value = Convert.ToDecimal(row.Cells["goal_difference"].Value);
                numLeagueStanding_Points.Value = Convert.ToDecimal(row.Cells["points"].Value);
            }
        }







        // 1. Load Groups into ComboBox
        private async Task LoadGroupsForStandingComboAsync()
        {
            // NOTE: "group" is a reserved word, must be quoted
            string query = @"SELECT group_id, group_name FROM ""group"" WHERE is_active = true ORDER BY group_name";

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
                        row["group_id"] = -1;
                        row["group_name"] = "-- Select Group --";
                        dt.Rows.InsertAt(row, 0);

                        cmbGroupStanding_Group.DataSource = dt;
                        cmbGroupStanding_Group.DisplayMember = "group_name";
                        cmbGroupStanding_Group.ValueMember = "group_id";
                        cmbGroupStanding_Group.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading groups: " + ex.Message); }
        }

        // 2. Load Teams into ComboBox
        private async Task LoadTeamsForGroupStandingComboAsync()
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

                        cmbGroupStanding_Team.DataSource = dt;
                        cmbGroupStanding_Team.DisplayMember = "name";
                        cmbGroupStanding_Team.ValueMember = "team_id";
                        cmbGroupStanding_Team.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadGroupStandingsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            gs.group_standing_id, 
            gs.group_id,
            g.group_name AS group_name,
            gs.team_id,
            t.name AS team_name,
            gs.rank, gs.played, gs.won, gs.drawn, gs.lost, 
            gs.goals_for, gs.goals_against, gs.goal_difference, gs.points
        FROM group_standing gs
        INNER JOIN ""group"" g ON gs.group_id = g.group_id
        INNER JOIN team t ON gs.team_id = t.team_id
        WHERE gs.is_active = true
        ORDER BY g.group_name, gs.rank";

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

                        dgvGroupStanding.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "group_standing_id", "group_id", "team_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvGroupStanding.Columns[col] != null)
                                dgvGroupStanding.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading group standings: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddGroupStanding_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbGroupStanding_Group.SelectedValue == -1 ||
                (int)cmbGroupStanding_Team.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Group and a Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int groupId = (int)cmbGroupStanding_Group.SelectedValue;
            int teamId = (int)cmbGroupStanding_Team.SelectedValue;

            // 2. Gather Numeric Data
            int rank = (int)numGroupStanding_Rank.Value;
            int played = (int)numGroupStanding_Played.Value;
            int won = (int)numGroupStanding_Won.Value;
            int drawn = (int)numGroupStanding_Drawn.Value;
            int lost = (int)numGroupStanding_Lost.Value;
            int gf = (int)numGroupStanding_GF.Value;
            int ga = (int)numGroupStanding_GA.Value;
            int gd = (int)numGroupStanding_GD.Value;
            int points = (int)numGroupStanding_Points.Value;

            string query = @"
        INSERT INTO group_standing 
        (group_id, team_id, rank, played, won, drawn, lost, goals_for, goals_against, goal_difference, points, created_by) 
        VALUES 
        (@groupId, @teamId, @rank, @played, @won, @drawn, @lost, @gf, @ga, @gd, @points, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@groupId", groupId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@rank", rank);
                    command.Parameters.AddWithValue("@played", played);
                    command.Parameters.AddWithValue("@won", won);
                    command.Parameters.AddWithValue("@drawn", drawn);
                    command.Parameters.AddWithValue("@lost", lost);
                    command.Parameters.AddWithValue("@gf", gf);
                    command.Parameters.AddWithValue("@ga", ga);
                    command.Parameters.AddWithValue("@gd", gd);
                    command.Parameters.AddWithValue("@points", points);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Group Standing added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadGroupStandingsAsync();
                        btnClearGroupStanding_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team is already registered in this Group.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateGroupStanding_Click(object sender, EventArgs e)
        {
            if (_selectedGroupStandingId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbGroupStanding_Group.SelectedValue == -1 ||
                (int)cmbGroupStanding_Team.SelectedValue == -1)
            {
                MessageBox.Show("Select Group and Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int groupId = (int)cmbGroupStanding_Group.SelectedValue;
            int teamId = (int)cmbGroupStanding_Team.SelectedValue;

            string query = @"
        UPDATE group_standing SET 
            group_id = @groupId, 
            team_id = @teamId,
            rank = @rank, 
            played = @played, 
            won = @won, 
            drawn = @drawn, 
            lost = @lost, 
            goals_for = @gf, 
            goals_against = @ga, 
            goal_difference = @gd, 
            points = @points,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            group_standing_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@groupId", groupId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@rank", (int)numGroupStanding_Rank.Value);
                    command.Parameters.AddWithValue("@played", (int)numGroupStanding_Played.Value);
                    command.Parameters.AddWithValue("@won", (int)numGroupStanding_Won.Value);
                    command.Parameters.AddWithValue("@drawn", (int)numGroupStanding_Drawn.Value);
                    command.Parameters.AddWithValue("@lost", (int)numGroupStanding_Lost.Value);
                    command.Parameters.AddWithValue("@gf", (int)numGroupStanding_GF.Value);
                    command.Parameters.AddWithValue("@ga", (int)numGroupStanding_GA.Value);
                    command.Parameters.AddWithValue("@gd", (int)numGroupStanding_GD.Value);
                    command.Parameters.AddWithValue("@points", (int)numGroupStanding_Points.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedGroupStandingId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Group Standing updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadGroupStandingsAsync();
                        btnClearGroupStanding_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating group standing: " + ex.Message); }
        }

        private async void btnDeleteGroupStanding_Click(object sender, EventArgs e)
        {
            if (_selectedGroupStandingId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE group_standing SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            group_standing_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedGroupStandingId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Group Standing deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadGroupStandingsAsync();
                            btnClearGroupStanding_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void btnClearGroupStanding_Click(object sender, EventArgs e)
        {
            _selectedGroupStandingId = 0;

            // Reset Combos
            if (cmbGroupStanding_Group.Items.Count > 0) cmbGroupStanding_Group.SelectedIndex = 0;
            if (cmbGroupStanding_Team.Items.Count > 0) cmbGroupStanding_Team.SelectedIndex = 0;

            // Reset Numerics
            numGroupStanding_Rank.Value = 0;
            numGroupStanding_Played.Value = 0;
            numGroupStanding_Won.Value = 0;
            numGroupStanding_Drawn.Value = 0;
            numGroupStanding_Lost.Value = 0;
            numGroupStanding_GF.Value = 0;
            numGroupStanding_GA.Value = 0;
            numGroupStanding_GD.Value = 0;
            numGroupStanding_Points.Value = 0;

            dgvGroupStanding.ClearSelection();
        }

        private void dgvGroupStanding_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvGroupStanding.Rows[e.RowIndex];

                _selectedGroupStandingId = Convert.ToInt32(row.Cells["group_standing_id"].Value);

                // Map Combos
                if (row.Cells["group_id"].Value != DBNull.Value)
                    cmbGroupStanding_Group.SelectedValue = Convert.ToInt32(row.Cells["group_id"].Value);

                if (row.Cells["team_id"].Value != DBNull.Value)
                    cmbGroupStanding_Team.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                // Map Numerics
                numGroupStanding_Rank.Value = Convert.ToDecimal(row.Cells["rank"].Value);
                numGroupStanding_Played.Value = Convert.ToDecimal(row.Cells["played"].Value);
                numGroupStanding_Won.Value = Convert.ToDecimal(row.Cells["won"].Value);
                numGroupStanding_Drawn.Value = Convert.ToDecimal(row.Cells["drawn"].Value);
                numGroupStanding_Lost.Value = Convert.ToDecimal(row.Cells["lost"].Value);
                numGroupStanding_GF.Value = Convert.ToDecimal(row.Cells["goals_for"].Value);
                numGroupStanding_GA.Value = Convert.ToDecimal(row.Cells["goals_against"].Value);
                numGroupStanding_GD.Value = Convert.ToDecimal(row.Cells["goal_difference"].Value);
                numGroupStanding_Points.Value = Convert.ToDecimal(row.Cells["points"].Value);
            }
        }




        // 1. Load Competition-Seasons into ComboBox (Display: "Premier League - 2024")
        private async Task LoadCompSeasonsForCSTComboAsync()
        {
            string query = @"
        SELECT 
            cs.competition_season_id, 
            c.name || ' - ' || s.name AS display_name
        FROM competition_season cs
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        WHERE cs.is_active = true
        ORDER BY c.name, s.name";

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
                        row["competition_season_id"] = -1;
                        row["display_name"] = "-- Select Season --";
                        dt.Rows.InsertAt(row, 0);

                        cmbCompetitonSeasonTeam_CompetitonSeason.DataSource = dt;
                        cmbCompetitonSeasonTeam_CompetitonSeason.DisplayMember = "display_name";
                        cmbCompetitonSeasonTeam_CompetitonSeason.ValueMember = "competition_season_id";
                        cmbCompetitonSeasonTeam_CompetitonSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 2. Load Teams into ComboBox
        private async Task LoadTeamsForCSTComboAsync()
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

                        cmbCompetitionSeasonTeam_Team.DataSource = dt;
                        cmbCompetitionSeasonTeam_Team.DisplayMember = "name";
                        cmbCompetitionSeasonTeam_Team.ValueMember = "team_id";
                        cmbCompetitionSeasonTeam_Team.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadCompSeasonTeamsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            cst.competition_season_team_id, 
            cst.competition_season_id,
            (c.name || ' - ' || s.name) AS comp_season_name,
            cst.team_id,
            t.name AS team_name,
            cst.final_position, 
            cst.overall_status
        FROM competition_season_team cst
        INNER JOIN competition_season cs ON cst.competition_season_id = cs.competition_season_id
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        INNER JOIN team t ON cst.team_id = t.team_id
        WHERE cst.is_active = true
        ORDER BY comp_season_name, cst.final_position";

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

                        dgvCompetitionSeasonTeams.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "competition_season_team_id", "competition_season_id", "team_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvCompetitionSeasonTeams.Columns[col] != null)
                                dgvCompetitionSeasonTeams.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading data: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddCompetitionSeasonTeam_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbCompetitonSeasonTeam_CompetitonSeason.SelectedValue == -1 ||
                (int)cmbCompetitionSeasonTeam_Team.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Competition-Season and a Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compSeasonId = (int)cmbCompetitonSeasonTeam_CompetitonSeason.SelectedValue;
            int teamId = (int)cmbCompetitionSeasonTeam_Team.SelectedValue;

            // 2. Get inputs
            int finalPosition = (int)numFinalPosition.Value;
            string status = txtOverallStatus.Text.Trim();

            string query = @"
        INSERT INTO competition_season_team 
        (competition_season_id, team_id, final_position, overall_status, created_by) 
        VALUES 
        (@compSeasonId, @teamId, @pos, @status, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@pos", finalPosition);
                    command.Parameters.AddWithValue("@status", status); // Optional: DBNull.Value if empty allowed
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompSeasonTeamsAsync();
                        btnClearCompetitionSeasonTeam_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team is already registered in this Season.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateCompetitionSeasonTeam_Click(object sender, EventArgs e)
        {
            if (_selectedCompSeasonTeamId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbCompetitonSeasonTeam_CompetitonSeason.SelectedValue == -1 ||
                (int)cmbCompetitionSeasonTeam_Team.SelectedValue == -1)
            {
                MessageBox.Show("Comboboxes are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int compSeasonId = (int)cmbCompetitonSeasonTeam_CompetitonSeason.SelectedValue;
            int teamId = (int)cmbCompetitionSeasonTeam_Team.SelectedValue;
            int finalPosition = (int)numFinalPosition.Value;
            string status = txtOverallStatus.Text.Trim();

            string query = @"
        UPDATE competition_season_team SET 
            competition_season_id = @compSeasonId, 
            team_id = @teamId,
            final_position = @pos,
            overall_status = @status,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            competition_season_team_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@pos", finalPosition);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedCompSeasonTeamId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCompSeasonTeamsAsync();
                        btnClearCompetitionSeasonTeam_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteCompetitionSeasonTeam_Click(object sender, EventArgs e)
        {
            if (_selectedCompSeasonTeamId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE competition_season_team SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            competition_season_team_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedCompSeasonTeamId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCompSeasonTeamsAsync();
                            btnClearCompetitionSeasonTeam_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void btnClearCompetitionSeasonTeam_Click(object sender, EventArgs e)
        {
            _selectedCompSeasonTeamId = 0;

            // Reset Combos
            if (cmbCompetitonSeasonTeam_CompetitonSeason.Items.Count > 0) cmbCompetitonSeasonTeam_CompetitonSeason.SelectedIndex = 0;
            if (cmbCompetitionSeasonTeam_Team.Items.Count > 0) cmbCompetitionSeasonTeam_Team.SelectedIndex = 0;

            // Reset Controls
            numFinalPosition.Value = 0;
            txtOverallStatus.Clear();

            dgvCompetitionSeasonTeams.ClearSelection();
        }

        private void dgvCompetitionSeasonTeams_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCompetitionSeasonTeams.Rows[e.RowIndex];

                _selectedCompSeasonTeamId = Convert.ToInt32(row.Cells["competition_season_team_id"].Value);

                // Map Combos
                if (row.Cells["competition_season_id"].Value != DBNull.Value)
                    cmbCompetitonSeasonTeam_CompetitonSeason.SelectedValue = Convert.ToInt32(row.Cells["competition_season_id"].Value);

                if (row.Cells["team_id"].Value != DBNull.Value)
                    cmbCompetitionSeasonTeam_Team.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                // Map Numeric
                if (row.Cells["final_position"].Value != DBNull.Value)
                    numFinalPosition.Value = Convert.ToDecimal(row.Cells["final_position"].Value);
                else
                    numFinalPosition.Value = 0;

                // Map TextBox
                txtOverallStatus.Text = row.Cells["overall_status"].Value?.ToString() ?? "";
            }
        }
    }
}
