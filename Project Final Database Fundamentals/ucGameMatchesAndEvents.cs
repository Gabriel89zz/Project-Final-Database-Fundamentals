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
    public partial class ucGameMatchesAndEvents : UserControl
    {
        private readonly int _adminUserId;
        private int _selectedMatchId = 0;
        private int _selectedMatchEventId = 0;
        private int _selectedMatchOfficialId = 0;
        private int _selectedGoalId = 0;
        private int _selectedShotId = 0;
        private int _selectedFoulId = 0;
        private int _selectedCardId = 0;
        private int _selectedSubstitutionId = 0;
        private int _selectedTeamMatchStatId = 0;
        public ucGameMatchesAndEvents(int adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
        }

        private async void tabControlGameMatchesAndEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlGameMatchesAndEvents.SelectedTab == tabPageMatch)
            {
                // Init Fixed Status (Sync)
                InitializeMatchStatusCombo();

                // Parallel Loading (Grid + 4 Combos)
                // Teams load handles both Home and Away
                var tGrid = LoadMatchesAsync();
                var tComp = LoadCompSeasonsForMatchComboAsync();
                var tStage = LoadStagesForMatchComboAsync();
                var tStadium = LoadStadiumsForMatchComboAsync();
                var tTeams = LoadTeamsForMatchCombosAsync();

                await Task.WhenAll(tGrid, tComp, tStage, tStadium, tTeams);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageMatchEvent)
            {
                // Parallel Loading (Grid + 4 Combos)
                var tGrid = LoadMatchEventsAsync();
                var tMatch = LoadMatchesForEventComboAsync();
                var tType = LoadEventTypesForComboAsync();
                var tTeam = LoadTeamsForEventComboAsync();
                var tPlayer = LoadPlayersForEventComboAsync();

                await Task.WhenAll(tGrid, tMatch, tType, tTeam, tPlayer);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageMatchOfficial)
            {
                // 1. Init Fixed Role Combo (Sync)
                InitializeOfficialRoleCombo();

                // 2. Parallel Loading (Grid + 2 DB Combos)
                var tGrid = LoadMatchOfficialsAsync();
                var tMatch = LoadMatchesForOfficialComboAsync();
                var tRef = LoadRefereesForOfficialComboAsync();

                await Task.WhenAll(tGrid, tMatch, tRef);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageGoal)
            {
                InitializeBodyPartCombo();

                // Parallel Loading
                var tGrid = LoadGoalsAsync();
                var tMatch = LoadMatchesForGoalComboAsync();
                var tTeam = LoadTeamsForGoalComboAsync();
                var tPlayers = LoadPlayersForGoalCombosAsync(); // Loads both Scorer & Assist

                await Task.WhenAll(tGrid, tMatch, tTeam, tPlayers);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageShot)
            {
                InitializeShotBodyPartCombo();

                // Parallel Loading
                var tGrid = LoadShotsAsync();
                var tMatch = LoadMatchesForShotComboAsync();
                var tTeam = LoadTeamsForShotComboAsync();
                var tPlayer = LoadPlayersForShotComboAsync();

                await Task.WhenAll(tGrid, tMatch, tTeam, tPlayer);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageFoul)
            {
                // Parallel Loading
                var tGrid = LoadFoulsAsync();
                var tMatch = LoadMatchesForFoulComboAsync();
                var tTeams = LoadTeamsForFoulCombosAsync();
                var tPlayers = LoadPlayersForFoulCombosAsync();

                await Task.WhenAll(tGrid, tMatch, tTeams, tPlayers);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageCard)
            {
                InitializeCardTypeCombo();

                // Parallel Loading
                var tGrid = LoadCardsAsync();
                var tMatch = LoadMatchesForCardComboAsync();
                var tTeam = LoadTeamsForCardComboAsync();
                var tPlayer = LoadPlayersForCardComboAsync();

                await Task.WhenAll(tGrid, tMatch, tTeam, tPlayer);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageSubstitution)
            {
                // Parallel Loading
                var tGrid = LoadSubstitutionsAsync();
                var tMatch = LoadMatchesForSubstitutionComboAsync();
                var tTeam = LoadTeamsForSubstitutionComboAsync();
                var tPlayers = LoadPlayersForSubstitutionCombosAsync();

                await Task.WhenAll(tGrid, tMatch, tTeam, tPlayers);
            }
            else if (tabControlGameMatchesAndEvents.SelectedTab == tabPageTeamMatchStat)
            {
                // Parallel Loading
                var tGrid = LoadTeamMatchStatsAsync();
                var tMatch = LoadMatchesForStatComboAsync();
                var tTeam = LoadTeamsForStatComboAsync();

                await Task.WhenAll(tGrid, tMatch, tTeam);
            }
        }






        // 1. Initialize Match Status (Fixed Options)
        private void InitializeMatchStatusCombo()
        {
            cmbMatchStatus.Items.Clear();
            cmbMatchStatus.Items.Add("Scheduled");
            cmbMatchStatus.Items.Add("In Progress");
            cmbMatchStatus.Items.Add("Finished");
            cmbMatchStatus.Items.Add("Postponed");
            cmbMatchStatus.Items.Add("Cancelled");
            cmbMatchStatus.SelectedIndex = 0;
        }

        // 2. Load Competition-Seasons
        private async Task LoadCompSeasonsForMatchComboAsync()
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

                        cmbMatchCompetitionSeason.DataSource = dt;
                        cmbMatchCompetitionSeason.DisplayMember = "display_name";
                        cmbMatchCompetitionSeason.ValueMember = "competition_season_id";
                        cmbMatchCompetitionSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 3. Load Stages
        private async Task LoadStagesForMatchComboAsync()
        {
            string query = @"
        SELECT 
            st.stage_id, 
            st.name 
        FROM competiton_stage st 
        WHERE st.is_active = true 
        ORDER BY st.stage_order";

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
                        row["name"] = "-- Select Stage --";
                        dt.Rows.InsertAt(row, 0);

                        cmbMatchStage.DataSource = dt;
                        cmbMatchStage.DisplayMember = "name";
                        cmbMatchStage.ValueMember = "stage_id";
                        cmbMatchStage.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stages: " + ex.Message); }
        }

        // 4. Load Stadiums
        private async Task LoadStadiumsForMatchComboAsync()
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

                        cmbMatchStadium.DataSource = dt;
                        cmbMatchStadium.DisplayMember = "name";
                        cmbMatchStadium.ValueMember = "stadium_id";
                        cmbMatchStadium.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stadiums: " + ex.Message); }
        }

        // 5. Load Teams (For Home and Away)
        // Note: We need separate DataTables for Home and Away combos to avoid binding conflicts
        private async Task LoadTeamsForMatchCombosAsync()
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
                        DataTable dtHome = new DataTable();
                        dtHome.Load(reader);

                        // Copy for Away combo
                        DataTable dtAway = dtHome.Copy();

                        // Add default rows
                        DataRow rowHome = dtHome.NewRow();
                        rowHome["team_id"] = -1;
                        rowHome["name"] = "-- Select Home Team --";
                        dtHome.Rows.InsertAt(rowHome, 0);

                        DataRow rowAway = dtAway.NewRow();
                        rowAway["team_id"] = -1;
                        rowAway["name"] = "-- Select Away Team --";
                        dtAway.Rows.InsertAt(rowAway, 0);

                        // Bind Home
                        cmbMatchHomeTeam.DataSource = dtHome;
                        cmbMatchHomeTeam.DisplayMember = "name";
                        cmbMatchHomeTeam.ValueMember = "team_id";
                        cmbMatchHomeTeam.SelectedIndex = 0;

                        // Bind Away
                        cmbMatchAwayTeam.DataSource = dtAway;
                        cmbMatchAwayTeam.DisplayMember = "name";
                        cmbMatchAwayTeam.ValueMember = "team_id";
                        cmbMatchAwayTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 6. Load Main Grid
        private async Task LoadMatchesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
            SELECT 
                m.match_id,
                m.competition_season_id,
                (comp.name || ' ' || seas.name) AS comp_display,
                m.stage_id,
                st.name AS stage_name,
                m.stadium_id,
                std.name AS stadium_name,
                m.match_date,
                m.home_team_id,
                t1.name AS home_team,
                m.away_team_id,
                t2.name AS away_team,
                m.home_score,
                m.away_score,
                m.match_status,
                m.attendance
            FROM ""match"" m
            INNER JOIN competition_season cs ON m.competition_season_id = cs.competition_season_id
            INNER JOIN competition comp ON cs.competition_id = comp.competition_id
            INNER JOIN season seas ON cs.season_id = seas.season_id
            INNER JOIN competiton_stage st ON m.stage_id = st.stage_id
            INNER JOIN stadium std ON m.stadium_id = std.stadium_id
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

                        dgvMatch.DataSource = dt;

                        // 1. MOSTRAR SOLO EL ID PRINCIPAL (PK)
                        if (dgvMatch.Columns["match_id"] != null)
                        {
                            dgvMatch.Columns["match_id"].Visible = true;
                            dgvMatch.Columns["match_id"].HeaderText = "ID Match";
                            dgvMatch.Columns["match_id"].DisplayIndex = 0;
                            dgvMatch.Columns["match_id"].Width = 60;
                        }

                        // 2. OCULTAR LAS LLAVES FORÁNEAS (FKs)
                        string[] hiddenFKs = { "competition_season_id", "stage_id", "stadium_id", "home_team_id", "away_team_id" };
                        foreach (var col in hiddenFKs)
                        {
                            if (dgvMatch.Columns[col] != null)
                                dgvMatch.Columns[col].Visible = false;
                        }

                        // Formato de Fecha
                        if (dgvMatch.Columns["match_date"] != null)
                            dgvMatch.Columns["match_date"].DefaultCellStyle.Format = "g";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddMatch_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbMatchCompetitionSeason.SelectedValue == -1 ||
                (int)cmbMatchStage.SelectedValue == -1 ||
                (int)cmbMatchStadium.SelectedValue == -1 ||
                (int)cmbMatchHomeTeam.SelectedValue == -1 ||
                (int)cmbMatchAwayTeam.SelectedValue == -1 ||
                cmbMatchStatus.SelectedItem == null)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int homeId = (int)cmbMatchHomeTeam.SelectedValue;
            int awayId = (int)cmbMatchAwayTeam.SelectedValue;

            // 2. Logic Check
            if (homeId == awayId)
            {
                MessageBox.Show("Home Team and Away Team cannot be the same.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Get Values
            int compSeasonId = (int)cmbMatchCompetitionSeason.SelectedValue;
            int stageId = (int)cmbMatchStage.SelectedValue;
            int stadiumId = (int)cmbMatchStadium.SelectedValue;
            DateTime matchDate = dtpMatchDate.Value;
            int homeScore = (int)nudMatchHomeScore.Value;
            int awayScore = (int)nudMatchAwayScore.Value;
            int attendance = (int)nudMatchAttendance.Value;
            string status = cmbMatchStatus.SelectedItem.ToString();

            // NOTE: "match" is reserved, use quotes
            string query = @"
        INSERT INTO ""match"" 
        (competition_season_id, stage_id, stadium_id, match_date, home_team_id, away_team_id, home_score, away_score, match_status, attendance, created_by) 
        VALUES 
        (@compSeasonId, @stageId, @stadiumId, @date, @homeId, @awayId, @homeScore, @awayScore, @status, @attendance, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", compSeasonId);
                    command.Parameters.AddWithValue("@stageId", stageId);
                    command.Parameters.AddWithValue("@stadiumId", stadiumId);
                    command.Parameters.AddWithValue("@date", matchDate);
                    command.Parameters.AddWithValue("@homeId", homeId);
                    command.Parameters.AddWithValue("@awayId", awayId);
                    command.Parameters.AddWithValue("@homeScore", homeScore);
                    command.Parameters.AddWithValue("@awayScore", awayScore);
                    command.Parameters.AddWithValue("@status", status);
                    command.Parameters.AddWithValue("@attendance", attendance);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Match added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchesAsync();
                        btnClearMatch_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateMatch_Click(object sender, EventArgs e)
        {
            if (_selectedMatchId == 0)
            {
                MessageBox.Show("Please select a match to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbMatchHomeTeam.SelectedValue == -1 ||
                (int)cmbMatchAwayTeam.SelectedValue == -1)
            {
                MessageBox.Show("Teams are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int homeId = (int)cmbMatchHomeTeam.SelectedValue;
            int awayId = (int)cmbMatchAwayTeam.SelectedValue;

            if (homeId == awayId)
            {
                MessageBox.Show("Home Team and Away Team cannot be the same.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE ""match"" SET 
            competition_season_id = @compSeasonId, 
            stage_id = @stageId, 
            stadium_id = @stadiumId, 
            match_date = @date, 
            home_team_id = @homeId, 
            away_team_id = @awayId, 
            home_score = @homeScore, 
            away_score = @awayScore, 
            match_status = @status, 
            attendance = @attendance,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            match_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@compSeasonId", (int)cmbMatchCompetitionSeason.SelectedValue);
                    command.Parameters.AddWithValue("@stageId", (int)cmbMatchStage.SelectedValue);
                    command.Parameters.AddWithValue("@stadiumId", (int)cmbMatchStadium.SelectedValue);
                    command.Parameters.AddWithValue("@date", dtpMatchDate.Value);
                    command.Parameters.AddWithValue("@homeId", homeId);
                    command.Parameters.AddWithValue("@awayId", awayId);
                    command.Parameters.AddWithValue("@homeScore", (int)nudMatchHomeScore.Value);
                    command.Parameters.AddWithValue("@awayScore", (int)nudMatchAwayScore.Value);
                    command.Parameters.AddWithValue("@status", cmbMatchStatus.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@attendance", (int)nudMatchAttendance.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedMatchId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Match updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchesAsync();
                        btnClearMatch_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating match: " + ex.Message); }
        }

        private async void btnDeleteMatch_Click(object sender, EventArgs e)
        {
            if (_selectedMatchId == 0)
            {
                MessageBox.Show("Please select a match to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE ""match"" SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                match_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedMatchId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Match deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadMatchesAsync();
                            btnClearMatch_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting match: " + ex.Message); }
            }
        }

        private void dgvMatch_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvMatch.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["match_id"].Value == null || row.Cells["match_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedMatchId = Convert.ToInt32(row.Cells["match_id"].Value);

                    // Map Combos
                    if (row.Cells["competition_season_id"].Value != null && row.Cells["competition_season_id"].Value != DBNull.Value)
                        cmbMatchCompetitionSeason.SelectedValue = Convert.ToInt32(row.Cells["competition_season_id"].Value);

                    if (row.Cells["stage_id"].Value != null && row.Cells["stage_id"].Value != DBNull.Value)
                        cmbMatchStage.SelectedValue = Convert.ToInt32(row.Cells["stage_id"].Value);

                    if (row.Cells["stadium_id"].Value != null && row.Cells["stadium_id"].Value != DBNull.Value)
                        cmbMatchStadium.SelectedValue = Convert.ToInt32(row.Cells["stadium_id"].Value);

                    if (row.Cells["home_team_id"].Value != null && row.Cells["home_team_id"].Value != DBNull.Value)
                        cmbMatchHomeTeam.SelectedValue = Convert.ToInt32(row.Cells["home_team_id"].Value);

                    if (row.Cells["away_team_id"].Value != null && row.Cells["away_team_id"].Value != DBNull.Value)
                        cmbMatchAwayTeam.SelectedValue = Convert.ToInt32(row.Cells["away_team_id"].Value);

                    // Map Date
                    if (row.Cells["match_date"].Value != null && row.Cells["match_date"].Value != DBNull.Value)
                        dtpMatchDate.Value = Convert.ToDateTime(row.Cells["match_date"].Value);

                    // Map Numerics
                    if (row.Cells["home_score"].Value != DBNull.Value)
                        nudMatchHomeScore.Value = Convert.ToDecimal(row.Cells["home_score"].Value);

                    if (row.Cells["away_score"].Value != DBNull.Value)
                        nudMatchAwayScore.Value = Convert.ToDecimal(row.Cells["away_score"].Value);

                    if (row.Cells["attendance"].Value != DBNull.Value)
                        nudMatchAttendance.Value = Convert.ToDecimal(row.Cells["attendance"].Value);

                    // Map Status
                    if (row.Cells["match_status"].Value != null && row.Cells["match_status"].Value != DBNull.Value)
                        cmbMatchStatus.SelectedItem = row.Cells["match_status"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting match: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearMatch_Click(object sender, EventArgs e)
        {
            _selectedMatchId = 0;
            dtpMatchDate.Value = DateTime.Now;
            nudMatchHomeScore.Value = 0;
            nudMatchAwayScore.Value = 0;
            nudMatchAttendance.Value = 0;

            if (cmbMatchCompetitionSeason.Items.Count > 0) cmbMatchCompetitionSeason.SelectedIndex = 0;
            if (cmbMatchStage.Items.Count > 0) cmbMatchStage.SelectedIndex = 0;
            if (cmbMatchStadium.Items.Count > 0) cmbMatchStadium.SelectedIndex = 0;
            if (cmbMatchHomeTeam.Items.Count > 0) cmbMatchHomeTeam.SelectedIndex = 0;
            if (cmbMatchAwayTeam.Items.Count > 0) cmbMatchAwayTeam.SelectedIndex = 0;
            if (cmbMatchStatus.Items.Count > 0) cmbMatchStatus.SelectedIndex = 0;

            dgvMatch.ClearSelection();
        }





        // 1. Load Matches (Formatted: "Team A vs Team B - Date")
        private async Task LoadMatchesForEventComboAsync()
        {
            string query = @"
        SELECT 
            m.match_id, 
            t1.name || ' vs ' || t2.name || ' (' || to_char(m.match_date, 'YYYY-MM-DD') || ')' AS display_name
        FROM match m
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

                        cmbMatchEventMatch.DataSource = dt;
                        cmbMatchEventMatch.DisplayMember = "display_name";
                        cmbMatchEventMatch.ValueMember = "match_id";
                        cmbMatchEventMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 2. Load Event Types
        private async Task LoadEventTypesForComboAsync()
        {
            // Assuming table 'event_type' exists
            string query = @"SELECT event_type_id, name FROM event_type WHERE is_active = true ORDER BY name";

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
                        row["event_type_id"] = -1;
                        row["name"] = "-- Select Type --";
                        dt.Rows.InsertAt(row, 0);

                        cmbMatchEventType.DataSource = dt;
                        cmbMatchEventType.DisplayMember = "name";
                        cmbMatchEventType.ValueMember = "event_type_id";
                        cmbMatchEventType.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading event types: " + ex.Message); }
        }

        // 3. Load Teams
        private async Task LoadTeamsForEventComboAsync()
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

                        cmbMatchEventTeam.DataSource = dt;
                        cmbMatchEventTeam.DisplayMember = "name";
                        cmbMatchEventTeam.ValueMember = "team_id";
                        cmbMatchEventTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Players
        private async Task LoadPlayersForEventComboAsync()
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

                        cmbMatchEventPlayer.DataSource = dt;
                        cmbMatchEventPlayer.DisplayMember = "full_name";
                        cmbMatchEventPlayer.ValueMember = "player_id";
                        cmbMatchEventPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 5. Load Main Grid (ID VISIBLE)
        private async Task LoadMatchEventsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            me.event_id, 
            me.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            me.event_type_id,
            et.name AS event_name,
            me.minute,
            me.team_id,
            t.name AS team_name,
            me.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name
        FROM match_event me
        INNER JOIN match m ON me.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN event_type et ON me.event_type_id = et.event_type_id
        INNER JOIN team t ON me.team_id = t.team_id
        INNER JOIN player p ON me.player_id = p.player_id
        WHERE me.is_active = true
        ORDER BY m.match_date DESC, me.minute";

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

                        dgvMatchEvent.DataSource = dt;

                        // NOTE: We do NOT hide "match_event_id" as requested.
                        // We only hide the foreign key columns to keep it clean.
                        string[] hiddenCols = { "match_id", "event_type_id", "team_id", "player_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvMatchEvent.Columns[col] != null)
                                dgvMatchEvent.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading events: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddMatchEvent_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbMatchEventMatch.SelectedValue == -1 ||
                (int)cmbMatchEventType.SelectedValue == -1 ||
                (int)cmbMatchEventTeam.SelectedValue == -1 ||
                (int)cmbMatchEventPlayer.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbMatchEventMatch.SelectedValue;
            int typeId = (int)cmbMatchEventType.SelectedValue;
            int teamId = (int)cmbMatchEventTeam.SelectedValue;
            int playerId = (int)cmbMatchEventPlayer.SelectedValue;
            int minute = (int)nudMatchEventMinute.Value;

            string query = @"
        INSERT INTO match_event 
        (match_id, event_type_id, team_id, player_id, minute, created_by) 
        VALUES 
        (@matchId, @typeId, @teamId, @playerId, @minute, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@typeId", typeId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@minute", minute);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Match Event added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchEventsAsync();
                        btnClearMatchEvent_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateMatchEvent_Click(object sender, EventArgs e)
        {
            if (_selectedMatchEventId == 0)
            {
                MessageBox.Show("Please select an event to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbMatchEventMatch.SelectedValue == -1 ||
                (int)cmbMatchEventType.SelectedValue == -1 ||
                (int)cmbMatchEventTeam.SelectedValue == -1 ||
                (int)cmbMatchEventPlayer.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE match_event SET 
            match_id = @matchId, 
            event_type_id = @typeId, 
            team_id = @teamId, 
            player_id = @playerId,
            minute = @minute,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            event_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbMatchEventMatch.SelectedValue);
                    command.Parameters.AddWithValue("@typeId", (int)cmbMatchEventType.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbMatchEventTeam.SelectedValue);
                    command.Parameters.AddWithValue("@playerId", (int)cmbMatchEventPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@minute", (int)nudMatchEventMinute.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedMatchEventId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Match Event updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchEventsAsync();
                        btnClearMatchEvent_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating event: " + ex.Message); }
        }

        private async void btnDeleteMatchEvent_Click(object sender, EventArgs e)
        {
            if (_selectedMatchEventId == 0)
            {
                MessageBox.Show("Please select an event to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE match_event SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                match_event_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedMatchEventId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Match Event deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadMatchEventsAsync();
                            btnClearMatchEvent_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting event: " + ex.Message); }
            }
        }

        private void dgvMatchEvent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvMatchEvent.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["event_id"].Value == null || row.Cells["event_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedMatchEventId = Convert.ToInt32(row.Cells["event_id"].Value);

                    // Map Combos
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbMatchEventMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["event_type_id"].Value != null && row.Cells["event_type_id"].Value != DBNull.Value)
                        cmbMatchEventType.SelectedValue = Convert.ToInt32(row.Cells["event_type_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbMatchEventTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbMatchEventPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    // Map Numeric
                    if (row.Cells["minute"].Value != null && row.Cells["minute"].Value != DBNull.Value)
                        nudMatchEventMinute.Value = Convert.ToDecimal(row.Cells["minute"].Value);
                    else
                        nudMatchEventMinute.Value = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting event: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearMatchEvent_Click(object sender, EventArgs e)
        {
            _selectedMatchEventId = 0;
            nudMatchEventMinute.Value = 0;

            if (cmbMatchEventMatch.Items.Count > 0) cmbMatchEventMatch.SelectedIndex = 0;
            if (cmbMatchEventType.Items.Count > 0) cmbMatchEventType.SelectedIndex = 0;
            if (cmbMatchEventTeam.Items.Count > 0) cmbMatchEventTeam.SelectedIndex = 0;
            if (cmbMatchEventPlayer.Items.Count > 0) cmbMatchEventPlayer.SelectedIndex = 0;

            dgvMatchEvent.ClearSelection();
        }








        // 1. Initialize Roles (Fixed Options)
        private void InitializeOfficialRoleCombo()
        {
            cmbMatchOfficialRole.Items.Clear();
            cmbMatchOfficialRole.Items.Add("Referee");
            cmbMatchOfficialRole.Items.Add("Assistant Referee 1");
            cmbMatchOfficialRole.Items.Add("Assistant Referee 2");
            cmbMatchOfficialRole.Items.Add("4th Official");
            cmbMatchOfficialRole.Items.Add("VAR");
            cmbMatchOfficialRole.Items.Add("AVAR");

            cmbMatchOfficialRole.SelectedIndex = 0;
        }

        // 2. Load Matches (Formatted: "Team A vs Team B - Date")
        private async Task LoadMatchesForOfficialComboAsync()
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

                        cmbMatchOfficialMatch.DataSource = dt;
                        cmbMatchOfficialMatch.DisplayMember = "display_name";
                        cmbMatchOfficialMatch.ValueMember = "match_id";
                        cmbMatchOfficialMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 3. Load Referees (Formatted: "First Last")
        private async Task LoadRefereesForOfficialComboAsync()
        {
            string query = @"
        SELECT 
            referee_id, 
            first_name || ' ' || last_name AS full_name 
        FROM referee 
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
                        row["referee_id"] = -1;
                        row["full_name"] = "-- Select Referee --";
                        dt.Rows.InsertAt(row, 0);

                        cmbMatchOfficialReferee.DataSource = dt;
                        cmbMatchOfficialReferee.DisplayMember = "full_name";
                        cmbMatchOfficialReferee.ValueMember = "referee_id";
                        cmbMatchOfficialReferee.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading referees: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadMatchOfficialsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            mo.match_official_id, 
            mo.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            mo.referee_id,
            (r.first_name || ' ' || r.last_name) AS referee_name,
            mo.role
        FROM match_official mo
        INNER JOIN ""match"" m ON mo.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN referee r ON mo.referee_id = r.referee_id
        WHERE mo.is_active = true
        ORDER BY m.match_date DESC, mo.role";

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

                        dgvMatchOfficial.DataSource = dt;

                        // NOTE: match_official_id is VISIBLE as requested.
                        // We only hide the foreign keys.
                        string[] hiddenCols = { "match_id", "referee_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvMatchOfficial.Columns[col] != null)
                                dgvMatchOfficial.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading match officials: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddMatchOfficial_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbMatchOfficialMatch.SelectedValue == -1 ||
                (int)cmbMatchOfficialReferee.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Match and a Referee.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbMatchOfficialRole.SelectedItem == null)
            {
                MessageBox.Show("Please select a Role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbMatchOfficialMatch.SelectedValue;
            int refereeId = (int)cmbMatchOfficialReferee.SelectedValue;
            string role = cmbMatchOfficialRole.SelectedItem.ToString();

            string query = @"
        INSERT INTO match_official 
        (match_id, referee_id, role, created_by) 
        VALUES 
        (@matchId, @refereeId, @role, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@refereeId", refereeId);
                    command.Parameters.AddWithValue("@role", role);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Match Official added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchOfficialsAsync();
                        btnClearMatchOffical_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Referee is already assigned to this match (or role conflict).", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateMatchOfficial_Click(object sender, EventArgs e)
        {
            if (_selectedMatchOfficialId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbMatchOfficialMatch.SelectedValue == -1 ||
                (int)cmbMatchOfficialReferee.SelectedValue == -1 ||
                cmbMatchOfficialRole.SelectedItem == null)
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE match_official SET 
            match_id = @matchId, 
            referee_id = @refereeId, 
            role = @role,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            match_official_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbMatchOfficialMatch.SelectedValue);
                    command.Parameters.AddWithValue("@refereeId", (int)cmbMatchOfficialReferee.SelectedValue);
                    command.Parameters.AddWithValue("@role", cmbMatchOfficialRole.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedMatchOfficialId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchOfficialsAsync();
                        btnClearMatchOffical_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteMatchOfficial_Click(object sender, EventArgs e)
        {
            if (_selectedMatchOfficialId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE match_official SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                match_official_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedMatchOfficialId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadMatchOfficialsAsync();
                            btnClearMatchOffical_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvMatchOfficial_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvMatchOfficial.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["match_official_id"].Value == null || row.Cells["match_official_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedMatchOfficialId = Convert.ToInt32(row.Cells["match_official_id"].Value);

                    // Map Combos
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbMatchOfficialMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["referee_id"].Value != null && row.Cells["referee_id"].Value != DBNull.Value)
                        cmbMatchOfficialReferee.SelectedValue = Convert.ToInt32(row.Cells["referee_id"].Value);

                    // Map Role (String)
                    if (row.Cells["role"].Value != null && row.Cells["role"].Value != DBNull.Value)
                    {
                        string role = row.Cells["role"].Value.ToString();
                        if (cmbMatchOfficialRole.Items.Contains(role))
                            cmbMatchOfficialRole.SelectedItem = role;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearMatchOffical_Click(object sender, EventArgs e)
        {
            _selectedMatchOfficialId = 0;

            if (cmbMatchOfficialMatch.Items.Count > 0) cmbMatchOfficialMatch.SelectedIndex = 0;
            if (cmbMatchOfficialReferee.Items.Count > 0) cmbMatchOfficialReferee.SelectedIndex = 0;
            if (cmbMatchOfficialRole.Items.Count > 0) cmbMatchOfficialRole.SelectedIndex = 0;

            dgvMatchOfficial.ClearSelection();
        }






        // 1. Initialize Body Part Combo (Fixed)
        private void InitializeBodyPartCombo()
        {
            cmbGoalBodyPart.Items.Clear();
            cmbGoalBodyPart.Items.Add("Right Foot");
            cmbGoalBodyPart.Items.Add("Left Foot");
            cmbGoalBodyPart.Items.Add("Head");
            cmbGoalBodyPart.Items.Add("Chest");
            cmbGoalBodyPart.Items.Add("Other");

            cmbGoalBodyPart.SelectedIndex = 0;
        }

        // 2. Load Matches (Formatted: "Team A vs Team B")
        private async Task LoadMatchesForGoalComboAsync()
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

                        cmbGoalMatch.DataSource = dt;
                        cmbGoalMatch.DisplayMember = "display_name";
                        cmbGoalMatch.ValueMember = "match_id";
                        cmbGoalMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 3. Load Teams
        private async Task LoadTeamsForGoalComboAsync()
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

                        cmbGoalScoringTeam.DataSource = dt;
                        cmbGoalScoringTeam.DisplayMember = "name";
                        cmbGoalScoringTeam.ValueMember = "team_id";
                        cmbGoalScoringTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Players (Used for Scorer AND Assist)
        // We create two separate calls/tables to avoid binding conflicts
        private async Task LoadPlayersForGoalCombosAsync()
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
                        // Scorer Table
                        DataTable dtScorer = new DataTable();
                        dtScorer.Load(reader);

                        // Assist Table (Copy structure and data)
                        DataTable dtAssist = dtScorer.Copy();

                        // Add Default Rows
                        DataRow rowScorer = dtScorer.NewRow();
                        rowScorer["player_id"] = -1;
                        rowScorer["full_name"] = "-- Select Scorer --";
                        dtScorer.Rows.InsertAt(rowScorer, 0);

                        DataRow rowAssist = dtAssist.NewRow();
                        rowAssist["player_id"] = -1; // -1 indicates No Assist
                        rowAssist["full_name"] = "-- No Assist / Select Player --";
                        dtAssist.Rows.InsertAt(rowAssist, 0);

                        // Bind Scorer
                        cmbGoalScoringPlayer.DataSource = dtScorer;
                        cmbGoalScoringPlayer.DisplayMember = "full_name";
                        cmbGoalScoringPlayer.ValueMember = "player_id";
                        cmbGoalScoringPlayer.SelectedIndex = 0;

                        // Bind Assist
                        cmbGoalAssistPlayer.DataSource = dtAssist;
                        cmbGoalAssistPlayer.DisplayMember = "full_name";
                        cmbGoalAssistPlayer.ValueMember = "player_id";
                        cmbGoalAssistPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 5. Load Main Grid (ID VISIBLE)
        private async Task LoadGoalsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            g.goal_id, 
            g.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            g.scoring_team_id,
            team.name AS scoring_team,
            g.scoring_player_id,
            (p1.first_name || ' ' || p1.last_name) AS scorer_name,
            g.assist_player_id,
            (p2.first_name || ' ' || p2.last_name) AS assist_name,
            g.minute,
            g.is_own_goal,
            g.is_penalty,
            g.body_part
        FROM goal g
        INNER JOIN ""match"" m ON g.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team ON g.scoring_team_id = team.team_id
        INNER JOIN player p1 ON g.scoring_player_id = p1.player_id
        LEFT JOIN player p2 ON g.assist_player_id = p2.player_id -- LEFT JOIN for optional assist
        WHERE g.is_active = true
        ORDER BY m.match_date DESC, g.minute";

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

                        dgvGoal.DataSource = dt;

                        // Only hide FK IDs, keep goal_id VISIBLE as requested
                        string[] hiddenCols = { "match_id", "scoring_team_id", "scoring_player_id", "assist_player_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvGoal.Columns[col] != null)
                                dgvGoal.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading goals: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddGoal_Click(object sender, EventArgs e)
        {
            // 1. Validate Required Combos
            if ((int)cmbGoalMatch.SelectedValue == -1 ||
                (int)cmbGoalScoringTeam.SelectedValue == -1 ||
                (int)cmbGoalScoringPlayer.SelectedValue == -1)
            {
                MessageBox.Show("Match, Scoring Team, and Scorer are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbGoalBodyPart.SelectedItem == null)
            {
                MessageBox.Show("Please select a Body Part.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbGoalMatch.SelectedValue;
            int teamId = (int)cmbGoalScoringTeam.SelectedValue;
            int scorerId = (int)cmbGoalScoringPlayer.SelectedValue;
            int minute = (int)nudGoalMinute.Value;
            bool isOwn = chkbIsOwnGoal.Checked;
            bool isPenalty = chkbIsPenalty.Checked;
            string bodyPart = cmbGoalBodyPart.SelectedItem.ToString();

            // Handle Optional Assist
            object assistId = DBNull.Value;
            if (cmbGoalAssistPlayer.SelectedValue != null && (int)cmbGoalAssistPlayer.SelectedValue != -1)
            {
                assistId = (int)cmbGoalAssistPlayer.SelectedValue;
            }

            string query = @"
        INSERT INTO goal 
        (match_id, scoring_team_id, scoring_player_id, assist_player_id, minute, is_own_goal, is_penalty, body_part, created_by) 
        VALUES 
        (@matchId, @teamId, @scorerId, @assistId, @minute, @isOwn, @isPenalty, @bodyPart, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@scorerId", scorerId);
                    command.Parameters.AddWithValue("@assistId", assistId);
                    command.Parameters.AddWithValue("@minute", minute);
                    command.Parameters.AddWithValue("@isOwn", isOwn);
                    command.Parameters.AddWithValue("@isPenalty", isPenalty);
                    command.Parameters.AddWithValue("@bodyPart", bodyPart);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Goal added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadGoalsAsync();
                        btnClearGoal_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateGoal_Click(object sender, EventArgs e)
        {
            if (_selectedGoalId == 0)
            {
                MessageBox.Show("Please select a goal to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbGoalMatch.SelectedValue == -1 ||
                (int)cmbGoalScoringTeam.SelectedValue == -1 ||
                (int)cmbGoalScoringPlayer.SelectedValue == -1 ||
                cmbGoalBodyPart.SelectedItem == null)
            {
                MessageBox.Show("Required fields missing.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            object assistId = DBNull.Value;
            if (cmbGoalAssistPlayer.SelectedValue != null && (int)cmbGoalAssistPlayer.SelectedValue != -1)
            {
                assistId = (int)cmbGoalAssistPlayer.SelectedValue;
            }

            string query = @"
        UPDATE goal SET 
            match_id = @matchId, 
            scoring_team_id = @teamId, 
            scoring_player_id = @scorerId, 
            assist_player_id = @assistId,
            minute = @minute,
            is_own_goal = @isOwn,
            is_penalty = @isPenalty,
            body_part = @bodyPart,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            goal_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbGoalMatch.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbGoalScoringTeam.SelectedValue);
                    command.Parameters.AddWithValue("@scorerId", (int)cmbGoalScoringPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@assistId", assistId);
                    command.Parameters.AddWithValue("@minute", (int)nudGoalMinute.Value);
                    command.Parameters.AddWithValue("@isOwn", chkbIsOwnGoal.Checked);
                    command.Parameters.AddWithValue("@isPenalty", chkbIsPenalty.Checked);
                    command.Parameters.AddWithValue("@bodyPart", cmbGoalBodyPart.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedGoalId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Goal updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadGoalsAsync();
                        btnClearGoal_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating goal: " + ex.Message); }
        }

        private async void btnDeleteGoal_Click(object sender, EventArgs e)
        {
            if (_selectedGoalId == 0)
            {
                MessageBox.Show("Please select a goal to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE goal SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                goal_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedGoalId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Goal deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadGoalsAsync();
                            btnClearGoal_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting goal: " + ex.Message); }
            }
        }

        private void dgvGoal_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvGoal.Rows[e.RowIndex];

                    // Ignore New Row
                    if (row.IsNewRow) return;

                    // Check Null ID
                    if (row.Cells["goal_id"].Value == null || row.Cells["goal_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedGoalId = Convert.ToInt32(row.Cells["goal_id"].Value);

                    // Map Combos (with null checks)
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbGoalMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["scoring_team_id"].Value != null && row.Cells["scoring_team_id"].Value != DBNull.Value)
                        cmbGoalScoringTeam.SelectedValue = Convert.ToInt32(row.Cells["scoring_team_id"].Value);

                    if (row.Cells["scoring_player_id"].Value != null && row.Cells["scoring_player_id"].Value != DBNull.Value)
                        cmbGoalScoringPlayer.SelectedValue = Convert.ToInt32(row.Cells["scoring_player_id"].Value);

                    // Handle Assist (Nullable)
                    if (row.Cells["assist_player_id"].Value != null && row.Cells["assist_player_id"].Value != DBNull.Value)
                        cmbGoalAssistPlayer.SelectedValue = Convert.ToInt32(row.Cells["assist_player_id"].Value);
                    else
                        cmbGoalAssistPlayer.SelectedIndex = 0; // Default to "No Assist"

                    // Map Body Part
                    if (row.Cells["body_part"].Value != null && row.Cells["body_part"].Value != DBNull.Value)
                        cmbGoalBodyPart.SelectedItem = row.Cells["body_part"].Value.ToString();

                    // Map Minute
                    if (row.Cells["minute"].Value != null && row.Cells["minute"].Value != DBNull.Value)
                        nudGoalMinute.Value = Convert.ToDecimal(row.Cells["minute"].Value);

                    // Map Checkboxes
                    if (row.Cells["is_own_goal"].Value != DBNull.Value)
                        chkbIsOwnGoal.Checked = Convert.ToBoolean(row.Cells["is_own_goal"].Value);
                    else
                        chkbIsOwnGoal.Checked = false;

                    if (row.Cells["is_penalty"].Value != DBNull.Value)
                        chkbIsPenalty.Checked = Convert.ToBoolean(row.Cells["is_penalty"].Value);
                    else
                        chkbIsPenalty.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting goal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearGoal_Click(object sender, EventArgs e)
        {
            _selectedGoalId = 0;
            nudGoalMinute.Value = 0;
            chkbIsOwnGoal.Checked = false;
            chkbIsPenalty.Checked = false;

            if (cmbGoalMatch.Items.Count > 0) cmbGoalMatch.SelectedIndex = 0;
            if (cmbGoalScoringTeam.Items.Count > 0) cmbGoalScoringTeam.SelectedIndex = 0;
            if (cmbGoalScoringPlayer.Items.Count > 0) cmbGoalScoringPlayer.SelectedIndex = 0;
            if (cmbGoalAssistPlayer.Items.Count > 0) cmbGoalAssistPlayer.SelectedIndex = 0;
            if (cmbGoalBodyPart.Items.Count > 0) cmbGoalBodyPart.SelectedIndex = 0;

            dgvGoal.ClearSelection();
        }









        // 1. Initialize Body Part Combo (Fixed)
        private void InitializeShotBodyPartCombo()
        {
            cmbShotBodyPart.Items.Clear();
            cmbShotBodyPart.Items.Add("Right Foot");
            cmbShotBodyPart.Items.Add("Left Foot");
            cmbShotBodyPart.Items.Add("Head");
            cmbShotBodyPart.Items.Add("Chest");
            cmbShotBodyPart.Items.Add("Other");

            cmbShotBodyPart.SelectedIndex = 0;
        }

        // 2. Load Matches (Context: "Team A vs Team B")
        private async Task LoadMatchesForShotComboAsync()
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

                        cmbShotMatch.DataSource = dt;
                        cmbShotMatch.DisplayMember = "display_name";
                        cmbShotMatch.ValueMember = "match_id";
                        cmbShotMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 3. Load Teams
        private async Task LoadTeamsForShotComboAsync()
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

                        cmbShotTeam.DataSource = dt;
                        cmbShotTeam.DisplayMember = "name";
                        cmbShotTeam.ValueMember = "team_id";
                        cmbShotTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Players
        private async Task LoadPlayersForShotComboAsync()
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

                        cmbShotPlayer.DataSource = dt;
                        cmbShotPlayer.DisplayMember = "full_name";
                        cmbShotPlayer.ValueMember = "player_id";
                        cmbShotPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 5. Load Main Grid (ID VISIBLE)
        private async Task LoadShotsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            s.shot_id, 
            s.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            s.team_id,
            t.name AS team_name,
            s.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            s.minute,
            s.is_on_target,
            s.is_goal,
            s.location_x,
            s.location_y,
            s.body_part
        FROM shot s
        INNER JOIN ""match"" m ON s.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team t ON s.team_id = t.team_id
        INNER JOIN player p ON s.player_id = p.player_id
        WHERE s.is_active = true
        ORDER BY m.match_date DESC, s.minute";

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

                        dgvShot.DataSource = dt;

                        // Only hide Foreign Keys, keep shot_id visible
                        string[] hiddenCols = { "match_id", "team_id", "player_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvShot.Columns[col] != null)
                                dgvShot.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading shots: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddShot_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbShotMatch.SelectedValue == -1 ||
                (int)cmbShotTeam.SelectedValue == -1 ||
                (int)cmbShotPlayer.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbShotBodyPart.SelectedItem == null)
            {
                MessageBox.Show("Please select a Body Part.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbShotMatch.SelectedValue;
            int teamId = (int)cmbShotTeam.SelectedValue;
            int playerId = (int)cmbShotPlayer.SelectedValue;
            int minute = (int)nudShotMinute.Value;
            bool isOnTarget = chkbIsOnTarget.Checked;
            bool isGoal = chkbIsGoal.Checked;
            decimal locX = nudShotLocationX.Value;
            decimal locY = nudShotLocationY.Value;
            string bodyPart = cmbShotBodyPart.SelectedItem.ToString();

            string query = @"
        INSERT INTO shot 
        (match_id, team_id, player_id, minute, is_on_target, is_goal, location_x, location_y, body_part, created_by) 
        VALUES 
        (@matchId, @teamId, @playerId, @minute, @target, @goal, @x, @y, @body, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@minute", minute);
                    command.Parameters.AddWithValue("@target", isOnTarget);
                    command.Parameters.AddWithValue("@goal", isGoal);
                    command.Parameters.AddWithValue("@x", locX);
                    command.Parameters.AddWithValue("@y", locY);
                    command.Parameters.AddWithValue("@body", bodyPart);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Shot added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadShotsAsync();
                        btnClearShot_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateShot_Click(object sender, EventArgs e)
        {
            if (_selectedShotId == 0)
            {
                MessageBox.Show("Please select a shot to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbShotMatch.SelectedValue == -1 ||
                (int)cmbShotTeam.SelectedValue == -1 ||
                (int)cmbShotPlayer.SelectedValue == -1 ||
                cmbShotBodyPart.SelectedItem == null)
            {
                MessageBox.Show("Required fields missing.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE shot SET 
            match_id = @matchId, 
            team_id = @teamId, 
            player_id = @playerId, 
            minute = @minute, 
            is_on_target = @target, 
            is_goal = @goal, 
            location_x = @x, 
            location_y = @y, 
            body_part = @body,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            shot_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbShotMatch.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbShotTeam.SelectedValue);
                    command.Parameters.AddWithValue("@playerId", (int)cmbShotPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@minute", (int)nudShotMinute.Value);
                    command.Parameters.AddWithValue("@target", chkbIsOnTarget.Checked);
                    command.Parameters.AddWithValue("@goal", chkbIsGoal.Checked);
                    command.Parameters.AddWithValue("@x", nudShotLocationX.Value);
                    command.Parameters.AddWithValue("@y", nudShotLocationY.Value);
                    command.Parameters.AddWithValue("@body", cmbShotBodyPart.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedShotId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Shot updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadShotsAsync();
                        btnClearShot_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating shot: " + ex.Message); }
        }

        private async void btnDeleteShot_Click(object sender, EventArgs e)
        {
            if (_selectedShotId == 0)
            {
                MessageBox.Show("Please select a shot to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE shot SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                shot_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedShotId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Shot deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadShotsAsync();
                            btnClearShot_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting shot: " + ex.Message); }
            }
        }

        private void btnClearShot_Click(object sender, EventArgs e)
        {
            _selectedShotId = 0;
            nudShotMinute.Value = 0;
            nudShotLocationX.Value = 0;
            nudShotLocationY.Value = 0;
            chkbIsOnTarget.Checked = false;
            chkbIsGoal.Checked = false;

            if (cmbShotMatch.Items.Count > 0) cmbShotMatch.SelectedIndex = 0;
            if (cmbShotTeam.Items.Count > 0) cmbShotTeam.SelectedIndex = 0;
            if (cmbShotPlayer.Items.Count > 0) cmbShotPlayer.SelectedIndex = 0;
            if (cmbShotBodyPart.Items.Count > 0) cmbShotBodyPart.SelectedIndex = 0;

            dgvShot.ClearSelection();
        }

        private void dgvShot_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvShot.Rows[e.RowIndex];

                    // Ignore New Row
                    if (row.IsNewRow) return;

                    // Check ID
                    if (row.Cells["shot_id"].Value == null || row.Cells["shot_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedShotId = Convert.ToInt32(row.Cells["shot_id"].Value);

                    // Map Combos
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbShotMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbShotTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbShotPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    // Map Body Part
                    if (row.Cells["body_part"].Value != null && row.Cells["body_part"].Value != DBNull.Value)
                        cmbShotBodyPart.SelectedItem = row.Cells["body_part"].Value.ToString();

                    // Map Numerics
                    if (row.Cells["minute"].Value != null && row.Cells["minute"].Value != DBNull.Value)
                        nudShotMinute.Value = Convert.ToDecimal(row.Cells["minute"].Value);

                    if (row.Cells["location_x"].Value != null && row.Cells["location_x"].Value != DBNull.Value)
                        nudShotLocationX.Value = Convert.ToDecimal(row.Cells["location_x"].Value);

                    if (row.Cells["location_y"].Value != null && row.Cells["location_y"].Value != DBNull.Value)
                        nudShotLocationY.Value = Convert.ToDecimal(row.Cells["location_y"].Value);

                    // Map Checkboxes
                    if (row.Cells["is_on_target"].Value != DBNull.Value)
                        chkbIsOnTarget.Checked = Convert.ToBoolean(row.Cells["is_on_target"].Value);
                    else
                        chkbIsOnTarget.Checked = false;

                    if (row.Cells["is_goal"].Value != DBNull.Value)
                        chkbIsGoal.Checked = Convert.ToBoolean(row.Cells["is_goal"].Value);
                    else
                        chkbIsGoal.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting shot: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }







        // 1. Load Matches
        private async Task LoadMatchesForFoulComboAsync()
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

                        cmbFoulMatch.DataSource = dt;
                        cmbFoulMatch.DisplayMember = "display_name";
                        cmbFoulMatch.ValueMember = "match_id";
                        cmbFoulMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 2. Load Teams (For Foulling AND Fouled)
        private async Task LoadTeamsForFoulCombosAsync()
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
                        DataTable dtFoulling = new DataTable();
                        dtFoulling.Load(reader);

                        // Clone for the second combobox
                        DataTable dtFouled = dtFoulling.Copy();

                        // Add default rows
                        DataRow row1 = dtFoulling.NewRow();
                        row1["team_id"] = -1;
                        row1["name"] = "-- Foulling Team --";
                        dtFoulling.Rows.InsertAt(row1, 0);

                        DataRow row2 = dtFouled.NewRow();
                        row2["team_id"] = -1;
                        row2["name"] = "-- Fouled Team --";
                        dtFouled.Rows.InsertAt(row2, 0);

                        // Bind Foulling Team
                        cmbFoulFoulingTeam.DataSource = dtFoulling;
                        cmbFoulFoulingTeam.DisplayMember = "name";
                        cmbFoulFoulingTeam.ValueMember = "team_id";
                        cmbFoulFoulingTeam.SelectedIndex = 0;

                        // Bind Fouled Team
                        cmbFouledTeam.DataSource = dtFouled;
                        cmbFouledTeam.DisplayMember = "name";
                        cmbFouledTeam.ValueMember = "team_id";
                        cmbFouledTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Players (For Foulling AND Fouled)
        private async Task LoadPlayersForFoulCombosAsync()
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
                        DataTable dtFoulling = new DataTable();
                        dtFoulling.Load(reader);

                        DataTable dtFouled = dtFoulling.Copy();

                        // Add Default Rows
                        DataRow row1 = dtFoulling.NewRow();
                        row1["player_id"] = -1;
                        row1["full_name"] = "-- Foulling Player --";
                        dtFoulling.Rows.InsertAt(row1, 0);

                        DataRow row2 = dtFouled.NewRow();
                        row2["player_id"] = -1;
                        row2["full_name"] = "-- Fouled Player --";
                        dtFouled.Rows.InsertAt(row2, 0);

                        // Bind Foulling Player
                        cmbFoulingPlayer.DataSource = dtFoulling;
                        cmbFoulingPlayer.DisplayMember = "full_name";
                        cmbFoulingPlayer.ValueMember = "player_id";
                        cmbFoulingPlayer.SelectedIndex = 0;

                        // Bind Fouled Player
                        cmbFouledPlayer.DataSource = dtFouled;
                        cmbFouledPlayer.DisplayMember = "full_name";
                        cmbFouledPlayer.ValueMember = "player_id";
                        cmbFouledPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadFoulsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            f.foul_id, 
            f.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            f.fouling_team_id,
            teamA.name AS foulling_team,
            f.fouled_team_id,
            teamB.name AS fouled_team,
            f.fouling_player_id,
            (p1.first_name || ' ' || p1.last_name) AS foulling_player,
            f.fouled_player_id,
            (p2.first_name || ' ' || p2.last_name) AS fouled_player,
            f.minute,
            f.is_penalty_kick
        FROM foul f
        INNER JOIN ""match"" m ON f.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team teamA ON f.fouling_team_id = teamA.team_id
        INNER JOIN team teamB ON f.fouled_team_id = teamB.team_id
        INNER JOIN player p1 ON f.fouling_player_id = p1.player_id
        INNER JOIN player p2 ON f.fouled_player_id = p2.player_id
        WHERE f.is_active = true
        ORDER BY m.match_date DESC, f.minute";

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

                        dgvFoul.DataSource = dt;

                        // Hide FK columns, keep foul_id visible
                        string[] hiddenCols = { "match_id", "fouling_team_id", "fouled_team_id", "fouling_player_id", "fouled_player_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvFoul.Columns[col] != null)
                                dgvFoul.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading fouls: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddFoul_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbFoulMatch.SelectedValue == -1 ||
                (int)cmbFoulFoulingTeam.SelectedValue == -1 ||
                (int)cmbFouledTeam.SelectedValue == -1 ||
                (int)cmbFoulingPlayer.SelectedValue == -1 ||
                (int)cmbFouledPlayer.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int matchId = (int)cmbFoulMatch.SelectedValue;
            int foullingTeam = (int)cmbFoulFoulingTeam.SelectedValue;
            int fouledTeam = (int)cmbFouledTeam.SelectedValue;
            int foullingPlayer = (int)cmbFoulingPlayer.SelectedValue;
            int fouledPlayer = (int)cmbFouledPlayer.SelectedValue;
            int minute = (int)nudFoulMinute.Value;
            bool isPenalty = chkbIsPenaltyKick.Checked;

            // Logic Check
            if (foullingTeam == fouledTeam)
            {
                MessageBox.Show("Fouling Team and Fouled Team cannot be the same.", "Logic Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO foul 
        (match_id, fouling_team_id, fouled_team_id, fouling_player_id, fouled_player_id, minute, is_penalty_kick, created_by) 
        VALUES 
        (@matchId, @team1, @team2, @p1, @p2, @min, @isPen, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@team1", foullingTeam);
                    command.Parameters.AddWithValue("@team2", fouledTeam);
                    command.Parameters.AddWithValue("@p1", foullingPlayer);
                    command.Parameters.AddWithValue("@p2", fouledPlayer);
                    command.Parameters.AddWithValue("@min", minute);
                    command.Parameters.AddWithValue("@isPen", isPenalty);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Foul added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadFoulsAsync();
                        btnClearFoul_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateFoul_Click(object sender, EventArgs e)
        {
            if (_selectedFoulId == 0)
            {
                MessageBox.Show("Please select a foul to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbFoulMatch.SelectedValue == -1 ||
                (int)cmbFoulFoulingTeam.SelectedValue == -1 ||
                (int)cmbFouledTeam.SelectedValue == -1 ||
                (int)cmbFoulingPlayer.SelectedValue == -1 ||
                (int)cmbFouledPlayer.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE foul SET 
            match_id = @matchId, 
            fouling_team_id = @team1, 
            fouled_team_id = @team2, 
            fouling_player_id = @p1, 
            fouled_player_id = @p2, 
            minute = @min, 
            is_penalty_kick = @isPen,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            foul_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbFoulMatch.SelectedValue);
                    command.Parameters.AddWithValue("@team1", (int)cmbFoulFoulingTeam.SelectedValue);
                    command.Parameters.AddWithValue("@team2", (int)cmbFouledTeam.SelectedValue);
                    command.Parameters.AddWithValue("@p1", (int)cmbFoulingPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@p2", (int)cmbFouledPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@min", (int)nudFoulMinute.Value);
                    command.Parameters.AddWithValue("@isPen", chkbIsPenaltyKick.Checked);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedFoulId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Foul updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadFoulsAsync();
                        btnClearFoul_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating foul: " + ex.Message); }
        }

        private async void btnDeleteFoul_Click(object sender, EventArgs e)
        {
            if (_selectedFoulId == 0)
            {
                MessageBox.Show("Please select a foul to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE foul SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                foul_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedFoulId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Foul deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadFoulsAsync();
                            btnClearFoul_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting foul: " + ex.Message); }
            }
        }

        private void dgvFoul_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvFoul.Rows[e.RowIndex];

                    // Ignore New Row
                    if (row.IsNewRow) return;

                    // Check ID
                    if (row.Cells["foul_id"].Value == null || row.Cells["foul_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedFoulId = Convert.ToInt32(row.Cells["foul_id"].Value);

                    // Map Combos
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbFoulMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["foulling_team_id"].Value != null && row.Cells["foulling_team_id"].Value != DBNull.Value)
                        cmbFoulFoulingTeam.SelectedValue = Convert.ToInt32(row.Cells["foulling_team_id"].Value);

                    if (row.Cells["fouled_team_id"].Value != null && row.Cells["fouled_team_id"].Value != DBNull.Value)
                        cmbFouledTeam.SelectedValue = Convert.ToInt32(row.Cells["fouled_team_id"].Value);

                    if (row.Cells["foulling_player_id"].Value != null && row.Cells["foulling_player_id"].Value != DBNull.Value)
                        cmbFoulingPlayer.SelectedValue = Convert.ToInt32(row.Cells["foulling_player_id"].Value);

                    if (row.Cells["fouled_player_id"].Value != null && row.Cells["fouled_player_id"].Value != DBNull.Value)
                        cmbFouledPlayer.SelectedValue = Convert.ToInt32(row.Cells["fouled_player_id"].Value);

                    // Map Numeric
                    if (row.Cells["minute"].Value != null && row.Cells["minute"].Value != DBNull.Value)
                        nudFoulMinute.Value = Convert.ToDecimal(row.Cells["minute"].Value);

                    // Map Checkbox
                    if (row.Cells["is_penalty_kick"].Value != DBNull.Value)
                        chkbIsPenaltyKick.Checked = Convert.ToBoolean(row.Cells["is_penalty_kick"].Value);
                    else
                        chkbIsPenaltyKick.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting foul: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearFoul_Click(object sender, EventArgs e)
        {
            _selectedFoulId = 0;
            nudFoulMinute.Value = 0;
            chkbIsPenaltyKick.Checked = false;

            if (cmbFoulMatch.Items.Count > 0) cmbFoulMatch.SelectedIndex = 0;
            if (cmbFoulFoulingTeam.Items.Count > 0) cmbFoulFoulingTeam.SelectedIndex = 0;
            if (cmbFouledTeam.Items.Count > 0) cmbFouledTeam.SelectedIndex = 0;
            if (cmbFoulingPlayer.Items.Count > 0) cmbFoulingPlayer.SelectedIndex = 0;
            if (cmbFouledPlayer.Items.Count > 0) cmbFouledPlayer.SelectedIndex = 0;

            dgvFoul.ClearSelection();
        }



        // 1. Initialize Card Type (Fixed)
        private void InitializeCardTypeCombo()
        {
            cmbCardType.Items.Clear();
            cmbCardType.Items.Add("Yellow");
            cmbCardType.Items.Add("Red");
            cmbCardType.SelectedIndex = 0;
        }

        // 2. Load Matches (Formatted: "Team A vs Team B")
        private async Task LoadMatchesForCardComboAsync()
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

                        cmbCardMatch.DataSource = dt;
                        cmbCardMatch.DisplayMember = "display_name";
                        cmbCardMatch.ValueMember = "match_id";
                        cmbCardMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 3. Load Teams
        private async Task LoadTeamsForCardComboAsync()
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

                        cmbCardTeam.DataSource = dt;
                        cmbCardTeam.DisplayMember = "name";
                        cmbCardTeam.ValueMember = "team_id";
                        cmbCardTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 4. Load Players
        private async Task LoadPlayersForCardComboAsync()
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

                        cmbCardPlayer.DataSource = dt;
                        cmbCardPlayer.DisplayMember = "full_name";
                        cmbCardPlayer.ValueMember = "player_id";
                        cmbCardPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 5. Load Main Grid (ID VISIBLE)
        private async Task LoadCardsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            c.card_id, 
            c.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            c.team_id,
            t.name AS team_name,
            c.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            c.card_type,
            c.minute,
            c.reason
        FROM card c
        INNER JOIN ""match"" m ON c.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team t ON c.team_id = t.team_id
        INNER JOIN player p ON c.player_id = p.player_id
        WHERE c.is_active = true
        ORDER BY m.match_date DESC, c.minute";

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

                        dgvCard.DataSource = dt;

                        // NOTE: Keeping card_id visible as requested.
                        // Hiding only Foreign Keys
                        string[] hiddenCols = { "match_id", "team_id", "player_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvCard.Columns[col] != null)
                                dgvCard.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading cards: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddCard_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbCardMatch.SelectedValue == -1 ||
                (int)cmbCardTeam.SelectedValue == -1 ||
                (int)cmbCardPlayer.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbCardType.SelectedItem == null)
            {
                MessageBox.Show("Please select a Card Type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbCardMatch.SelectedValue;
            int teamId = (int)cmbCardTeam.SelectedValue;
            int playerId = (int)cmbCardPlayer.SelectedValue;
            string type = cmbCardType.SelectedItem.ToString();
            int minute = (int)nudCardMinute.Value;
            string reason = txtCardReason.Text.Trim();

            string query = @"
        INSERT INTO card 
        (match_id, team_id, player_id, card_type, minute, reason, created_by) 
        VALUES 
        (@matchId, @teamId, @playerId, @type, @minute, @reason, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@type", type);
                    command.Parameters.AddWithValue("@minute", minute);
                    command.Parameters.AddWithValue("@reason", reason);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Card added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCardsAsync();
                        btnClearCard_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateCard_Click(object sender, EventArgs e)
        {
            if (_selectedCardId == 0)
            {
                MessageBox.Show("Please select a card to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbCardMatch.SelectedValue == -1 ||
                (int)cmbCardTeam.SelectedValue == -1 ||
                (int)cmbCardPlayer.SelectedValue == -1 ||
                cmbCardType.SelectedItem == null)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE card SET 
            match_id = @matchId, 
            team_id = @teamId, 
            player_id = @playerId, 
            card_type = @type, 
            minute = @minute, 
            reason = @reason,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            card_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbCardMatch.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbCardTeam.SelectedValue);
                    command.Parameters.AddWithValue("@playerId", (int)cmbCardPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@type", cmbCardType.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@minute", (int)nudCardMinute.Value);
                    command.Parameters.AddWithValue("@reason", txtCardReason.Text.Trim());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedCardId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Card updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCardsAsync();
                        btnClearCard_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating card: " + ex.Message); }
        }

        private async void btnDeleteCard_Click(object sender, EventArgs e)
        {
            if (_selectedCardId == 0)
            {
                MessageBox.Show("Please select a card to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE card SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                card_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedCardId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Card deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadCardsAsync();
                            btnClearCard_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting card: " + ex.Message); }
            }
        }

        private void dgvCard_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvCard.Rows[e.RowIndex];

                    // Ignore New Row
                    if (row.IsNewRow) return;

                    // Check ID
                    if (row.Cells["card_id"].Value == null || row.Cells["card_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedCardId = Convert.ToInt32(row.Cells["card_id"].Value);

                    // Map Combos
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbCardMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbCardTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbCardPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    // Map Type
                    if (row.Cells["card_type"].Value != null && row.Cells["card_type"].Value != DBNull.Value)
                        cmbCardType.SelectedItem = row.Cells["card_type"].Value.ToString();

                    // Map Numeric
                    if (row.Cells["minute"].Value != null && row.Cells["minute"].Value != DBNull.Value)
                        nudCardMinute.Value = Convert.ToDecimal(row.Cells["minute"].Value);

                    // Map Text
                    txtCardReason.Text = row.Cells["reason"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting card: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearCard_Click(object sender, EventArgs e)
        {
            _selectedCardId = 0;
            nudCardMinute.Value = 0;
            txtCardReason.Clear();

            if (cmbCardMatch.Items.Count > 0) cmbCardMatch.SelectedIndex = 0;
            if (cmbCardTeam.Items.Count > 0) cmbCardTeam.SelectedIndex = 0;
            if (cmbCardPlayer.Items.Count > 0) cmbCardPlayer.SelectedIndex = 0;
            if (cmbCardType.Items.Count > 0) cmbCardType.SelectedIndex = 0;

            dgvCard.ClearSelection();
        }






        // 1. Load Matches (Formatted: "Team A vs Team B")
        private async Task LoadMatchesForSubstitutionComboAsync()
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

                        cmbSubstitutionMatch.DataSource = dt;
                        cmbSubstitutionMatch.DisplayMember = "display_name";
                        cmbSubstitutionMatch.ValueMember = "match_id";
                        cmbSubstitutionMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 2. Load Teams
        private async Task LoadTeamsForSubstitutionComboAsync()
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

                        cmbSubstitutionTeam.DataSource = dt;
                        cmbSubstitutionTeam.DisplayMember = "name";
                        cmbSubstitutionTeam.ValueMember = "team_id";
                        cmbSubstitutionTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Players (For Player IN and Player OUT)
        private async Task LoadPlayersForSubstitutionCombosAsync()
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
                        // We need separate DataTables for IN and OUT combos
                        DataTable dtIn = new DataTable();
                        dtIn.Load(reader);

                        DataTable dtOut = dtIn.Copy();

                        // Add default rows
                        DataRow rowIn = dtIn.NewRow();
                        rowIn["player_id"] = -1;
                        rowIn["full_name"] = "-- Player IN --";
                        dtIn.Rows.InsertAt(rowIn, 0);

                        DataRow rowOut = dtOut.NewRow();
                        rowOut["player_id"] = -1;
                        rowOut["full_name"] = "-- Player OUT --";
                        dtOut.Rows.InsertAt(rowOut, 0);

                        // Bind Player IN
                        cmbSubstitutionPlayerIn.DataSource = dtIn;
                        cmbSubstitutionPlayerIn.DisplayMember = "full_name";
                        cmbSubstitutionPlayerIn.ValueMember = "player_id";
                        cmbSubstitutionPlayerIn.SelectedIndex = 0;

                        // Bind Player OUT
                        cmbSubstitutionPlayerOut.DataSource = dtOut;
                        cmbSubstitutionPlayerOut.DisplayMember = "full_name";
                        cmbSubstitutionPlayerOut.ValueMember = "player_id";
                        cmbSubstitutionPlayerOut.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 4. Load Main Grid (ID VISIBLE)
        private async Task LoadSubstitutionsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            s.substitution_id, 
            s.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            s.team_id,
            t.name AS team_name,
            s.player_in_id,
            (p1.first_name || ' ' || p1.last_name) AS player_in,
            s.player_out_id,
            (p2.first_name || ' ' || p2.last_name) AS player_out,
            s.minute,
            s.reason
        FROM substitution s
        INNER JOIN ""match"" m ON s.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team t ON s.team_id = t.team_id
        INNER JOIN player p1 ON s.player_in_id = p1.player_id
        INNER JOIN player p2 ON s.player_out_id = p2.player_id
        WHERE s.is_active = true
        ORDER BY m.match_date DESC, s.minute";

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

                        dgvSubstitution.DataSource = dt;

                        // NOTE: substitution_id is VISIBLE.
                        // We only hide the FK columns.
                        string[] hiddenCols = { "match_id", "team_id", "player_in_id", "player_out_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvSubstitution.Columns[col] != null)
                                dgvSubstitution.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading substitutions: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddSubstitution_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbSubstitutionMatch.SelectedValue == -1 ||
                (int)cmbSubstitutionTeam.SelectedValue == -1 ||
                (int)cmbSubstitutionPlayerIn.SelectedValue == -1 ||
                (int)cmbSubstitutionPlayerOut.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int matchId = (int)cmbSubstitutionMatch.SelectedValue;
            int teamId = (int)cmbSubstitutionTeam.SelectedValue;
            int playerIn = (int)cmbSubstitutionPlayerIn.SelectedValue;
            int playerOut = (int)cmbSubstitutionPlayerOut.SelectedValue;
            int minute = (int)nudSubstitutionMinute.Value;
            string reason = txtSubstitutionReason.Text.Trim();

            // Logic Check
            if (playerIn == playerOut)
            {
                MessageBox.Show("Player IN cannot be the same as Player OUT.", "Logic Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO substitution 
        (match_id, team_id, player_in_id, player_out_id, minute, reason, created_by) 
        VALUES 
        (@matchId, @teamId, @pIn, @pOut, @min, @reason, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@pIn", playerIn);
                    command.Parameters.AddWithValue("@pOut", playerOut);
                    command.Parameters.AddWithValue("@min", minute);
                    command.Parameters.AddWithValue("@reason", reason);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Substitution added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSubstitutionsAsync();
                        btnClearSubstitution_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateSubstitution_Click(object sender, EventArgs e)
        {
            if (_selectedSubstitutionId == 0)
            {
                MessageBox.Show("Please select a substitution to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbSubstitutionMatch.SelectedValue == -1 ||
                (int)cmbSubstitutionTeam.SelectedValue == -1 ||
                (int)cmbSubstitutionPlayerIn.SelectedValue == -1 ||
                (int)cmbSubstitutionPlayerOut.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE substitution SET 
            match_id = @matchId, 
            team_id = @teamId, 
            player_in_id = @pIn, 
            player_out_id = @pOut, 
            minute = @min, 
            reason = @reason,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            substitution_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbSubstitutionMatch.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbSubstitutionTeam.SelectedValue);
                    command.Parameters.AddWithValue("@pIn", (int)cmbSubstitutionPlayerIn.SelectedValue);
                    command.Parameters.AddWithValue("@pOut", (int)cmbSubstitutionPlayerOut.SelectedValue);
                    command.Parameters.AddWithValue("@min", (int)nudSubstitutionMinute.Value);
                    command.Parameters.AddWithValue("@reason", txtSubstitutionReason.Text.Trim());
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedSubstitutionId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Substitution updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSubstitutionsAsync();
                        btnClearSubstitution_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating substitution: " + ex.Message); }
        }

        private async void btnDeleteSubstitution_Click(object sender, EventArgs e)
        {
            if (_selectedSubstitutionId == 0)
            {
                MessageBox.Show("Please select a substitution to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE substitution SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                substitution_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedSubstitutionId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Substitution deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSubstitutionsAsync();
                            btnClearSubstitution_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting substitution: " + ex.Message); }
            }
        }

        private void dgvSubstitution_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvSubstitution.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    // Check ID
                    if (row.Cells["substitution_id"].Value == null || row.Cells["substitution_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedSubstitutionId = Convert.ToInt32(row.Cells["substitution_id"].Value);

                    // Map Combos (with null checks)
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbSubstitutionMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbSubstitutionTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    if (row.Cells["player_in_id"].Value != null && row.Cells["player_in_id"].Value != DBNull.Value)
                        cmbSubstitutionPlayerIn.SelectedValue = Convert.ToInt32(row.Cells["player_in_id"].Value);

                    if (row.Cells["player_out_id"].Value != null && row.Cells["player_out_id"].Value != DBNull.Value)
                        cmbSubstitutionPlayerOut.SelectedValue = Convert.ToInt32(row.Cells["player_out_id"].Value);

                    // Map Numeric
                    if (row.Cells["minute"].Value != null && row.Cells["minute"].Value != DBNull.Value)
                        nudSubstitutionMinute.Value = Convert.ToDecimal(row.Cells["minute"].Value);

                    // Map Text
                    txtSubstitutionReason.Text = row.Cells["reason"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting substitution: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearSubstitution_Click(object sender, EventArgs e)
        {
            _selectedSubstitutionId = 0;
            nudSubstitutionMinute.Value = 0;
            txtSubstitutionReason.Clear();

            if (cmbSubstitutionMatch.Items.Count > 0) cmbSubstitutionMatch.SelectedIndex = 0;
            if (cmbSubstitutionTeam.Items.Count > 0) cmbSubstitutionTeam.SelectedIndex = 0;
            if (cmbSubstitutionPlayerIn.Items.Count > 0) cmbSubstitutionPlayerIn.SelectedIndex = 0;
            if (cmbSubstitutionPlayerOut.Items.Count > 0) cmbSubstitutionPlayerOut.SelectedIndex = 0;

            dgvSubstitution.ClearSelection();
        }







        // 1. Load Matches (Formatted: "Team A vs Team B")
        private async Task LoadMatchesForStatComboAsync()
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

                        cmbTeamMatchStatMatch.DataSource = dt;
                        cmbTeamMatchStatMatch.DisplayMember = "display_name";
                        cmbTeamMatchStatMatch.ValueMember = "match_id";
                        cmbTeamMatchStatMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 2. Load Teams
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

                        cmbTeamMatchStatTeam.DataSource = dt;
                        cmbTeamMatchStatTeam.DisplayMember = "name";
                        cmbTeamMatchStatTeam.ValueMember = "team_id";
                        cmbTeamMatchStatTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Main Grid (ID VISIBLE)
        private async Task LoadTeamMatchStatsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            tms.team_match_stat_id, 
            tms.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            tms.team_id,
            t.name AS team_name,
            tms.possession_percentage,
            tms.corners,
            tms.offsides
        FROM team_match_stat tms
        INNER JOIN ""match"" m ON tms.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team t ON tms.team_id = t.team_id
        WHERE tms.is_active = true
        ORDER BY m.match_date DESC, t.name";

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

                        dgvTeamMatchStat.DataSource = dt;

                        // NOTE: team_match_stat_id is VISIBLE.
                        // We only hide the Foreign Keys.
                        string[] hiddenCols = { "match_id", "team_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvTeamMatchStat.Columns[col] != null)
                                dgvTeamMatchStat.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading stats: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddTeamMatchStat_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbTeamMatchStatMatch.SelectedValue == -1 ||
                (int)cmbTeamMatchStatTeam.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Match and a Team.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbTeamMatchStatMatch.SelectedValue;
            int teamId = (int)cmbTeamMatchStatTeam.SelectedValue;
            decimal possession = nudPossesionPorcentage.Value;
            int corners = (int)nudCorners.Value;
            int offsides = (int)nudOffsides.Value;

            string query = @"
        INSERT INTO team_match_stat 
        (match_id, team_id, possession_percentage, corners, offsides, created_by) 
        VALUES 
        (@matchId, @teamId, @possession, @corners, @offsides, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@possession", possession);
                    command.Parameters.AddWithValue("@corners", corners);
                    command.Parameters.AddWithValue("@offsides", offsides);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stats added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamMatchStatsAsync();
                        btnClearTeamMatchStat_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("Stats for this team in this match already exist.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateTeamMatchStat_Click(object sender, EventArgs e)
        {
            if (_selectedTeamMatchStatId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbTeamMatchStatMatch.SelectedValue == -1 ||
                (int)cmbTeamMatchStatTeam.SelectedValue == -1)
            {
                MessageBox.Show("All dropdown fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE team_match_stat SET 
            match_id = @matchId, 
            team_id = @teamId, 
            possession_percentage = @possession, 
            corners = @corners, 
            offsides = @offsides,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            team_match_stat_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbTeamMatchStatMatch.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbTeamMatchStatTeam.SelectedValue);
                    command.Parameters.AddWithValue("@possession", nudPossesionPorcentage.Value);
                    command.Parameters.AddWithValue("@corners", (int)nudCorners.Value);
                    command.Parameters.AddWithValue("@offsides", (int)nudOffsides.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedTeamMatchStatId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stats updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadTeamMatchStatsAsync();
                        btnClearTeamMatchStat_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating stats: " + ex.Message); }
        }

        private async void btnDeleteTeamMatchStat_Click(object sender, EventArgs e)
        {
            if (_selectedTeamMatchStatId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE team_match_stat SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                team_match_stat_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedTeamMatchStatId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Stats deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadTeamMatchStatsAsync();
                            btnClearTeamMatchStat_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting stats: " + ex.Message); }
            }
        }

        private void dgvTeamMatchStat_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvTeamMatchStat.Rows[e.RowIndex];

                    // Ignore New Row
                    if (row.IsNewRow) return;

                    // Check ID Null
                    if (row.Cells["team_match_stat_id"].Value == null ||
                        row.Cells["team_match_stat_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedTeamMatchStatId = Convert.ToInt32(row.Cells["team_match_stat_id"].Value);

                    // Map Combos
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbTeamMatchStatMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbTeamMatchStatTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    // Map Numerics
                    if (row.Cells["possession_percentage"].Value != null && row.Cells["possession_percentage"].Value != DBNull.Value)
                        nudPossesionPorcentage.Value = Convert.ToDecimal(row.Cells["possession_percentage"].Value);
                    else
                        nudPossesionPorcentage.Value = 0;

                    if (row.Cells["corners"].Value != null && row.Cells["corners"].Value != DBNull.Value)
                        nudCorners.Value = Convert.ToDecimal(row.Cells["corners"].Value);
                    else
                        nudCorners.Value = 0;

                    if (row.Cells["offsides"].Value != null && row.Cells["offsides"].Value != DBNull.Value)
                        nudOffsides.Value = Convert.ToDecimal(row.Cells["offsides"].Value);
                    else
                        nudOffsides.Value = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting stats: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearTeamMatchStat_Click(object sender, EventArgs e)
        {
            _selectedTeamMatchStatId = 0;
            nudPossesionPorcentage.Value = 0;
            nudCorners.Value = 0;
            nudOffsides.Value = 0;

            if (cmbTeamMatchStatMatch.Items.Count > 0) cmbTeamMatchStatMatch.SelectedIndex = 0;
            if (cmbTeamMatchStatTeam.Items.Count > 0) cmbTeamMatchStatTeam.SelectedIndex = 0;

            dgvTeamMatchStat.ClearSelection();
        }
    }
}
