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
    public partial class ucLineupsAndFormations : UserControl
    {
        private readonly int _adminUserId;
        private int _selectedSquadId = 0;
        private int _selectedSquadMemberId = 0;
        private int _selectedSquadStaffId = 0;
        private int _selectedSquadCoachId = 0;
        private int _selectedPositionId = 0;
        private int _selectedMatchLineupId = 0;
        private int _selectedFormationId = 0;
        private int _selectedLineupPlayerId = 0;
        public ucLineupsAndFormations(int adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
        }

        private async void tabControlLineupsAndFormations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlLineupsAndFormations.SelectedTab == tabPageSquad)
            {
                // Parallel Loading (Grid + 2 Combos)
                var tGrid = LoadSquadsAsync();
                var tTeam = LoadTeamsForSquadComboAsync();
                var tSeason = LoadSeasonsForSquadComboAsync();

                await Task.WhenAll(tGrid, tTeam, tSeason);
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPageSquadMember)
            {
                // Parallel Loading (Grid + 2 Combos)
                var tGrid = LoadSquadMembersAsync();
                var tSquad = LoadSquadsForMemberComboAsync();
                var tPlayer = LoadPlayersForMemberComboAsync();

                await Task.WhenAll(tGrid, tSquad, tPlayer);
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPageSquadStaff)
            {
                // Parallel Loading
                var tGrid = LoadSquadStaffAsync();
                var tSquad = LoadSquadsForStaffComboAsync();
                var tMember = LoadStaffMembersForComboAsync();

                await Task.WhenAll(tGrid, tSquad, tMember);
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPageSquadCoach)
            {
                // Parallel Loading
                var tGrid = LoadSquadCoachesAsync();
                var tSquad = LoadSquadsForCoachComboAsync();
                var tCoach = LoadCoachesForSquadComboAsync();

                await Task.WhenAll(tGrid, tSquad, tCoach);
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPagePosition)
            {
                await LoadPositionsAsync();
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPageMatchLineup)
            {
                // Parallel Loading (Grid + 4 Combos)
                var tGrid = LoadMatchLineupsAsync();
                var tMatch = LoadMatchesForLineupComboAsync();
                var tTeam = LoadTeamsForLineupComboAsync();
                var tCoach = LoadCoachesForLineupComboAsync();
                var tForm = LoadFormationsForLineupComboAsync();

                await Task.WhenAll(tGrid, tMatch, tTeam, tCoach, tForm);
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPageFormation)
            {
                await LoadFormationsAsync();
            }
            else if (tabControlLineupsAndFormations.SelectedTab == tabPageLineupPlayer)
            {
                // Parallel Loading (Grid + 3 Combos)
                var tGrid = LoadLineupPlayersAsync();
                var tLineup = LoadLineupsForPlayerComboAsync();
                var tPlayer = LoadPlayersForLineupComboAsync();
                var tPos = LoadPositionsForLineupComboAsync();

                await Task.WhenAll(tGrid, tLineup, tPlayer, tPos);
            }
        }







        // 1. Load Teams into ComboBox
        private async Task LoadTeamsForSquadComboAsync()
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

                        cmbSquadTeam.DataSource = dt;
                        cmbSquadTeam.DisplayMember = "name";
                        cmbSquadTeam.ValueMember = "team_id";
                        cmbSquadTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 2. Load Seasons into ComboBox
        private async Task LoadSeasonsForSquadComboAsync()
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

                        cmbSquadSeason.DataSource = dt;
                        cmbSquadSeason.DisplayMember = "name";
                        cmbSquadSeason.ValueMember = "season_id";
                        cmbSquadSeason.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading seasons: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadSquadsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            sq.squad_id, 
            sq.team_id,
            t.name AS team_name,
            sq.season_id,
            s.name AS season_name
        FROM squad sq
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        WHERE sq.is_active = true
        ORDER BY t.name, s.name DESC";

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

                        dgvSquad.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "squad_id", "team_id", "season_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvSquad.Columns[col] != null)
                                dgvSquad.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squads: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddSquad_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbSquadTeam.SelectedValue == -1 || (int)cmbSquadSeason.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Team and a Season.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int teamId = (int)cmbSquadTeam.SelectedValue;
            int seasonId = (int)cmbSquadSeason.SelectedValue;

            string query = @"
        INSERT INTO squad (team_id, season_id, created_by) 
        VALUES (@teamId, @seasonId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@seasonId", seasonId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadsAsync();
                        btnClearSquad_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Squad already exists (Team + Season).", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateSquad_Click(object sender, EventArgs e)
        {
            if (_selectedSquadId == 0)
            {
                MessageBox.Show("Please select a squad to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbSquadTeam.SelectedValue == -1 || (int)cmbSquadSeason.SelectedValue == -1)
            {
                MessageBox.Show("Combobox selections are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE squad SET 
            team_id = @teamId, 
            season_id = @seasonId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            squad_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@teamId", (int)cmbSquadTeam.SelectedValue);
                    command.Parameters.AddWithValue("@seasonId", (int)cmbSquadSeason.SelectedValue);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedSquadId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadsAsync();
                        btnClearSquad_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating squad: " + ex.Message); }
        }

        private async void btnDeleteSquad_Click(object sender, EventArgs e)
        {
            if (_selectedSquadId == 0)
            {
                MessageBox.Show("Please select a squad to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                // CAMBIOS AQUÍ: Apuntamos a los campos de eliminación
                string query = @"
     UPDATE squad SET 
         is_active = false,             
         deleted_at = CURRENT_TIMESTAMP, 
         deleted_by = @deleterId        
     WHERE 
         squad_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        // Pasamos el ID del usuario al nuevo parámetro @deleterId
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedSquadId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Squad deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // RECUERDA: Tu método LoadSquadsAsync debe tener en su SELECT:
                            // "WHERE deleted_at IS NULL" para que desaparezca de la lista visualmente.
                            await LoadSquadsAsync();

                            btnClearSquad_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting squad: " + ex.Message); }
            }
        }

        private void btnClearSquad_Click(object sender, EventArgs e)
        {
            _selectedSquadId = 0;

            if (cmbSquadTeam.Items.Count > 0) cmbSquadTeam.SelectedIndex = 0;
            if (cmbSquadSeason.Items.Count > 0) cmbSquadSeason.SelectedIndex = 0;

            dgvSquad.ClearSelection();
        }

        private void dgvSquad_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // 1. Valid Row
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvSquad.Rows[e.RowIndex];

                    // 2. New Row
                    if (row.IsNewRow) return;

                    // 3. Null ID Check
                    if (row.Cells["squad_id"].Value == null || row.Cells["squad_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedSquadId = Convert.ToInt32(row.Cells["squad_id"].Value);

                    // Map Combos (Null Safe)
                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbSquadTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    if (row.Cells["season_id"].Value != null && row.Cells["season_id"].Value != DBNull.Value)
                        cmbSquadSeason.SelectedValue = Convert.ToInt32(row.Cells["season_id"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting squad: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }









        // 1. Load Squads (Formatted: "Team Name - Season Name")
        private async Task LoadSquadsForMemberComboAsync()
        {
            string query = @"
        SELECT 
            sq.squad_id, 
            t.name || ' - ' || s.name AS display_name
        FROM squad sq
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        WHERE sq.is_active = true
        ORDER BY t.name, s.name DESC";

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
                        row["squad_id"] = -1;
                        row["display_name"] = "-- Select Squad --";
                        dt.Rows.InsertAt(row, 0);

                        cmbSquadMemberSquad.DataSource = dt;
                        cmbSquadMemberSquad.DisplayMember = "display_name";
                        cmbSquadMemberSquad.ValueMember = "squad_id";
                        cmbSquadMemberSquad.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squads: " + ex.Message); }
        }

        // 2. Load Players (Formatted: "First Name Last Name")
        private async Task LoadPlayersForMemberComboAsync()
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

                        cmbSquadMemberPlayer.DataSource = dt;
                        cmbSquadMemberPlayer.DisplayMember = "full_name";
                        cmbSquadMemberPlayer.ValueMember = "player_id";
                        cmbSquadMemberPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadSquadMembersAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            sm.squad_member_id, 
            sm.squad_id,
            (t.name || ' - ' || s.name) AS squad_display,
            sm.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            sm.jersey_number,
            sm.join_date,
            sm.leave_date
        FROM squad_member sm
        INNER JOIN squad sq ON sm.squad_id = sq.squad_id
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        INNER JOIN player p ON sm.player_id = p.player_id
        WHERE sm.is_active = true
        ORDER BY t.name, sm.jersey_number";

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

                        dgvSquadMember.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "squad_member_id", "squad_id", "player_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvSquadMember.Columns[col] != null)
                                dgvSquadMember.Columns[col].Visible = false;
                        }

                        // Format Date Columns
                        if (dgvSquadMember.Columns["join_date"] != null)
                            dgvSquadMember.Columns["join_date"].DefaultCellStyle.Format = "d";
                        if (dgvSquadMember.Columns["leave_date"] != null)
                            dgvSquadMember.Columns["leave_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squad members: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddSquadMember_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbSquadMemberSquad.SelectedValue == -1 || (int)cmbSquadMemberPlayer.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Squad and a Player.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int squadId = (int)cmbSquadMemberSquad.SelectedValue;
            int playerId = (int)cmbSquadMemberPlayer.SelectedValue;
            int jersey = (int)nudSquadMemberJerseyNumber.Value;
            DateTime joinDate = dtpSquadMemberJoinDate.Value;

            // Handle Leave Date (Optional logic: if unchecked/default, maybe send NULL, 
            // but for simplicity assuming DateTimePicker always has value)
            DateTime leaveDate = dtpSquadMemberLeaveDate.Value;

            // Logic check: Leave date shouldn't be before Join date
            if (leaveDate < joinDate)
            {
                MessageBox.Show("Leave Date cannot be before Join Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO squad_member 
        (squad_id, player_id, jersey_number, join_date, leave_date, created_by) 
        VALUES 
        (@squadId, @playerId, @jersey, @join, @leave, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@squadId", squadId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@jersey", jersey);
                    command.Parameters.AddWithValue("@join", joinDate);
                    command.Parameters.AddWithValue("@leave", leaveDate);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad Member added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadMembersAsync();
                        btnClearSquadMember_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Player is already in this Squad (or Jersey conflict).", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateSquadMember_Click(object sender, EventArgs e)
        {
            if (_selectedSquadMemberId == 0)
            {
                MessageBox.Show("Please select a member to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbSquadMemberSquad.SelectedValue == -1 || (int)cmbSquadMemberPlayer.SelectedValue == -1)
            {
                MessageBox.Show("Comboboxes are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int squadId = (int)cmbSquadMemberSquad.SelectedValue;
            int playerId = (int)cmbSquadMemberPlayer.SelectedValue;
            int jersey = (int)nudSquadMemberJerseyNumber.Value;
            DateTime joinDate = dtpSquadMemberJoinDate.Value;
            DateTime leaveDate = dtpSquadMemberLeaveDate.Value;

            string query = @"
        UPDATE squad_member SET 
            squad_id = @squadId, 
            player_id = @playerId, 
            jersey_number = @jersey, 
            join_date = @join, 
            leave_date = @leave,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            squad_member_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@squadId", squadId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@jersey", jersey);
                    command.Parameters.AddWithValue("@join", joinDate);
                    command.Parameters.AddWithValue("@leave", leaveDate);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedSquadMemberId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad Member updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadMembersAsync();
                        btnClearSquadMember_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating member: " + ex.Message); }
        }

        private async void btnDeleteSquadMember_Click(object sender, EventArgs e)
        {
            if (_selectedSquadMemberId == 0)
            {
                MessageBox.Show("Please select a member to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE squad_member SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            squad_member_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedSquadMemberId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Squad Member deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSquadMembersAsync();
                            btnClearSquadMember_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting member: " + ex.Message); }
            }
        }

        private void dgvSquadMember_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvSquadMember.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["squad_member_id"].Value == null || row.Cells["squad_member_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedSquadMemberId = Convert.ToInt32(row.Cells["squad_member_id"].Value);

                    // Map Combos
                    if (row.Cells["squad_id"].Value != null && row.Cells["squad_id"].Value != DBNull.Value)
                        cmbSquadMemberSquad.SelectedValue = Convert.ToInt32(row.Cells["squad_id"].Value);

                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbSquadMemberPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    // Map Numeric
                    if (row.Cells["jersey_number"].Value != null && row.Cells["jersey_number"].Value != DBNull.Value)
                        nudSquadMemberJerseyNumber.Value = Convert.ToDecimal(row.Cells["jersey_number"].Value);
                    else
                        nudSquadMemberJerseyNumber.Value = 0;

                    // Map Dates
                    if (row.Cells["join_date"].Value != null && row.Cells["join_date"].Value != DBNull.Value)
                        dtpSquadMemberJoinDate.Value = Convert.ToDateTime(row.Cells["join_date"].Value);

                    if (row.Cells["leave_date"].Value != null && row.Cells["leave_date"].Value != DBNull.Value)
                        dtpSquadMemberLeaveDate.Value = Convert.ToDateTime(row.Cells["leave_date"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting member: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearSquadMember_Click(object sender, EventArgs e)
        {
            _selectedSquadMemberId = 0;
            nudSquadMemberJerseyNumber.Value = 0;
            dtpSquadMemberJoinDate.Value = DateTime.Now;
            dtpSquadMemberLeaveDate.Value = DateTime.Now;

            if (cmbSquadMemberSquad.Items.Count > 0) cmbSquadMemberSquad.SelectedIndex = 0;
            if (cmbSquadMemberPlayer.Items.Count > 0) cmbSquadMemberPlayer.SelectedIndex = 0;

            dgvSquadMember.ClearSelection();
        }









        // 1. Load Squads (Formatted: "Team - Season")
        private async Task LoadSquadsForStaffComboAsync()
        {
            string query = @"
        SELECT 
            sq.squad_id, 
            t.name || ' - ' || s.name AS display_name
        FROM squad sq
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        WHERE sq.is_active = true
        ORDER BY t.name, s.name DESC";

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
                        row["squad_id"] = -1;
                        row["display_name"] = "-- Select Squad --";
                        dt.Rows.InsertAt(row, 0);

                        cmbSquadStaffSquad.DataSource = dt;
                        cmbSquadStaffSquad.DisplayMember = "display_name";
                        cmbSquadStaffSquad.ValueMember = "squad_id";
                        cmbSquadStaffSquad.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squads: " + ex.Message); }
        }

        // 2. Load Staff Members (Formatted: "First Last")
        private async Task LoadStaffMembersForComboAsync()
        {
            string query = @"
        SELECT 
            staff_member_id, 
            first_name || ' ' || last_name AS full_name 
        FROM staff_member 
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
                        row["staff_member_id"] = -1;
                        row["full_name"] = "-- Select Staff Member --";
                        dt.Rows.InsertAt(row, 0);

                        cmbSquadStaffMember.DataSource = dt;
                        cmbSquadStaffMember.DisplayMember = "full_name";
                        cmbSquadStaffMember.ValueMember = "staff_member_id";
                        cmbSquadStaffMember.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading staff members: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadSquadStaffAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            ss.squad_staff_id, 
            ss.squad_id,
            (t.name || ' - ' || s.name) AS squad_display,
            ss.staff_member_id,
            (sm.first_name || ' ' || sm.last_name) AS staff_name,
            ss.start_date,
            ss.end_date
        FROM squad_staff ss
        INNER JOIN squad sq ON ss.squad_id = sq.squad_id
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        INNER JOIN staff_member sm ON ss.staff_member_id = sm.staff_member_id
        WHERE ss.is_active = true
        ORDER BY t.name, sm.last_name";

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

                        dgvSquadStaff.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "squad_staff_id", "squad_id", "staff_member_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvSquadStaff.Columns[col] != null)
                                dgvSquadStaff.Columns[col].Visible = false;
                        }

                        // Format Date Columns
                        if (dgvSquadStaff.Columns["start_date"] != null)
                            dgvSquadStaff.Columns["start_date"].DefaultCellStyle.Format = "d";
                        if (dgvSquadStaff.Columns["end_date"] != null)
                            dgvSquadStaff.Columns["end_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squad staff: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddSquadStaff_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbSquadStaffSquad.SelectedValue == -1 || (int)cmbSquadStaffMember.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Squad and a Staff Member.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int squadId = (int)cmbSquadStaffSquad.SelectedValue;
            int staffId = (int)cmbSquadStaffMember.SelectedValue;
            DateTime startDate = dtpSquadStaffStartDate.Value;
            DateTime endDate = dtpSqudStaffEndDate.Value;

            // 3. Validate Dates
            if (endDate < startDate)
            {
                MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO squad_staff 
        (squad_id, staff_member_id, start_date, end_date, created_by) 
        VALUES 
        (@squadId, @staffId, @start, @end, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@squadId", squadId);
                    command.Parameters.AddWithValue("@staffId", staffId);
                    command.Parameters.AddWithValue("@start", startDate);
                    command.Parameters.AddWithValue("@end", endDate);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad Staff added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadStaffAsync();
                        btnClearSquadStaff_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Staff Member is already assigned to this Squad.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateSquadStaff_Click(object sender, EventArgs e)
        {
            if (_selectedSquadStaffId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbSquadStaffSquad.SelectedValue == -1 || (int)cmbSquadStaffMember.SelectedValue == -1)
            {
                MessageBox.Show("Comboboxes are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime startDate = dtpSquadStaffStartDate.Value;
            DateTime endDate = dtpSqudStaffEndDate.Value;

            if (endDate < startDate)
            {
                MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE squad_staff SET 
            squad_id = @squadId, 
            staff_member_id = @staffId, 
            start_date = @start, 
            end_date = @end,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            squad_staff_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@squadId", (int)cmbSquadStaffSquad.SelectedValue);
                    command.Parameters.AddWithValue("@staffId", (int)cmbSquadStaffMember.SelectedValue);
                    command.Parameters.AddWithValue("@start", startDate);
                    command.Parameters.AddWithValue("@end", endDate);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedSquadStaffId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadStaffAsync();
                        btnClearSquadStaff_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteSquadStaff_Click(object sender, EventArgs e)
        {
            if (_selectedSquadStaffId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE squad_staff SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            squad_staff_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedSquadStaffId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSquadStaffAsync();
                            btnClearSquadStaff_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvSquadStaff_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvSquadStaff.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["squad_staff_id"].Value == null || row.Cells["squad_staff_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedSquadStaffId = Convert.ToInt32(row.Cells["squad_staff_id"].Value);

                    // Map Combos
                    if (row.Cells["squad_id"].Value != null && row.Cells["squad_id"].Value != DBNull.Value)
                        cmbSquadStaffSquad.SelectedValue = Convert.ToInt32(row.Cells["squad_id"].Value);

                    if (row.Cells["staff_member_id"].Value != null && row.Cells["staff_member_id"].Value != DBNull.Value)
                        cmbSquadStaffMember.SelectedValue = Convert.ToInt32(row.Cells["staff_member_id"].Value);

                    // Map Dates
                    if (row.Cells["start_date"].Value != null && row.Cells["start_date"].Value != DBNull.Value)
                        dtpSquadStaffStartDate.Value = Convert.ToDateTime(row.Cells["start_date"].Value);

                    if (row.Cells["end_date"].Value != null && row.Cells["end_date"].Value != DBNull.Value)
                        dtpSqudStaffEndDate.Value = Convert.ToDateTime(row.Cells["end_date"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearSquadStaff_Click(object sender, EventArgs e)
        {
            _selectedSquadStaffId = 0;
            dtpSquadStaffStartDate.Value = DateTime.Now;
            dtpSqudStaffEndDate.Value = DateTime.Now;

            if (cmbSquadStaffSquad.Items.Count > 0) cmbSquadStaffSquad.SelectedIndex = 0;
            if (cmbSquadStaffMember.Items.Count > 0) cmbSquadStaffMember.SelectedIndex = 0;

            dgvSquadStaff.ClearSelection();
        }






        // 1. Load Squads (Formatted: "Team Name - Season")
        private async Task LoadSquadsForCoachComboAsync()
        {
            string query = @"
        SELECT 
            sq.squad_id, 
            t.name || ' - ' || s.name AS display_name
        FROM squad sq
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        WHERE sq.is_active = true
        ORDER BY t.name, s.name DESC";

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
                        row["squad_id"] = -1;
                        row["display_name"] = "-- Select Squad --";
                        dt.Rows.InsertAt(row, 0);

                        cmbSquadCoachSquad.DataSource = dt;
                        cmbSquadCoachSquad.DisplayMember = "display_name";
                        cmbSquadCoachSquad.ValueMember = "squad_id";
                        cmbSquadCoachSquad.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squads: " + ex.Message); }
        }

        // 2. Load Coaches (Formatted: "First Name Last Name")
        private async Task LoadCoachesForSquadComboAsync()
        {
            string query = @"
        SELECT 
            coach_id, 
            first_name || ' ' || last_name AS full_name 
        FROM coach 
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
                        row["coach_id"] = -1;
                        row["full_name"] = "-- Select Coach --";
                        dt.Rows.InsertAt(row, 0);

                        cmbSquadCoachCoach.DataSource = dt;
                        cmbSquadCoachCoach.DisplayMember = "full_name";
                        cmbSquadCoachCoach.ValueMember = "coach_id";
                        cmbSquadCoachCoach.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading coaches: " + ex.Message); }
        }

        // 3. Load Main Grid
        private async Task LoadSquadCoachesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            sc.squad_coach_id, 
            sc.squad_id,
            (t.name || ' - ' || s.name) AS squad_display,
            sc.coach_id,
            (c.first_name || ' ' || c.last_name) AS coach_name,
            sc.start_date,
            sc.end_date
        FROM squad_coach sc
        INNER JOIN squad sq ON sc.squad_id = sq.squad_id
        INNER JOIN team t ON sq.team_id = t.team_id
        INNER JOIN season s ON sq.season_id = s.season_id
        INNER JOIN coach c ON sc.coach_id = c.coach_id
        WHERE sc.is_active = true
        ORDER BY t.name, s.name DESC";

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

                        dgvSquadCoach.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "squad_coach_id", "squad_id", "coach_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvSquadCoach.Columns[col] != null)
                                dgvSquadCoach.Columns[col].Visible = false;
                        }

                        // Format Dates
                        if (dgvSquadCoach.Columns["start_date"] != null)
                            dgvSquadCoach.Columns["start_date"].DefaultCellStyle.Format = "d";
                        if (dgvSquadCoach.Columns["end_date"] != null)
                            dgvSquadCoach.Columns["end_date"].DefaultCellStyle.Format = "d";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading squad coaches: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnAddSquadCoach_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbSquadCoachSquad.SelectedValue == -1 || (int)cmbSquadCoachCoach.SelectedValue == -1)
            {
                MessageBox.Show("Please select a Squad and a Coach.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int squadId = (int)cmbSquadCoachSquad.SelectedValue;
            int coachId = (int)cmbSquadCoachCoach.SelectedValue;
            DateTime startDate = dtpSquadCoachStartDate.Value;
            DateTime endDate = dtpSquaCoachEndDate.Value;

            // 2. Validate Dates
            if (endDate < startDate)
            {
                MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO squad_coach 
        (squad_id, coach_id, start_date, end_date, created_by) 
        VALUES 
        (@squadId, @coachId, @start, @end, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@squadId", squadId);
                    command.Parameters.AddWithValue("@coachId", coachId);
                    command.Parameters.AddWithValue("@start", startDate);
                    command.Parameters.AddWithValue("@end", endDate);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad Coach added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadCoachesAsync();
                        btnClearSquadCoach_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Coach is already assigned to this Squad.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateSquadCoach_Click(object sender, EventArgs e)
        {
            if (_selectedSquadCoachId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbSquadCoachSquad.SelectedValue == -1 || (int)cmbSquadCoachCoach.SelectedValue == -1)
            {
                MessageBox.Show("Comboboxes are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime startDate = dtpSquadCoachStartDate.Value;
            DateTime endDate = dtpSquaCoachEndDate.Value;

            if (endDate < startDate)
            {
                MessageBox.Show("End Date cannot be before Start Date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE squad_coach SET 
            squad_id = @squadId, 
            coach_id = @coachId, 
            start_date = @start, 
            end_date = @end,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            squad_coach_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@squadId", (int)cmbSquadCoachSquad.SelectedValue);
                    command.Parameters.AddWithValue("@coachId", (int)cmbSquadCoachCoach.SelectedValue);
                    command.Parameters.AddWithValue("@start", startDate);
                    command.Parameters.AddWithValue("@end", endDate);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedSquadCoachId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Squad Coach updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSquadCoachesAsync();
                        btnClearSquadCoach_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteSquadCoach_Click(object sender, EventArgs e)
        {
            if (_selectedSquadCoachId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE squad_coach SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            squad_coach_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedSquadCoachId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSquadCoachesAsync();
                            btnClearSquadCoach_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvSquadCoach_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvSquadCoach.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["squad_coach_id"].Value == null || row.Cells["squad_coach_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedSquadCoachId = Convert.ToInt32(row.Cells["squad_coach_id"].Value);

                    // Map Combos
                    if (row.Cells["squad_id"].Value != null && row.Cells["squad_id"].Value != DBNull.Value)
                        cmbSquadCoachSquad.SelectedValue = Convert.ToInt32(row.Cells["squad_id"].Value);

                    if (row.Cells["coach_id"].Value != null && row.Cells["coach_id"].Value != DBNull.Value)
                        cmbSquadCoachCoach.SelectedValue = Convert.ToInt32(row.Cells["coach_id"].Value);

                    // Map Dates
                    if (row.Cells["start_date"].Value != null && row.Cells["start_date"].Value != DBNull.Value)
                        dtpSquadCoachStartDate.Value = Convert.ToDateTime(row.Cells["start_date"].Value);

                    if (row.Cells["end_date"].Value != null && row.Cells["end_date"].Value != DBNull.Value)
                        dtpSquaCoachEndDate.Value = Convert.ToDateTime(row.Cells["end_date"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearSquadCoach_Click(object sender, EventArgs e)
        {
            _selectedSquadCoachId = 0;
            dtpSquadCoachStartDate.Value = DateTime.Now;
            dtpSquaCoachEndDate.Value = DateTime.Now;

            if (cmbSquadCoachSquad.Items.Count > 0) cmbSquadCoachSquad.SelectedIndex = 0;
            if (cmbSquadCoachCoach.Items.Count > 0) cmbSquadCoachCoach.SelectedIndex = 0;

            dgvSquadCoach.ClearSelection();
        }







        // --- ASYNC: Load Positions ---
        private async Task LoadPositionsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // NOTE: "position" must be quoted because it is a SQL reserved keyword
            string query = @"
        SELECT 
            position_id, 
            name, 
            acronym, 
            category 
        FROM ""position""
        WHERE is_active = true
        ORDER BY name";

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

                        dgvPosition.DataSource = dt;

                        // Hide internal ID
                        if (dgvPosition.Columns["position_id"] != null)
                            dgvPosition.Columns["position_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading positions: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddPosition_Click(object sender, EventArgs e)
        {
            string name = txtPositionName.Text.Trim();
            string acronym = txtPositionAcronym.Text.Trim();
            string category = txtPositionCategory.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(acronym) || string.IsNullOrEmpty(category))
            {
                MessageBox.Show("All fields (Name, Acronym, Category) are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO ""position"" (name, acronym, category, created_by) 
        VALUES (@name, @acronym, @category, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@acronym", acronym);
                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Position added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPositionsAsync();
                        btnClearPosition_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Position Name or Acronym already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdatePosition_Click(object sender, EventArgs e)
        {
            if (_selectedPositionId == 0)
            {
                MessageBox.Show("Please select a position to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtPositionName.Text.Trim();
            string acronym = txtPositionAcronym.Text.Trim();
            string category = txtPositionCategory.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(acronym) || string.IsNullOrEmpty(category))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE ""position"" SET 
            name = @name, 
            acronym = @acronym, 
            category = @category,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            position_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@acronym", acronym);
                    command.Parameters.AddWithValue("@category", category);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedPositionId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Position updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadPositionsAsync();
                        btnClearPosition_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating position: " + ex.Message); }
        }

        private async void btnDeletePosition_Click(object sender, EventArgs e)
        {
            if (_selectedPositionId == 0)
            {
                MessageBox.Show("Please select a position to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE ""position"" SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            position_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedPositionId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Position deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadPositionsAsync();
                            btnClearPosition_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting position: " + ex.Message); }
            }
        }

        private void dgvPosition_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvPosition.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["position_id"].Value == null || row.Cells["position_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedPositionId = Convert.ToInt32(row.Cells["position_id"].Value);

                    // Map Texts (Null Safe)
                    txtPositionName.Text = row.Cells["name"].Value?.ToString() ?? "";
                    txtPositionAcronym.Text = row.Cells["acronym"].Value?.ToString() ?? "";
                    txtPositionCategory.Text = row.Cells["category"].Value?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting position: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearPosition_Click(object sender, EventArgs e)
        {
            _selectedPositionId = 0;
            txtPositionName.Clear();
            txtPositionAcronym.Clear();
            txtPositionCategory.Clear();

            dgvPosition.ClearSelection();
        }






        // 1. Load Matches (Formatted: "Team A vs Team B")
        private async Task LoadMatchesForLineupComboAsync()
        {
            // Assuming 'match' table has home_team_id and away_team_id
            // We join to get names to make the dropdown readable
            string query = @"
        SELECT 
            m.match_id, 
            t1.name || ' vs ' || t2.name AS display_name
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

                        cmbMatchLineupMatch.DataSource = dt;
                        cmbMatchLineupMatch.DisplayMember = "display_name";
                        cmbMatchLineupMatch.ValueMember = "match_id";
                        cmbMatchLineupMatch.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading matches: " + ex.Message); }
        }

        // 2. Load Teams
        private async Task LoadTeamsForLineupComboAsync()
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

                        cmbMatchLineupTeam.DataSource = dt;
                        cmbMatchLineupTeam.DisplayMember = "name";
                        cmbMatchLineupTeam.ValueMember = "team_id";
                        cmbMatchLineupTeam.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading teams: " + ex.Message); }
        }

        // 3. Load Coaches
        private async Task LoadCoachesForLineupComboAsync()
        {
            string query = @"
        SELECT 
            coach_id, 
            first_name || ' ' || last_name AS full_name 
        FROM coach 
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
                        row["coach_id"] = -1;
                        row["full_name"] = "-- Select Coach --";
                        dt.Rows.InsertAt(row, 0);

                        cmbMatchLineupCoach.DataSource = dt;
                        cmbMatchLineupCoach.DisplayMember = "full_name";
                        cmbMatchLineupCoach.ValueMember = "coach_id";
                        cmbMatchLineupCoach.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading coaches: " + ex.Message); }
        }

        // 4. Load Formations
        private async Task LoadFormationsForLineupComboAsync()
        {
            // Assuming table 'formation' exists
            string query = @"SELECT formation_id, name FROM formation WHERE is_active = true ORDER BY name";

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
                        row["formation_id"] = -1;
                        row["name"] = "-- Select Formation --";
                        dt.Rows.InsertAt(row, 0);

                        cmbMatchLineupFormation.DataSource = dt;
                        cmbMatchLineupFormation.DisplayMember = "name";
                        cmbMatchLineupFormation.ValueMember = "formation_id";
                        cmbMatchLineupFormation.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading formations: " + ex.Message); }
        }

        // 5. Load Main Grid
        private async Task LoadMatchLineupsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            ml.match_lineup_id, 
            ml.match_id,
            (t1.name || ' vs ' || t2.name) AS match_display,
            ml.team_id,
            t.name AS team_name,
            ml.coach_id,
            (c.first_name || ' ' || c.last_name) AS coach_name,
            ml.formation_id,
            f.name AS formation_name
        FROM match_lineup ml
        INNER JOIN match m ON ml.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        INNER JOIN team t ON ml.team_id = t.team_id
        INNER JOIN coach c ON ml.coach_id = c.coach_id
        INNER JOIN formation f ON ml.formation_id = f.formation_id
        WHERE ml.is_active = true
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

                        dgvMatchLineup.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "match_lineup_id", "match_id", "team_id", "coach_id", "formation_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvMatchLineup.Columns[col] != null)
                                dgvMatchLineup.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading lineups: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }

        private async void btnAddMatchLineup_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbMatchLineupMatch.SelectedValue == -1 ||
                (int)cmbMatchLineupTeam.SelectedValue == -1 ||
                (int)cmbMatchLineupCoach.SelectedValue == -1 ||
                (int)cmbMatchLineupFormation.SelectedValue == -1)
            {
                MessageBox.Show("All selection fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int matchId = (int)cmbMatchLineupMatch.SelectedValue;
            int teamId = (int)cmbMatchLineupTeam.SelectedValue;
            int coachId = (int)cmbMatchLineupCoach.SelectedValue;
            int formationId = (int)cmbMatchLineupFormation.SelectedValue;

            string query = @"
        INSERT INTO match_lineup 
        (match_id, team_id, coach_id, formation_id, created_by) 
        VALUES 
        (@matchId, @teamId, @coachId, @formationId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", matchId);
                    command.Parameters.AddWithValue("@teamId", teamId);
                    command.Parameters.AddWithValue("@coachId", coachId);
                    command.Parameters.AddWithValue("@formationId", formationId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Match Lineup added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchLineupsAsync();
                        btnClearMatchLineup_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Team already has a lineup for this Match.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateMatchLineup_Click(object sender, EventArgs e)
        {
            if (_selectedMatchLineupId == 0)
            {
                MessageBox.Show("Please select a lineup to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbMatchLineupMatch.SelectedValue == -1 ||
                (int)cmbMatchLineupTeam.SelectedValue == -1 ||
                (int)cmbMatchLineupCoach.SelectedValue == -1 ||
                (int)cmbMatchLineupFormation.SelectedValue == -1)
            {
                MessageBox.Show("All selection fields are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE match_lineup SET 
            match_id = @matchId, 
            team_id = @teamId, 
            coach_id = @coachId, 
            formation_id = @formationId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            match_lineup_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@matchId", (int)cmbMatchLineupMatch.SelectedValue);
                    command.Parameters.AddWithValue("@teamId", (int)cmbMatchLineupTeam.SelectedValue);
                    command.Parameters.AddWithValue("@coachId", (int)cmbMatchLineupCoach.SelectedValue);
                    command.Parameters.AddWithValue("@formationId", (int)cmbMatchLineupFormation.SelectedValue);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedMatchLineupId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Lineup updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadMatchLineupsAsync();
                        btnClearMatchLineup_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating lineup: " + ex.Message); }
        }

        private async void btnDeleteMatchLineup_Click(object sender, EventArgs e)
        {
            if (_selectedMatchLineupId == 0)
            {
                MessageBox.Show("Please select a lineup to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
        UPDATE match_lineup SET 
            is_active = false,
            deleted_at = CURRENT_TIMESTAMP,
            deleted_by = @deleterId
        WHERE 
            match_lineup_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedMatchLineupId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Lineup deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadMatchLineupsAsync();
                            btnClearMatchLineup_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting lineup: " + ex.Message); }
            }
        }

        private void dgvMatchLineup_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvMatchLineup.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["match_lineup_id"].Value == null || row.Cells["match_lineup_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedMatchLineupId = Convert.ToInt32(row.Cells["match_lineup_id"].Value);

                    // Map Combos with null checks
                    if (row.Cells["match_id"].Value != null && row.Cells["match_id"].Value != DBNull.Value)
                        cmbMatchLineupMatch.SelectedValue = Convert.ToInt32(row.Cells["match_id"].Value);

                    if (row.Cells["team_id"].Value != null && row.Cells["team_id"].Value != DBNull.Value)
                        cmbMatchLineupTeam.SelectedValue = Convert.ToInt32(row.Cells["team_id"].Value);

                    if (row.Cells["coach_id"].Value != null && row.Cells["coach_id"].Value != DBNull.Value)
                        cmbMatchLineupCoach.SelectedValue = Convert.ToInt32(row.Cells["coach_id"].Value);

                    if (row.Cells["formation_id"].Value != null && row.Cells["formation_id"].Value != DBNull.Value)
                        cmbMatchLineupFormation.SelectedValue = Convert.ToInt32(row.Cells["formation_id"].Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting lineup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearMatchLineup_Click(object sender, EventArgs e)
        {
            _selectedMatchLineupId = 0;

            if (cmbMatchLineupMatch.Items.Count > 0) cmbMatchLineupMatch.SelectedIndex = 0;
            if (cmbMatchLineupTeam.Items.Count > 0) cmbMatchLineupTeam.SelectedIndex = 0;
            if (cmbMatchLineupCoach.Items.Count > 0) cmbMatchLineupCoach.SelectedIndex = 0;
            if (cmbMatchLineupFormation.Items.Count > 0) cmbMatchLineupFormation.SelectedIndex = 0;

            dgvMatchLineup.ClearSelection();
        }







        // --- ASYNC: Load Formations ---
        private async Task LoadFormationsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            formation_id, 
            name 
        FROM formation
        WHERE is_active = true
        ORDER BY name";

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

                        dgvFormation.DataSource = dt;

                        // Hide internal ID
                        if (dgvFormation.Columns["formation_id"] != null)
                            dgvFormation.Columns["formation_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading formations: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private async void btnAddFormation_Click(object sender, EventArgs e)
        {
            string name = txtFormationName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Formation Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO formation (name, created_by) 
        VALUES (@name, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Formation added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadFormationsAsync();
                        btnClearFormation_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Formation already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateFormation_Click(object sender, EventArgs e)
        {
            if (_selectedFormationId == 0)
            {
                MessageBox.Show("Please select a formation to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtFormationName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Formation Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE formation SET 
            name = @name, 
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            formation_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedFormationId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Formation updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadFormationsAsync();
                        btnClearFormation_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating formation: " + ex.Message); }
        }

        private async void btnDeleteFormation_Click(object sender, EventArgs e)
        {
            if (_selectedFormationId == 0)
            {
                MessageBox.Show("Please select a formation to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                // CAMBIO AQUÍ: Ahora apuntamos a los campos de 'deleted'
                string query = @"
            UPDATE formation SET 
                is_active = false,               -- Mantenemos el flag booleano (si lo usas)
                deleted_at = CURRENT_TIMESTAMP,  -- Registramos CUÁNDO se borró
                deleted_by = @deleterId          -- Registramos QUIÉN lo borró
            WHERE 
                formation_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        // Pasamos el ID del usuario actual al campo de borrado
                        command.Parameters.AddWithValue("@deleterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedFormationId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Formation deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Recargamos la grilla para que ya no aparezca el registro borrado
                            // (Asegúrate de que tu SELECT en LoadFormationsAsync tenga "WHERE deleted_at IS NULL")
                            await LoadFormationsAsync();

                            btnClearFormation_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting formation: " + ex.Message);
                }
            }
        }

        private void dgvFormation_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvFormation.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["formation_id"].Value == null || row.Cells["formation_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedFormationId = Convert.ToInt32(row.Cells["formation_id"].Value);

                    // Map Name
                    if (row.Cells["name"].Value != null && row.Cells["name"].Value != DBNull.Value)
                    {
                        txtFormationName.Text = row.Cells["name"].Value.ToString();
                    }
                    else
                    {
                        txtFormationName.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting formation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearFormation_Click(object sender, EventArgs e)
        {
            _selectedFormationId = 0;
            txtFormationName.Clear();
            dgvFormation.ClearSelection();
        }







        // 1. Load Match Lineups (Context: "Real Madrid (vs Barcelona)")
        private async Task LoadLineupsForPlayerComboAsync()
        {
            string query = @"
        SELECT 
            ml.match_lineup_id, 
            t.name || ' (vs ' || 
            CASE 
                WHEN m.home_team_id = ml.team_id THEN t2.name 
                ELSE t1.name 
            END || ')' AS display_name
        FROM match_lineup ml
        INNER JOIN team t ON ml.team_id = t.team_id
        INNER JOIN match m ON ml.match_id = m.match_id
        INNER JOIN team t1 ON m.home_team_id = t1.team_id
        INNER JOIN team t2 ON m.away_team_id = t2.team_id
        WHERE ml.is_active = true
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

                        DataRow row = dt.NewRow();
                        row["match_lineup_id"] = -1;
                        row["display_name"] = "-- Select Lineup --";
                        dt.Rows.InsertAt(row, 0);

                        cmbLineupPlayerMatchLineup.DataSource = dt;
                        cmbLineupPlayerMatchLineup.DisplayMember = "display_name";
                        cmbLineupPlayerMatchLineup.ValueMember = "match_lineup_id";
                        cmbLineupPlayerMatchLineup.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading lineups: " + ex.Message); }
        }

        // 2. Load Players
        private async Task LoadPlayersForLineupComboAsync()
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

                        cmbLineupPlayerPlayer.DataSource = dt;
                        cmbLineupPlayerPlayer.DisplayMember = "full_name";
                        cmbLineupPlayerPlayer.ValueMember = "player_id";
                        cmbLineupPlayerPlayer.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading players: " + ex.Message); }
        }

        // 3. Load Positions
        private async Task LoadPositionsForLineupComboAsync()
        {
            // NOTE: "position" is a reserved keyword, use quotes
            string query = @"SELECT position_id, name FROM ""position"" WHERE is_active = true ORDER BY name";

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

                        cmbLineupPlayerPosition.DataSource = dt;
                        cmbLineupPlayerPosition.DisplayMember = "name";
                        cmbLineupPlayerPosition.ValueMember = "position_id";
                        cmbLineupPlayerPosition.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading positions: " + ex.Message); }
        }

        // 4. Load Main Grid
        private async Task LoadLineupPlayersAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            lp.lineup_player_id, 
            lp.match_lineup_id,
            (t.name || ' - Match ' || m.match_id) AS lineup_display,
            lp.player_id,
            (p.first_name || ' ' || p.last_name) AS player_name,
            lp.position_id,
            pos.name AS position_name,
            lp.is_starter,
            lp.is_captain
        FROM lineup_player lp
        INNER JOIN match_lineup ml ON lp.match_lineup_id = ml.match_lineup_id
        INNER JOIN team t ON ml.team_id = t.team_id
        INNER JOIN match m ON ml.match_id = m.match_id
        INNER JOIN player p ON lp.player_id = p.player_id
        INNER JOIN ""position"" pos ON lp.position_id = pos.position_id
        WHERE lp.is_active = true
        ORDER BY t.name, lp.is_starter DESC, p.last_name";

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

                        dgvLineupPlayer.DataSource = dt;

                        // Hide internal IDs
                        string[] hiddenCols = { "lineup_player_id", "match_lineup_id", "player_id", "position_id" };
                        foreach (var col in hiddenCols)
                        {
                            if (dgvLineupPlayer.Columns[col] != null)
                                dgvLineupPlayer.Columns[col].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading lineup players: " + ex.Message); }
            finally { this.Cursor = Cursors.Default; }
        }
        private async void btnLineupPlayer_Click(object sender, EventArgs e)
        {
            // 1. Validate Combos
            if ((int)cmbLineupPlayerMatchLineup.SelectedValue == -1 ||
                (int)cmbLineupPlayerPlayer.SelectedValue == -1 ||
                (int)cmbLineupPlayerPosition.SelectedValue == -1)
            {
                MessageBox.Show("All dropdowns are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Get Values
            int lineupId = (int)cmbLineupPlayerMatchLineup.SelectedValue;
            int playerId = (int)cmbLineupPlayerPlayer.SelectedValue;
            int positionId = (int)cmbLineupPlayerPosition.SelectedValue;
            bool isStarter = chkbIsStarter.Checked;
            bool isCaptain = chkbIsCaptain.Checked;

            string query = @"
        INSERT INTO lineup_player 
        (match_lineup_id, player_id, position_id, is_starter, is_captain, created_by) 
        VALUES 
        (@lineupId, @playerId, @posId, @isStarter, @isCaptain, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@lineupId", lineupId);
                    command.Parameters.AddWithValue("@playerId", playerId);
                    command.Parameters.AddWithValue("@posId", positionId);
                    command.Parameters.AddWithValue("@isStarter", isStarter);
                    command.Parameters.AddWithValue("@isCaptain", isCaptain);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Player added to lineup successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadLineupPlayersAsync();
                        btnClearLineupPlayer_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Player is already in this Lineup.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex) { MessageBox.Show("Unexpected Error: " + ex.Message); }
        }

        private async void btnUpdateLineupPlayer_Click(object sender, EventArgs e)
        {
            if (_selectedLineupPlayerId == 0)
            {
                MessageBox.Show("Please select a record to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if ((int)cmbLineupPlayerMatchLineup.SelectedValue == -1 ||
                (int)cmbLineupPlayerPlayer.SelectedValue == -1 ||
                (int)cmbLineupPlayerPosition.SelectedValue == -1)
            {
                MessageBox.Show("All dropdowns are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE lineup_player SET 
            match_lineup_id = @lineupId, 
            player_id = @playerId, 
            position_id = @posId, 
            is_starter = @isStarter,
            is_captain = @isCaptain,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            lineup_player_id = @id;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@lineupId", (int)cmbLineupPlayerMatchLineup.SelectedValue);
                    command.Parameters.AddWithValue("@playerId", (int)cmbLineupPlayerPlayer.SelectedValue);
                    command.Parameters.AddWithValue("@posId", (int)cmbLineupPlayerPosition.SelectedValue);
                    command.Parameters.AddWithValue("@isStarter", chkbIsStarter.Checked);
                    command.Parameters.AddWithValue("@isCaptain", chkbIsCaptain.Checked);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@id", _selectedLineupPlayerId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Record updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadLineupPlayersAsync();
                        btnClearLineupPlayer_Click(sender, e);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error updating record: " + ex.Message); }
        }

        private async void btnDeleteLineupPlayer_Click(object sender, EventArgs e)
        {
            if (_selectedLineupPlayerId == 0)
            {
                MessageBox.Show("Please select a record to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                // NOTE: Using deleted_at and deleted_by as requested
                string query = @"
            UPDATE lineup_player SET 
                is_active = false,
                deleted_at = CURRENT_TIMESTAMP,
                deleted_by = @updaterId
            WHERE 
                lineup_player_id = @id;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@id", _selectedLineupPlayerId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Record deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadLineupPlayersAsync();
                            btnClearLineupPlayer_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error deleting record: " + ex.Message); }
            }
        }

        private void dgvLineupPlayer_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvLineupPlayer.Rows[e.RowIndex];

                    if (row.IsNewRow) return;

                    if (row.Cells["lineup_player_id"].Value == null || row.Cells["lineup_player_id"].Value == DBNull.Value)
                    {
                        return;
                    }

                    _selectedLineupPlayerId = Convert.ToInt32(row.Cells["lineup_player_id"].Value);

                    // Map Combos
                    if (row.Cells["match_lineup_id"].Value != null && row.Cells["match_lineup_id"].Value != DBNull.Value)
                        cmbLineupPlayerMatchLineup.SelectedValue = Convert.ToInt32(row.Cells["match_lineup_id"].Value);

                    if (row.Cells["player_id"].Value != null && row.Cells["player_id"].Value != DBNull.Value)
                        cmbLineupPlayerPlayer.SelectedValue = Convert.ToInt32(row.Cells["player_id"].Value);

                    if (row.Cells["position_id"].Value != null && row.Cells["position_id"].Value != DBNull.Value)
                        cmbLineupPlayerPosition.SelectedValue = Convert.ToInt32(row.Cells["position_id"].Value);

                    // Map Checkboxes
                    if (row.Cells["is_starter"].Value != DBNull.Value)
                        chkbIsStarter.Checked = Convert.ToBoolean(row.Cells["is_starter"].Value);
                    else
                        chkbIsStarter.Checked = false;

                    if (row.Cells["is_captain"].Value != DBNull.Value)
                        chkbIsCaptain.Checked = Convert.ToBoolean(row.Cells["is_captain"].Value);
                    else
                        chkbIsCaptain.Checked = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClearLineupPlayer_Click(object sender, EventArgs e)
        {
            _selectedLineupPlayerId = 0;
            chkbIsStarter.Checked = false;
            chkbIsCaptain.Checked = false;

            if (cmbLineupPlayerMatchLineup.Items.Count > 0) cmbLineupPlayerMatchLineup.SelectedIndex = 0;
            if (cmbLineupPlayerPlayer.Items.Count > 0) cmbLineupPlayerPlayer.SelectedIndex = 0;
            if (cmbLineupPlayerPosition.Items.Count > 0) cmbLineupPlayerPosition.SelectedIndex = 0;

            dgvLineupPlayer.ClearSelection();
        }
    }
}
