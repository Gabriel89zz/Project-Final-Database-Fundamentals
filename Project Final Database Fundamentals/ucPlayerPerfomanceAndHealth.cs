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
    public partial class ucPlayerPerfomanceAndHealth : UserControl
    {
        private readonly int _adminUserId;
        private int _selectedInjuryId = 0;
        private int _selectedInjuryTypeId = 0;
        private int _selectedPlayerAvailabilityId = 0;
        private int _selectedPlayerSeasonStatId = 0;
        private int _selectedPlayerAwardId = 0;
        private int _selectedScoutingReportId = 0;
        private int _selectedTransferHistoryId = 0;
        public ucPlayerPerfomanceAndHealth(int adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
        }

        private async void tabControlPlayerPerfomanceAndHealth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPageInjury)
            {
                // Parallel Loading
                var tGrid = LoadInjuriesAsync();
                var tPlayer = LoadPlayersForInjuryComboAsync();
                var tType = LoadInjuryTypesForComboAsync();
                var tMatch = LoadMatchesForInjuryComboAsync();

                await Task.WhenAll(tGrid, tPlayer, tType, tMatch);
            }
            else if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPageInjuryType)
            {
                InitializeSeverityCombo();

                await LoadInjuryTypesAsync();
            }
            else if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPagePlayerAvailability)
            {
                // 1. Init Fixed Status (Sync)
                InitializeAvailabilityStatusCombo();

                // 2. Parallel Loading
                var tGrid = LoadPlayerAvailabilityAsync();
                var tPlayer = LoadPlayersForAvailabilityComboAsync();
                var tMatch = LoadMatchesForAvailabilityComboAsync();

                await Task.WhenAll(tGrid, tPlayer, tMatch);
            }
            else if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPagePlayerSeasonStat)
            {
                // Parallel Loading (Grid + 3 Combos)
                var tGrid = LoadPlayerSeasonStatsAsync();
                var tPlayer = LoadPlayersForStatComboAsync();
                var tSeason = LoadCompSeasonsForStatComboAsync();
                var tTeam = LoadTeamsForStatComboAsync();

                await Task.WhenAll(tGrid, tPlayer, tSeason, tTeam);
            }
            else if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPagePlayerAward)
            {
                // Parallel Loading (Grid + 3 Combos)
                var tGrid = LoadPlayerAwardsAsync();
                var tAward = LoadAwardsForPlayerAwardComboAsync();
                var tPlayer = LoadPlayersForPlayerAwardComboAsync();
                var tSeason = LoadSeasonsForPlayerAwardComboAsync();

                await Task.WhenAll(tGrid, tAward, tPlayer, tSeason);
            }
            else if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPageScoutingReport)
            {
                // Parallel Loading (Grid + 3 Combos)
                var tGrid = LoadScoutingReportsAsync();
                var tScout = LoadScoutsForReportComboAsync();
                var tPlayer = LoadPlayersForReportComboAsync();
                var tMatch = LoadMatchesForReportComboAsync();

                await Task.WhenAll(tGrid, tScout, tPlayer, tMatch);
            }
            else if (tabControlPlayerPerfomanceAndHealth.SelectedTab == tabPageTransferHistory)
            {
                // 1. Init Fixed Type (Sync)
                InitializeTransferTypeCombo();

                // 2. Parallel Loading
                var tGrid = LoadTransferHistoryAsync();
                var tPlayer = LoadPlayersForTransferComboAsync();
                var tTeams = LoadTeamsForTransferCombosAsync(); // Loads both From/To

                await Task.WhenAll(tGrid, tPlayer, tTeams);
            }
        }







        // 1. Load Players (Full Name)
        private async Task LoadPlayersForInjuryComboAsync()
        {
            string query = @"
        SELECT 
            player_id, 
            first_name || ' ' || last_name AS full_name 
        FROM player 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["player_id"] = -1;
                        row["full_name"] = "-- Select Player --";
                        dt.Rows.InsertAt(row, 0);

                        cmbInjuryPlayer.DataSource = dt;
                        cmbInjuryPlayer.DisplayMember = "full_name";
                        cmbInjuryPlayer.ValueMember = "player_id";
                        cmbInjuryPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 2. Load Injury Types (Assuming table 'injury_type')
        private async Task LoadInjuryTypesForComboAsync()
        {
            string query = @"SELECT injury_type_id, name FROM injury_type WHERE is_active = true ORDER BY name";

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
                        row["injury_type_id"] = -1;
                        row["name"] = "-- Select Type --";
                        dt.Rows.InsertAt(row, 0);

                        cmbInjuryType.DataSource = dt;
                        cmbInjuryType.DisplayMember = "name";
                        cmbInjuryType.ValueMember = "injury_type_id";
                        cmbInjuryType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading injury types: " + ex.Message); }
        }

        // 3. Load Matches (Context: "Team A vs Team B - Date")
        private async Task LoadMatchesForInjuryComboAsync()
        {
            string query = @"
        SELECT 
            m.match_id, 
            t1.name || ' vs ' || t2.name || ' (' || to_char(m.match_date, 'YYYY-MM-DD') || ')' AS display_name
        FROM ""match"" m
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE m.is_active = true
        ORDER BY m.match_date DESC";

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
                        row["match_id"] = -1;
                        row["display_name"] = "-- Select Match (Optional) --";
                        dt.Rows.InsertAt(row, 0);

                        cmbInjuryMatch.DataSource = dt;
                        cmbInjuryMatch.DisplayMember = "display_name";
                        cmbInjuryMatch.ValueMember = "match_id";
                        cmbInjuryMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadInjuriesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // Note: match_id_incurred is likely nullable, so we use LEFT JOIN for match info
            string query = @"
        SELECT 
            i.injury_id, 
            i.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            i.injury_type_id,
            it.name AS injury_type,
            i.match_id_incurred,
            (t1.name || ' vs ' || t2.name) AS match_context,
            i.date_incurred,
            i.expected_return_date,
            i.actual_return_date
        FROM injury i
        INNER JOIN player p ON i.player_id = p.player_id
        INNER JOIN injury_type it ON i.injury_type_id = it.injury_type_id
        LEFT JOIN ""match"" m ON i.match_id_incurred = m.match_id
        LEFT JOIN team t1 ON m.home_team_id = t1.team_id
        LEFT JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE i.is_active = true
        ORDER BY i.date_incurred DESC";

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

                        dgvInjury.DataSource = dt;

                        // NOTE: injury_id is VISIBLE.
                        // Hiding only foreign keys.
                        string[] hiddenCols = { "player_id", "injury_type_id", "match_id_incurred" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvInjury.Columns[col] != null)
                                dgvInjury.Columns[col].Visible = false;
                        }

                        // Format Dates
                        string[] dateCols = { "date_incurred", "expected_return_date", "actual_return_date" };
                        foreach (var col in dateCols)
                        {
                            if (dgvInjury.Columns[col] != null)
                                dgvInjury.Columns[col].DefaultCellStyle.Format = "d";
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading injuries: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddInjury_Click(object sender, EventArgs e)
        {
            // 1. Validate Required Combos
            if ((int)cmbInjuryPlayer.SelectedValue == -1 ||
                (int)cmbInjuryType.SelectedValue == -1)
            {
                MessageBox.Show("Player and Injury Type are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int playerId = (int)cmbInjuryPlayer.SelectedValue;
            int typeId = (int)cmbInjuryType.SelectedValue;
            DateTime dateIncurred = dtpInjuryDateIncurred.Value;
            DateTime expectedReturn = dtpInjuryExpectedReturnDate.Value;

            // Handle Optional Match
            object matchId = DBNull.Value;
            if (cmbInjuryMatch.SelectedValue != null && (int)cmbInjuryMatch.SelectedValue != -1)
            {
                matchId = (int)cmbInjuryMatch.SelectedValue;
            }

            // Note: handling Actual Return Date is tricky if it can be null.
            // Here we assume if it's set to today/default it might be null, 
            // but typically standard DTPs always send a value.
            // For this example, we send the value from the picker.
            DateTime actualReturn = dtpInjuryActualReturnDate.Value;

            string query = @"
        INSERT INTO injury 
        (player_id, injury_type_id, date_incurred, expected_return_date, actual_return_date, match_id_incurred, created_by) 
        VALUES 
        (@playerId, @typeId, @incurred, @expected, @actual, @matchId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@typeId", typeId);
                    command.Parameters.AddWithValue("@incurred", dateIncurred);
                    command.Parameters.AddWithValue("@expected", expectedReturn);
                    command.Parameters.AddWithValue("@actual", actualReturn); // Or DBNull.Value if logic dictates
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Injury record added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadInjuriesAsync();
                        btnClearInjury_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateInjury_Click(object sender, EventArgs e)
        {
            if (_selectedInjuryId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbInjuryPlayer.SelectedValue == -1 || (int)cmbInjuryType.SelectedValue == -1)
            {
                MessageBox.Show("Player and Type are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object matchId = DBNull.Value;
            if (cmbInjuryMatch.SelectedValue != null && (int)cmbInjuryMatch.SelectedValue != -1)
            {
                matchId = (int)cmbInjuryMatch.SelectedValue;
            }

            string query = @"
        UPDATE injury SET 
            player_id = @playerId, 
            injury_type_id = @typeId, 
            date_incurred = @incurred, 
            expected_return_date = @expected,
            actual_return_date = @actual,
            match_id_incurred = @matchId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            injury_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", (int)cmbInjuryPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@typeId", (int)cmbInjuryType.SelectedValue);
                    command.Parameters.AddWithValue("@incurred", dtpInjuryDateIncurred.Value);
                    command.Parameters.AddWithValue("@expected", dtpInjuryExpectedReturnDate.Value);
                    command.Parameters.AddWithValue("@actual", dtpInjuryActualReturnDate.Value);
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedInjuryId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Injury record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadInjuriesAsync();
                        btnClearInjury_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteInjury_Click(object sender, EventArgs e)
        {
            if (_selectedInjuryId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE injury SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                injury_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedInjuryId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Injury record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadInjuriesAsync();
                            btnClearInjury_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvInjury_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvInjury.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["injury_id"].Value == null || row.Cells["injury_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedInjuryId = Convert.ToInt32(row.Cells["injury_id"].Value);

                    // Map Combos
                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbInjuryPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    if (row.Cells["injury_type_id"].Value != null && row.Cells["injury_type_id"].Value != DBNull.Value)
                        cmbInjuryType.SelectedValue = Convert.ToInt32(row.Cells["injury_type_id"].Value);

                    if (row.Cells["match_id_incurred"].Value != null && row.Cells["match_id_incurred"].Value != DBNull.Value)
                        cmbInjuryMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id_incurred"].Value);
                    else
                        cmbInjuryMatch.SelectedIndex = 0; // Default to "Select"

                    // Map Dates (Safe)
                    if (row.Cells["date_ocurred"].Value != null && row.Cells["date_ocurred"].Value != DBNull.Value)
                        dtpInjuryDateIncurred.Value = Convert.ToDateTime(row.Cells["date_ocurred"].Value);

                    if (row.Cells["expected_return_date"].Value != null && row.Cells["expected_return_date"].Value != DBNull.Value)
                        dtpInjuryExpectedReturnDate.Value = Convert.ToDateTime(row.Cells["expected_return_date"].Value);

                    if (row.Cells["actual_return_date"].Value != null && row.Cells["actual_return_date"].Value != DBNull.Value)
                        dtpInjuryActualReturnDate.Value = Convert.ToDateTime(row.Cells["actual_return_date"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearInjury_Click(object sender, EventArgs e)
        {
            _selectedInjuryId = 0;

            // Reset Dates
            dtpInjuryActualReturnDate.Value = DateTime.Now;
            dtpInjuryExpectedReturnDate.Value = DateTime.Now;
            dtpInjuryActualReturnDate.Value = DateTime.Now;

            // Reset Combos
            if (cmbInjuryPlayer.Items.Count > 0) cmbInjuryPlayer.SelectedIndex = 0;
            if (cmbInjuryType.Items.Count > 0) cmbInjuryType.SelectedIndex = 0;
            if (cmbInjuryMatch.Items.Count > 0) cmbInjuryMatch.SelectedIndex = 0;

            dgvInjury.ClearSelection();
        }






        // 1. Initialize Severity Level ComboBox (Fixed Options)
        private void InitializeSeverityCombo()
        {
            cmbInjuryTypeSeverity.Items.Clear();
            cmbInjuryTypeSeverity.Items.Add("Low");
            cmbInjuryTypeSeverity.Items.Add("Medium");
            cmbInjuryTypeSeverity.Items.Add("High");
            cmbInjuryTypeSeverity.Items.Add("Critical");

            cmbInjuryTypeSeverity.SelectedIndex = 0;
        }

        // 2. Load Main Grid (ID VISIBLE)
        private async Task LoadInjuryTypesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            injury_type_id, 
            name, 
            severity_level 
        FROM injury_type
        WHERE is_active = true
        ORDER BY injury_type_id";

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

                        dgvInjuryType.DataSource = dt;

                        // NOTE: We do NOT hide "injury_type_id" as requested.
                        // It will be visible as the first column.
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading injury types: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private async void btnAddInjuryType_Click(object sender, EventArgs e)
        {
            string name = txtInjuryTypeName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbInjuryTypeSeverity.SelectedItem == null)
            {
                MessageBox.Show("Severity Level is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string severity = cmbInjuryTypeSeverity.SelectedItem.ToString();

            string query = @"
        INSERT INTO injury_type (name, severity_level, created_by) 
        VALUES (@name, @severity, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@severity", severity);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Injury Type added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadInjuryTypesAsync();
                        btnClearInjuryType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Injury Type already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateInjuryType_Click(object sender, EventArgs e)
        {
            if (_selectedInjuryTypeId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtInjuryTypeName.Text.Trim();

            if (string.IsNullOrEmpty(name) || cmbInjuryTypeSeverity.SelectedItem == null)
            {
                MessageBox.Show("Name and Severity are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string severity = cmbInjuryTypeSeverity.SelectedItem.ToString();

            string query = @"
        UPDATE injury_type SET 
            name = @name, 
            severity_level = @severity,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            injury_type_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@severity", severity);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedInjuryTypeId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Injury Type updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadInjuryTypesAsync();
                        btnClearInjuryType_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteInjuryType_Click(object sender, EventArgs e)
        {
            if (_selectedInjuryTypeId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE injury_type SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                injury_type_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedInjuryTypeId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadInjuryTypesAsync();
                            btnClearInjuryType_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvInjuryType_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvInjuryType.Rows[e.RowIndex];

                    // 1. Check New Row
                    if (row.IsNewRow) return;

                    // 2. Check Null ID
                    if (row.Cells["injury_type_id"].Value == null || row.Cells["injury_type_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedInjuryTypeId = Convert.ToInt32(row.Cells["injury_type_id"].Value);

                    // Map Name
                    txtInjuryTypeName.Text = row.Cells["name"].Value?.ToString() ?? "";

                    // Map Severity
                    if (row.Cells["severity_level"].Value != null && row.Cells["severity_level"].Value != DBNull.Value)
                    {
                        string severity = row.Cells["severity_level"].Value.ToString();
                        if (cmbInjuryTypeSeverity.Items.Contains(severity))
                        {
                            cmbInjuryTypeSeverity.SelectedItem = severity;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearInjuryType_Click(object sender, EventArgs e)
        {
            _selectedInjuryTypeId = 0;
            txtInjuryTypeName.Clear();

            if (cmbInjuryTypeSeverity.Items.Count > 0)
                cmbInjuryTypeSeverity.SelectedIndex = 0;

            dgvInjuryType.ClearSelection();
        }






        // 1. Initialize Status ComboBox (Fixed Options)
        private void InitializeAvailabilityStatusCombo()
        {
            cmbPlayerAvailabilityStatus.Items.Clear();
            cmbPlayerAvailabilityStatus.Items.Add("Available");
            cmbPlayerAvailabilityStatus.Items.Add("Injured");
            cmbPlayerAvailabilityStatus.Items.Add("Suspended");
            cmbPlayerAvailabilityStatus.Items.Add("International Duty");
            cmbPlayerAvailabilityStatus.Items.Add("Personal Reasons");
            cmbPlayerAvailabilityStatus.Items.Add("Rested");

            cmbPlayerAvailabilityStatus.SelectedIndex = 0;
        }

        // 2. Load Players
        private async Task LoadPlayersForAvailabilityComboAsync()
        {
            string query = @"
        SELECT 
            player_id, 
            first_name || ' ' || last_name AS full_name 
        FROM player 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["player_id"] = -1;
                        row["full_name"] = "-- Select Player --";
                        dt.Rows.InsertAt(row, 0);

                        cmbPlayerAvailabilityPlayer.DataSource = dt;
                        cmbPlayerAvailabilityPlayer.DisplayMember = "full_name";
                        cmbPlayerAvailabilityPlayer.ValueMember = "player_id";
                        cmbPlayerAvailabilityPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 3. Load Matches (Context: Team vs Team)
        private async Task LoadMatchesForAvailabilityComboAsync()
        {
            string query = @"
        SELECT 
            m.match_id, 
            t1.name || ' vs ' || t2.name || ' (' || to_char(m.match_date, 'YYYY-MM-DD') || ')' AS display_name
        FROM ""match"" m
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE m.is_active = true
        ORDER BY m.match_date DESC";

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
                        row["match_id"] = -1;
                        row["display_name"] = "-- Select Match --";
                        dt.Rows.InsertAt(row, 0);

                        cmbPlayerAvailabilityMatch.DataSource = dt;
                        cmbPlayerAvailabilityMatch.DisplayMember = "display_name";
                        cmbPlayerAvailabilityMatch.ValueMember = "match_id";
                        cmbPlayerAvailabilityMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadPlayerAvailabilityAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            pa.availability_id, 
            pa.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            pa.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            pa.status,
            pa.reason
        FROM player_availability pa
        INNER JOIN player p ON pa.player_id = p.player_id
        INNER JOIN ""match"" m ON pa.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE pa.is_active = true
        ORDER BY m.match_date DESC, p.last_name";

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

                        dgvPlayerAvailability.DataSource = dt;

                        // NOTE: availability_id is VISIBLE.
                        // We only hide the Foreign Keys.
                        string[] hiddenCols = { "player_id", "match_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvPlayerAvailability.Columns[col] != null)
                                dgvPlayerAvailability.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading availability data: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddPlayerAvailability_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbPlayerAvailabilityPlayer.SelectedValue == -1 ||
                (int)cmbPlayerAvailabilityMatch.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Player and a Match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbPlayerAvailabilityStatus.SelectedItem == null)
            {
                MessageBox.Show("Please select a Status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int playerId = (int)cmbPlayerAvailabilityPlayer.SelectedValue;
            int matchId = (int)cmbPlayerAvailabilityMatch.SelectedValue;
            string status = cmbPlayerAvailabilityStatus.SelectedItem.ToString();
            string reason = txtPlayerAvailabilityReason.Text.Trim();

            string query = @"
        INSERT INTO player_availability 
        (player_id, match_id, status, reason, created_by) 
        VALUES 
        (@playerId, @matchId, @status, @reason, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@reason", reason);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Availability record added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayerAvailabilityAsync();
                        btnClearPlayerAvailability_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("A record for this player and match already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdatePlayerAvailability_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerAvailabilityId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbPlayerAvailabilityPlayer.SelectedValue == -1 ||
                (int)cmbPlayerAvailabilityMatch.SelectedValue == -1 ||
                cmbPlayerAvailabilityStatus.SelectedItem == null)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE player_availability SET 
            player_id = @playerId, 
            match_id = @matchId, 
            status = @status, 
            reason = @reason,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            availability_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", (int)cmbPlayerAvailabilityPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@matchId", (int)cmbPlayerAvailabilityMatch.SelectedValue);
                    command.Parameters.AddWithValue("@status", cmbPlayerAvailabilityStatus.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@reason", txtPlayerAvailabilityReason.Text.Trim());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedPlayerAvailabilityId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayerAvailabilityAsync();
                        btnClearPlayerAvailability_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeletePlayerAvailability_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerAvailabilityId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE player_availability SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                availability_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedPlayerAvailabilityId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadPlayerAvailabilityAsync();
                            btnClearPlayerAvailability_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvPlayerAvailability_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvPlayerAvailability.Rows[e.RowIndex];

                    // 1. Ignore New Row
                    if (row.IsNewRow) return;

                    // 2. Check Null ID
                    if (row.Cells["availability_id"].Value == null ||
                        row.Cells["availability_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedPlayerAvailabilityId = Convert.ToInt32(row.Cells["availability_id"].Value);

                    // Map Combos (Safe check)
                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbPlayerAvailabilityPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbPlayerAvailabilityMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    // Map Status (String)
                    if (row.Cells["status"].Value != null && row.Cells["status"].Value != DBNull.Value)
                    {
                        string status = row.Cells["status"].Value.ToString();
                        if (cmbPlayerAvailabilityStatus.Items.Contains(status))
                            cmbPlayerAvailabilityStatus.SelectedItem = status;
                    }

                    // Map Reason
                    txtPlayerAvailabilityReason.Text = row.Cells["reason"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearPlayerAvailability_Click(object sender, EventArgs e)
        {
            _selectedPlayerAvailabilityId = 0;
            txtPlayerAvailabilityReason.Clear();

            if (cmbPlayerAvailabilityPlayer.Items.Count > 0) cmbPlayerAvailabilityPlayer.SelectedIndex = 0;
            if (cmbPlayerAvailabilityMatch.Items.Count > 0) cmbPlayerAvailabilityMatch.SelectedIndex = 0;
            if (cmbPlayerAvailabilityStatus.Items.Count > 0) cmbPlayerAvailabilityStatus.SelectedIndex = 0;

            dgvPlayerAvailability.ClearSelection();
        }






        // 1. Load Players (Full Name)
        private async Task LoadPlayersForStatComboAsync()
        {
            string query = @"
        SELECT 
            player_id, 
            first_name || ' ' || last_name AS full_name 
        FROM player 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["player_id"] = -1;
                        row["full_name"] = "-- Select Player --";
                        dt.Rows.InsertAt(row, 0);

                        cmbPlayerSeasonStatPlayer.DataSource = dt;
                        cmbPlayerSeasonStatPlayer.DisplayMember = "full_name";
                        cmbPlayerSeasonStatPlayer.ValueMember = "player_id";
                        cmbPlayerSeasonStatPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 2. Load Competition Seasons (Formatted)
        private async Task LoadCompSeasonsForStatComboAsync()
        {
            string query = @"
        SELECT 
            cs.competition_season_id, 
            c.name || ' - ' || s.name AS display_name
        FROM competition_season cs
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        WHERE cs.is_active = true
        ORDER BY c.name, s.name DESC";

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

                        cmbPlayerSeasonStatCompSeason.DataSource = dt;
                        cmbPlayerSeasonStatCompSeason.DisplayMember = "display_name";
                        cmbPlayerSeasonStatCompSeason.ValueMember = "competition_season_id";
                        cmbPlayerSeasonStatCompSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 3. Load Teams
        private async Task LoadTeamsForStatComboAsync()
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

                        cmbPlayerSeasonStatTeam.DataSource = dt;
                        cmbPlayerSeasonStatTeam.DisplayMember = "name";
                        cmbPlayerSeasonStatTeam.ValueMember = "team_id";
                        cmbPlayerSeasonStatTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Main Grid 
        private async Task LoadPlayerSeasonStatsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            pss.player_season_stat_id,
            pss.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            pss.competition_season_id,
            (c.name || ' - ' || s.name) AS comp_season_display,
            pss.team_id,
            t.name AS team_name,
            pss.matches_played,
            pss.minutes_played,
            pss.goals,
            pss.assists,
            pss.yellow_cards,
            pss.red_cards,
            pss.shots_on_target
        FROM player_season_stat pss
        INNER JOIN player p ON pss.player_id = p.player_id
        INNER JOIN competition_season cs ON pss.competition_season_id = cs.competition_season_id
        INNER JOIN competition c ON cs.competition_id = c.competition_id
        INNER JOIN season s ON cs.season_id = s.season_id
        INNER JOIN team t ON pss.team_id = t.team_id
        WHERE pss.is_active = true
        ORDER BY p.last_name, c.name";

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

                        dgvPlayerSeasonStat.DataSource = dt;

                        // NOTE: player_season_stat_id is VISIBLE.
                        // Hide only Foreign Keys.
                        string[] hiddenCols = { "player_id", "competition_season_id", "team_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvPlayerSeasonStat.Columns[col] != null)
                                dgvPlayerSeasonStat.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stats: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddPlayerSeasonStat_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbPlayerSeasonStatPlayer.SelectedValue == -1 ||
                (int)cmbPlayerSeasonStatCompSeason.SelectedValue == -1 ||
                (int)cmbPlayerSeasonStatTeam.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Player, Season, and Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int playerId = (int)cmbPlayerSeasonStatPlayer.SelectedValue;
            int compSeasonId = (int)cmbPlayerSeasonStatCompSeason.SelectedValue;
            int teamId = (int)cmbPlayerSeasonStatTeam.SelectedValue;

            // 3. Get Stats
            int matches = (int)numPlayerSeasonStatMatches.Value;
            int minutes = (int)numPlayerSeasonStatMinutes.Value;
            int goals = (int)numPlayerSeasonStatGoals.Value;
            int assists = (int)numPlayerSeasonStatAssists.Value;
            int yellow = (int)numPlayerSeasonStatYellowCards.Value;
            int red = (int)numPlayerSeasonStatRedCards.Value;
            int shots = (int)numPlayerSeasonStatShotsOnTarget.Value;

            string query = @"
        INSERT INTO player_season_stat 
        (player_id, competition_season_id, team_id, matches_played, minutes_played, goals, assists, yellow_cards, red_cards, shots_on_target, created_by) 
        VALUES 
        (@playerId, @csId, @teamId, @matches, @mins, @goals, @assists, @yellow, @red, @shots, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@csId", compSeasonId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@matches", matches);
                    command.Parameters.AddWithValue("@mins", minutes);
                    command.Parameters.AddWithValue("@goals", goals);
                    command.Parameters.AddWithValue("@assists", assists);
                    command.Parameters.AddWithValue("@yellow", yellow);
                    command.Parameters.AddWithValue("@red", red);
                    command.Parameters.AddWithValue("@shots", shots);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stats added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayerSeasonStatsAsync();
                        btnClearPlayerSeasonStat_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("Stats for this player in this season/team already exist.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdatePlayerSeasonStat_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerSeasonStatId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbPlayerSeasonStatPlayer.SelectedValue == -1 ||
                (int)cmbPlayerSeasonStatCompSeason.SelectedValue == -1 ||
                (int)cmbPlayerSeasonStatTeam.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE player_season_stat SET 
            player_id = @playerId, 
            competition_season_id = @csId, 
            team_id = @teamId, 
            matches_played = @matches, 
            minutes_played = @mins, 
            goals = @goals, 
            assists = @assists, 
            yellow_cards = @yellow, 
            red_cards = @red, 
            shots_on_target = @shots,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            player_season_stat_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", (int)cmbPlayerSeasonStatPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@csId", (int)cmbPlayerSeasonStatCompSeason.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbPlayerSeasonStatTeam.SelectedValue);
                    command.Parameters.AddWithValue("@matches", (int)numPlayerSeasonStatMatches.Value);
                    command.Parameters.AddWithValue("@mins", (int)numPlayerSeasonStatMinutes.Value);
                    command.Parameters.AddWithValue("@goals", (int)numPlayerSeasonStatGoals.Value);
                    command.Parameters.AddWithValue("@assists", (int)numPlayerSeasonStatAssists.Value);
                    command.Parameters.AddWithValue("@yellow", (int)numPlayerSeasonStatYellowCards.Value);
                    command.Parameters.AddWithValue("@red", (int)numPlayerSeasonStatRedCards.Value);
                    command.Parameters.AddWithValue("@shots", (int)numPlayerSeasonStatShotsOnTarget.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedPlayerSeasonStatId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stats updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayerSeasonStatsAsync();
                        btnClearPlayerSeasonStat_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating stats: " + ex.Message); }
        }

        private async void btnDeletePlayerSeasonStat_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerSeasonStatId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE player_season_stat SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                player_season_stat_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedPlayerSeasonStatId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadPlayerSeasonStatsAsync();
                            btnClearPlayerSeasonStat_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvPlayerSeasonStat_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvPlayerSeasonStat.Rows[e.RowIndex];

                    // Ignore New Row
                    if (row.IsNewRow) return;

                    // Check Null ID
                    if (row.Cells["player_season_stat_id"].Value == null ||
                        row.Cells["player_season_stat_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedPlayerSeasonStatId = Convert.ToInt32(row.Cells["player_season_stat_id"].Value);

                    // Map Combos
                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbPlayerSeasonStatPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    if (row.Cells["competition_season_id"].Value != null && row.Cells["competition_season_id"].Value != DBNull.Value)
                        cmbPlayerSeasonStatCompSeason.SelectedValue = Convert.ToInt32(row.Cells["competition_season_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbPlayerSeasonStatTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    // Map Numerics (Safe Check)
                    numPlayerSeasonStatMatches.Value = Convert.ToDecimal(row.Cells["matches_played"].Value ?? 0);
                    numPlayerSeasonStatMinutes.Value = Convert.ToDecimal(row.Cells["minutes_played"].Value ?? 0);
                    numPlayerSeasonStatGoals.Value = Convert.ToDecimal(row.Cells["goals"].Value ?? 0);
                    numPlayerSeasonStatAssists.Value = Convert.ToDecimal(row.Cells["assists"].Value ?? 0);
                    numPlayerSeasonStatYellowCards.Value = Convert.ToDecimal(row.Cells["yellow_cards"].Value ?? 0);
                    numPlayerSeasonStatRedCards.Value = Convert.ToDecimal(row.Cells["red_cards"].Value ?? 0);
                    numPlayerSeasonStatShotsOnTarget.Value = Convert.ToDecimal(row.Cells["shots_on_target"].Value ?? 0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting stats: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearPlayerSeasonStat_Click(object sender, EventArgs e)
        {
            _selectedPlayerSeasonStatId = 0;

            // Reset Numerics
            numPlayerSeasonStatMatches.Value = 0;
            numPlayerSeasonStatMinutes.Value = 0;
            numPlayerSeasonStatGoals.Value = 0;
            numPlayerSeasonStatAssists.Value = 0;
            numPlayerSeasonStatYellowCards.Value = 0;
            numPlayerSeasonStatRedCards.Value = 0;
            numPlayerSeasonStatShotsOnTarget.Value = 0;

            // Reset Combos
            if (cmbPlayerSeasonStatPlayer.Items.Count > 0) cmbPlayerSeasonStatPlayer.SelectedIndex = 0;
            if (cmbPlayerSeasonStatCompSeason.Items.Count > 0) cmbPlayerSeasonStatCompSeason.SelectedIndex = 0;
            if (cmbPlayerSeasonStatTeam.Items.Count > 0) cmbPlayerSeasonStatTeam.SelectedIndex = 0;

            dgvPlayerSeasonStat.ClearSelection();
        }






        // 1. Load Awards
        private async Task LoadAwardsForPlayerAwardComboAsync()
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

                        cmbPlayerAwardAward.DataSource = dt;
                        cmbPlayerAwardAward.DisplayMember = "name";
                        cmbPlayerAwardAward.ValueMember = "award_id";
                        cmbPlayerAwardAward.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading awards: " + ex.Message); }
        }

        // 2. Load Players (Full Name)
        private async Task LoadPlayersForPlayerAwardComboAsync()
        {
            string query = @"
        SELECT 
            player_id, 
            first_name || ' ' || last_name AS full_name 
        FROM player 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["player_id"] = -1;
                        row["full_name"] = "-- Select Player --";
                        dt.Rows.InsertAt(row, 0);

                        cmbPlayerAwardPlayer.DataSource = dt;
                        cmbPlayerAwardPlayer.DisplayMember = "full_name";
                        cmbPlayerAwardPlayer.ValueMember = "player_id";
                        cmbPlayerAwardPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 3. Load Seasons
        private async Task LoadSeasonsForPlayerAwardComboAsync()
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

                        cmbPlayerAwardSeason.DataSource = dt;
                        cmbPlayerAwardSeason.DisplayMember = "name";
                        cmbPlayerAwardSeason.ValueMember = "season_id";
                        cmbPlayerAwardSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadPlayerAwardsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            pa.player_award_id, 
            pa.award_id,
            a.name AS award_name,
            pa.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            pa.season_id,
            s.name AS season_name
        FROM player_award pa
        INNER JOIN award a ON pa.award_id = a.award_id
        INNER JOIN player p ON pa.player_id = p.player_id
        INNER JOIN season s ON pa.season_id = s.season_id
        WHERE pa.is_active = true
        ORDER BY s.name DESC, p.last_name";

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

                        dgvPlayerAward.DataSource = dt;

                        // NOTE: player_award_id is VISIBLE.
                        // We only hide the Foreign Keys to keep the view clean but informative.
                        string[] hiddenCols = { "award_id", "player_id", "season_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvPlayerAward.Columns[col] != null)
                                dgvPlayerAward.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading player awards: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddPlayerAward_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbPlayerAwardAward.SelectedValue == -1 ||
                (int)cmbPlayerAwardPlayer.SelectedValue == -1 ||
                (int)cmbPlayerAwardSeason.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int awardId = (int)cmbPlayerAwardAward.SelectedValue;
            int playerId = (int)cmbPlayerAwardPlayer.SelectedValue;
            int seasonId = (int)cmbPlayerAwardSeason.SelectedValue;

            string query = @"
        INSERT INTO player_award 
        (award_id, player_id, season_id, created_by) 
        VALUES 
        (@awardId, @playerId, @seasonId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@awardId", awardId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@seasonId", seasonId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Player Award added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayerAwardsAsync();
                        btnClearPlayerAward_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Player already has this Award for this Season.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdatePlayerAward_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerAwardId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbPlayerAwardAward.SelectedValue == -1 ||
                (int)cmbPlayerAwardPlayer.SelectedValue == -1 ||
                (int)cmbPlayerAwardSeason.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE player_award SET 
            award_id = @awardId, 
            player_id = @playerId, 
            season_id = @seasonId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            player_award_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@awardId", (int)cmbPlayerAwardAward.SelectedValue);
                    command.Parameters.AddWithValue("@playerId", (int)cmbPlayerAwardPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@seasonId", (int)cmbPlayerAwardSeason.SelectedValue);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedPlayerAwardId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPlayerAwardsAsync();
                        btnClearPlayerAward_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeletePlayerAward_Click(object sender, EventArgs e)
        {
            if (_selectedPlayerAwardId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE player_award SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                player_award_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedPlayerAwardId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadPlayerAwardsAsync();
                            btnClearPlayerAward_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvPlayerAward_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvPlayerAward.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["player_award_id"].Value == null || row.Cells["player_award_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedPlayerAwardId = Convert.ToInt32(row.Cells["player_award_id"].Value);

                    // Map Combos
                    if (row.Cells["award_id"].Value != null && row.Cells["award_id"].Value != DBNull.Value)
                        cmbPlayerAwardAward.SelectedValue = Convert.ToInt32(row.Cells["award_id"].Value);

                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbPlayerAwardPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    if (row.Cells["season_id"].Value != null && row.Cells["season_id"].Value != DBNull.Value)
                        cmbPlayerAwardSeason.SelectedValue = Convert.ToInt32(row.Cells["season_id"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearPlayerAward_Click(object sender, EventArgs e)
        {
            _selectedPlayerAwardId = 0;

            if (cmbPlayerAwardAward.Items.Count > 0) cmbPlayerAwardAward.SelectedIndex = 0;
            if (cmbPlayerAwardPlayer.Items.Count > 0) cmbPlayerAwardPlayer.SelectedIndex = 0;
            if (cmbPlayerAwardSeason.Items.Count > 0) cmbPlayerAwardSeason.SelectedIndex = 0;

            dgvPlayerAward.ClearSelection();
        }







        // 1. Load Scouts (Full Name)
        private async Task LoadScoutsForReportComboAsync()
        {
            string query = @"
        SELECT 
            scout_id, 
            first_name || ' ' || last_name AS full_name 
        FROM scout 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["scout_id"] = -1;
                        row["full_name"] = "-- Select Scout --";
                        dt.Rows.InsertAt(row, 0);

                        cmbScoutingReportScout.DataSource = dt;
                        cmbScoutingReportScout.DisplayMember = "full_name";
                        cmbScoutingReportScout.ValueMember = "scout_id";
                        cmbScoutingReportScout.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading scouts: " + ex.Message); }
        }

        // 2. Load Players (Full Name)
        private async Task LoadPlayersForReportComboAsync()
        {
            string query = @"
        SELECT 
            player_id, 
            first_name || ' ' || last_name AS full_name 
        FROM player 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["player_id"] = -1;
                        row["full_name"] = "-- Select Player --";
                        dt.Rows.InsertAt(row, 0);

                        cmbScoutingReportPlayer.DataSource = dt;
                        cmbScoutingReportPlayer.DisplayMember = "full_name";
                        cmbScoutingReportPlayer.ValueMember = "player_id";
                        cmbScoutingReportPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 3. Load Matches (Team vs Team)
        private async Task LoadMatchesForReportComboAsync()
        {
            string query = @"
        SELECT 
            m.match_id, 
            t1.name || ' vs ' || t2.name || ' (' || to_char(m.match_date, 'YYYY-MM-DD') || ')' AS display_name
        FROM ""match"" m
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE m.is_active = true
        ORDER BY m.match_date DESC";

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
                        row["match_id"] = -1;
                        row["display_name"] = "-- Select Match --";
                        dt.Rows.InsertAt(row, 0);

                        cmbScoutingReportMatch.DataSource = dt;
                        cmbScoutingReportMatch.DisplayMember = "display_name";
                        cmbScoutingReportMatch.ValueMember = "match_id";
                        cmbScoutingReportMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadScoutingReportsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            sr.report_id, 
            sr.scout_id,
            (s.first_name || ' ' || s.last_name) AS scout_name,
            sr.scouted_player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            sr.match_observed_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            sr.report_date,
            sr.overall_rating,
            sr.summary_text
        FROM scouting_report sr
        INNER JOIN scout s ON sr.scout_id = s.scout_id
        INNER JOIN player p ON sr.scouted_player_id = p.player_id
        INNER JOIN ""match"" m ON sr.match_observed_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE sr.is_active = true
        ORDER BY sr.report_date DESC";

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

                        dgvScoutingReport.DataSource = dt;

                        // NOTE: report_id is VISIBLE.
                        // Hide only foreign keys.
                        string[] hiddenCols = { "scout_id", "scouted_player_id", "match_observed_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvScoutingReport.Columns[col] != null)
                                dgvScoutingReport.Columns[col].Visible = false;
                        }

                        // Format Date
                        if (dgvScoutingReport.Columns["report_date"] != null)
                            dgvScoutingReport.Columns["report_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading reports: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddScoutingReport_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbScoutingReportScout.SelectedValue == -1 ||
                (int)cmbScoutingReportPlayer.SelectedValue == -1 ||
                (int)cmbScoutingReportMatch.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int scoutId = (int)cmbScoutingReportScout.SelectedValue;
            int playerId = (int)cmbScoutingReportPlayer.SelectedValue;
            int matchId = (int)cmbScoutingReportMatch.SelectedValue;
            DateTime reportDate = dtpScoutingReportDate.Value;
            decimal rating = numScoutingReportRating.Value;
            string summary = txtScoutingReportSummary.Text.Trim();

            string query = @"
        INSERT INTO scouting_report 
        (scout_id, scouted_player_id, match_observed_id, report_date, overall_rating, summary_text, created_by) 
        VALUES 
        (@scoutId, @playerId, @matchId, @date, @rating, @summary, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@scoutId", scoutId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@date", reportDate);
                    command.Parameters.AddWithValue("@rating", rating);
                    command.Parameters.AddWithValue("@summary", summary);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Report added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadScoutingReportsAsync();
                        btnClearScoutingReport_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateScoutingReport_Click(object sender, EventArgs e)
        {
            if (_selectedScoutingReportId == 0)
            {
                MessageBox.Show("Please select a report to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbScoutingReportScout.SelectedValue == -1 ||
                (int)cmbScoutingReportPlayer.SelectedValue == -1 ||
                (int)cmbScoutingReportMatch.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE scouting_report SET 
            scout_id = @scoutId, 
            scouted_player_id = @playerId, 
            match_observed_id = @matchId, 
            report_date = @date, 
            overall_rating = @rating, 
            summary_text = @summary,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            report_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@scoutId", (int)cmbScoutingReportScout.SelectedValue);
                    command.Parameters.AddWithValue("@playerId", (int)cmbScoutingReportPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@matchId", (int)cmbScoutingReportMatch.SelectedValue);
                    command.Parameters.AddWithValue("@date", dtpScoutingReportDate.Value);
                    command.Parameters.AddWithValue("@rating", numScoutingReportRating.Value);
                    command.Parameters.AddWithValue("@summary", txtScoutingReportSummary.Text.Trim());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedScoutingReportId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Report updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadScoutingReportsAsync();
                        btnClearScoutingReport_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating report: " + ex.Message); }
        }

        private async void btnDeleteScoutingReport_Click(object sender, EventArgs e)
        {
            if (_selectedScoutingReportId == 0)
            {
                MessageBox.Show("Please select a report to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE scouting_report SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                report_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedScoutingReportId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Report deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadScoutingReportsAsync();
                            btnClearScoutingReport_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting report: " + ex.Message); }
            }
        }

        private void dgvScoutingReport_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvScoutingReport.Rows[e.RowIndex];

                    // 1. Ignore New Row
                    if (row.IsNewRow) return;

                    // 2. Check Null ID
                    if (row.Cells["report_id"].Value == null ||
                        row.Cells["report_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedScoutingReportId = Convert.ToInt32(row.Cells["report_id"].Value);

                    // Map Combos
                    if (row.Cells["scout_id"].Value != null && row.Cells["scout_id"].Value != DBNull.Value)
                        cmbScoutingReportScout.SelectedValue = Convert.ToInt32(row.Cells["scout_id"].Value);

                    if (row.Cells["scouted_player_id"].Value != null && row.Cells["scouted_player_id"].Value != DBNull.Value)
                        cmbScoutingReportPlayer.SelectedValue = Convert.ToInt32(row.Cells["scouted_player_id"].Value);

                    if (row.Cells["match_observed_id"].Value != null && row.Cells["match_observed_id"].Value != DBNull.Value)
                        cmbScoutingReportMatch.SelectedValue = Convert.ToInt32(row.Cells["match_observed_id"].Value);

                    // Map Date
                    if (row.Cells["report_date"].Value != null && row.Cells["report_date"].Value != DBNull.Value)
                        dtpScoutingReportDate.Value = Convert.ToDateTime(row.Cells["report_date"].Value);

                    // Map Numeric
                    if (row.Cells["overall_rating"].Value != null && row.Cells["overall_rating"].Value != DBNull.Value)
                        numScoutingReportRating.Value = Convert.ToDecimal(row.Cells["overall_rating"].Value);

                    // Map Text
                    txtScoutingReportSummary.Text = row.Cells["summary_text"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearScoutingReport_Click(object sender, EventArgs e)
        {
            _selectedScoutingReportId = 0;
            numScoutingReportRating.Value = 0;
            txtScoutingReportSummary.Clear();
            dtpScoutingReportDate.Value = DateTime.Now;

            if (cmbScoutingReportScout.Items.Count > 0) cmbScoutingReportScout.SelectedIndex = 0;
            if (cmbScoutingReportPlayer.Items.Count > 0) cmbScoutingReportPlayer.SelectedIndex = 0;
            if (cmbScoutingReportMatch.Items.Count > 0) cmbScoutingReportMatch.SelectedIndex = 0;

            dgvScoutingReport.ClearSelection();
        }






        // 1. Initialize Transfer Type ComboBox (Fixed Options)
        private void InitializeTransferTypeCombo()
        {
            cmbTransferHistoryType.Items.Clear();
            cmbTransferHistoryType.Items.Add("Permanent");
            cmbTransferHistoryType.Items.Add("Loan");
            cmbTransferHistoryType.Items.Add("Free Transfer");
            cmbTransferHistoryType.Items.Add("Return from Loan");
            cmbTransferHistoryType.Items.Add("Swap");

            cmbTransferHistoryType.SelectedIndex = 0;
        }

        // 2. Load Players
        private async Task LoadPlayersForTransferComboAsync()
        {
            string query = @"
        SELECT 
            player_id, 
            first_name || ' ' || last_name AS full_name 
        FROM player 
        WHERE is_active = true 
        ORDER BY last_name";

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
                        row["player_id"] = -1;
                        row["full_name"] = "-- Select Player --";
                        dt.Rows.InsertAt(row, 0);

                        cmbTransferHistoryPlayer.DataSource = dt;
                        cmbTransferHistoryPlayer.DisplayMember = "full_name";
                        cmbTransferHistoryPlayer.ValueMember = "player_id";
                        cmbTransferHistoryPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 3. Load Teams (For 'From' and 'To' Combos)
        private async Task LoadTeamsForTransferCombosAsync()
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
                        // Create two separate tables to avoid binding conflicts
                        DataTable dtFrom = new DataTable();
                        dtFrom.Load(reader);

                        DataTable dtTo = dtFrom.Copy();

                        // Add Defaults
                        DataRow rowFrom = dtFrom.NewRow();
                        rowFrom["team_id"] = -1;
                        rowFrom["name"] = "-- From Team --";
                        dtFrom.Rows.InsertAt(rowFrom, 0);

                        DataRow rowTo = dtTo.NewRow();
                        rowTo["team_id"] = -1;
                        rowTo["name"] = "-- To Team --";
                        dtTo.Rows.InsertAt(rowTo, 0);

                        // Bind From Team
                        cmbTransferHistoryFromTeam.DataSource = dtFrom;
                        cmbTransferHistoryFromTeam.DisplayMember = "name";
                        cmbTransferHistoryFromTeam.ValueMember = "team_id";
                        cmbTransferHistoryFromTeam.SelectedIndex = 0;

                        // Bind To Team
                        cmbTransferHistoryToTeam.DataSource = dtTo;
                        cmbTransferHistoryToTeam.DisplayMember = "name";
                        cmbTransferHistoryToTeam.ValueMember = "team_id";
                        cmbTransferHistoryToTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadTransferHistoryAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            th.transfer_id, 
            th.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            th.transfer_date,
            th.from_team_id,
            t1.name AS from_team,
            th.to_team_id,
            t2.name AS to_team,
            th.transfer_fee_eur,
            th.transfer_type
        FROM transfer_history th
        INNER JOIN player p ON th.player_id = p.player_id
        LEFT JOIN team t1 ON th.from_team_id = t1.team_id
        INNER JOIN team t2 ON th.to_team_id = t2.team_id
        WHERE th.is_active = true
        ORDER BY th.transfer_date DESC";

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

                        dgvTransferHistory.DataSource = dt;

                        // NOTE: transfer_id is VISIBLE.
                        // Hide only Foreign Keys.
                        string[] hiddenCols = { "player_id", "from_team_id", "to_team_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTransferHistory.Columns[col] != null)
                                dgvTransferHistory.Columns[col].Visible = false;
                        }

                        // Format Currency and Date
                        if (dgvTransferHistory.Columns["transfer_fee_eur"] != null)
                            dgvTransferHistory.Columns["transfer_fee_eur"].DefaultCellStyle.Format = "C0"; // Currency no decimals

                        if (dgvTransferHistory.Columns["transfer_date"] != null)
                            dgvTransferHistory.Columns["transfer_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading transfers: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddTransfer_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbTransferHistoryPlayer.SelectedValue == -1 ||
                (int)cmbTransferHistoryToTeam.SelectedValue == -1)
            {
                MessageBox.Show("Player and Destination Team are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbTransferHistoryType.SelectedItem == null)
            {
                MessageBox.Show("Transfer Type is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int playerId = (int)cmbTransferHistoryPlayer.SelectedValue;
            int toTeamId = (int)cmbTransferHistoryToTeam.SelectedValue;
            DateTime date = dtpTransferHistoryDate.Value;
            decimal fee = numTransferHistoryFee.Value;
            string type = cmbTransferHistoryType.SelectedItem.ToString();

            // Handle optional "From Team" (Free agents might not have a 'from' team)
            object fromTeamId = DBNull.Value;
            if (cmbTransferHistoryFromTeam.SelectedValue != null && (int)cmbTransferHistoryFromTeam.SelectedValue != -1)
            {
                fromTeamId = (int)cmbTransferHistoryFromTeam.SelectedValue;

                // Logic Check: Cannot transfer to same team
                if ((int)fromTeamId == toTeamId)
                {
                    MessageBox.Show("Origin and Destination teams cannot be the same.", "Logic Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string query = @"
        INSERT INTO transfer_history 
        (player_id, transfer_date, from_team_id, to_team_id, transfer_fee_eur, transfer_type, created_by) 
        VALUES 
        (@playerId, @date, @fromId, @toId, @fee, @type, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@date", date);
                    command.Parameters.AddWithValue("@fromId", fromTeamId);
                    command.Parameters.AddWithValue("@toId", toTeamId);
                    command.Parameters.AddWithValue("@fee", fee);
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Transfer added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTransferHistoryAsync();
                        btnClearTransfer_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTransfer_Click(object sender, EventArgs e)
        {
            if (_selectedTransferHistoryId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbTransferHistoryPlayer.SelectedValue == -1 ||
                (int)cmbTransferHistoryToTeam.SelectedValue == -1 ||
                cmbTransferHistoryType.SelectedItem == null)
            {
                MessageBox.Show("Required fields missing.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Logic Check
            object fromTeamId = DBNull.Value;
            int toTeamId = (int)cmbTransferHistoryToTeam.SelectedValue;

            if (cmbTransferHistoryFromTeam.SelectedValue != null && (int)cmbTransferHistoryFromTeam.SelectedValue != -1)
            {
                fromTeamId = (int)cmbTransferHistoryFromTeam.SelectedValue;
                if ((int)fromTeamId == toTeamId)
                {
                    MessageBox.Show("Origin and Destination teams cannot be the same.", "Logic Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string query = @"
        UPDATE transfer_history SET 
            player_id = @playerId, 
            transfer_date = @date, 
            from_team_id = @fromId, 
            to_team_id = @toId, 
            transfer_fee_eur = @fee,
            transfer_type = @type,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            transfer_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@playerId", (int)cmbTransferHistoryPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@date", dtpTransferHistoryDate.Value);
                    command.Parameters.AddWithValue("@fromId", fromTeamId);
                    command.Parameters.AddWithValue("@toId", toTeamId);
                    command.Parameters.AddWithValue("@fee", numTransferHistoryFee.Value);
                    command.Parameters.AddWithValue("@type", cmbTransferHistoryType.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedTransferHistoryId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Transfer updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTransferHistoryAsync();
                        btnClearTransfer_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating transfer: " + ex.Message); }
        }

        private async void btnDeleteTransfer_Click(object sender, EventArgs e)
        {
            if (_selectedTransferHistoryId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE transfer_history SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                transfer_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedTransferHistoryId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Transfer deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTransferHistoryAsync();
                            btnClearTransfer_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting transfer: " + ex.Message); }
            }
        }

        private void dgvTransferHistory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvTransferHistory.Rows[e.RowIndex];

                    // 1. Ignore New Row
                    if (row.IsNewRow) return;

                    // 2. Check Null ID
                    if (row.Cells["transfer_id"].Value == null ||
                        row.Cells["transfer_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedTransferHistoryId = Convert.ToInt32(row.Cells["transfer_id"].Value);

                    // Map Player
                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbTransferHistoryPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    // Map From Team (Nullable)
                    if (row.Cells["from_team_id"].Value != null && row.Cells["from_team_id"].Value != DBNull.Value)
                        cmbTransferHistoryFromTeam.SelectedValue = Convert.ToInt32(row.Cells["from_team_id"].Value);
                    else
                        cmbTransferHistoryFromTeam.SelectedIndex = 0; // Default Select

                    // Map To Team
                    if (row.Cells["to_team_id"].Value != null && row.Cells["to_team_id"].Value != DBNull.Value)
                        cmbTransferHistoryToTeam.SelectedValue = Convert.ToInt32(row.Cells["to_team_id"].Value);

                    // Map Date
                    if (row.Cells["transfer_date"].Value != null && row.Cells["transfer_date"].Value != DBNull.Value)
                        dtpTransferHistoryDate.Value = Convert.ToDateTime(row.Cells["transfer_date"].Value);

                    // Map Fee
                    if (row.Cells["transfer_fee_eur"].Value != null && row.Cells["transfer_fee_eur"].Value != DBNull.Value)
                        numTransferHistoryFee.Value = Convert.ToDecimal(row.Cells["transfer_fee_eur"].Value);
                    else
                        numTransferHistoryFee.Value = 0;

                    // Map Type
                    if (row.Cells["transfer_type"].Value != null && row.Cells["transfer_type"].Value != DBNull.Value)
                        cmbTransferHistoryType.SelectedItem = row.Cells["transfer_type"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting transfer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearTransfer_Click(object sender, EventArgs e)
        {
            _selectedTransferHistoryId = 0;
            numTransferHistoryFee.Value = 0;
            dtpTransferHistoryDate.Value = DateTime.Now;

            if (cmbTransferHistoryPlayer.Items.Count > 0) cmbTransferHistoryPlayer.SelectedIndex = 0;
            if (cmbTransferHistoryFromTeam.Items.Count > 0) cmbTransferHistoryFromTeam.SelectedIndex = 0;
            if (cmbTransferHistoryToTeam.Items.Count > 0) cmbTransferHistoryToTeam.SelectedIndex = 0;
            if (cmbTransferHistoryType.Items.Count > 0) cmbTransferHistoryType.SelectedIndex = 0;

            dgvTransferHistory.ClearSelection();
        }
    }
}
