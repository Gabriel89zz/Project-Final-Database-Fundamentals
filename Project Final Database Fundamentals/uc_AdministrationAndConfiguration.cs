using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Project_Final_Database_Fundamentals
{
    public partial class uc_AdministrationAndConfiguration : UserControl
    {
        private readonly int _adminUserId;
        private int _selectedConfederationId = 0;
        private int _selectedCountryId = 0;

        public uc_AdministrationAndConfiguration(int adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
        }

        private void LoadConfederations()
        {
            // Nota: En Postgres, is_active = true (boolean)
            string query = $"SELECT confederation_id, name, acronym, foundation_year FROM confederation WHERE is_active = true";

            try
            {
                // Usamos DatabaseConnection (Asegúrate que tu clase DatabaseConnection devuelva NpgsqlConnection)
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvConfederations.DataSource = dt;

                    // Bind the DataTable to the ComboBox
                    cmbConfederation.DataSource = dt;
                    cmbConfederation.DisplayMember = "name"; // What the user sees
                    cmbConfederation.ValueMember = "confederation_id";     // The real value behind (the ID)

                    // Set default selection
                    cmbConfederation.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar confederaciones: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddConfederation_Click_1(object sender, EventArgs e)
        {
            string name = txtNameConfederation.Text;
            string acronym = txtAcronym.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(acronym))
            {
                MessageBox.Show("Nombre y Acrónimo son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? foundationYear = null;
            if (!string.IsNullOrEmpty(txtFoundationYear.Text))
            {
                if (int.TryParse(txtFoundationYear.Text, out int year))
                    foundationYear = year;
                else
                {
                    MessageBox.Show("Año inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string query = $@"
            INSERT INTO confederation (name, acronym, foundation_year, created_by) 
            VALUES (@name, @acronym, @foundationYear, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@acronym", acronym);
                    command.Parameters.AddWithValue("@foundationYear", (object)foundationYear ?? DBNull.Value);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Confederación creada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadConfederations();
                        btnClearConfederation_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex) // <--- Excepción específica de Postgres
            {
                // 23505 es el código de error para Unique Violation en Postgres
                if (ex.SqlState == "23505")
                {
                    MessageBox.Show("El Nombre o Acrónimo ya existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Error de base de datos (Postgres): " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabConfederation_Click(object sender, EventArgs e)
        {
            LoadConfederations();
        }

        private void btnUpdateConfederation_Click(object sender, EventArgs e)
        {
            if (_selectedConfederationId == 0)
            {
                MessageBox.Show("Selecciona un registro para actualizar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameConfederation.Text;
            string acronym = txtAcronym.Text;
            int? foundationYear = null;

            if (!string.IsNullOrEmpty(txtFoundationYear.Text))
            {
                if (int.TryParse(txtFoundationYear.Text, out int year))
                    foundationYear = year;
                else
                {
                    MessageBox.Show("Año inválido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // NOTA: Usamos CURRENT_TIMESTAMP en lugar de GETDATE()
            string query = $@"
            UPDATE ""confederation"" SET 
                name = @name, 
                acronym = @acronym, 
                foundation_year = @foundationYear,
                updated_at = CURRENT_TIMESTAMP, 
                updated_by = @updaterId
            WHERE 
                confederation_id = @confederationId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@acronym", acronym);
                    command.Parameters.AddWithValue("@foundationYear", (object)foundationYear ?? DBNull.Value);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@confederationId", _selectedConfederationId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadConfederations();
                        btnClearConfederation_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                {
                    MessageBox.Show("El Nombre o Acrónimo ya existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Error Postgres: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnClearConfederation_Click(object sender, EventArgs e)
        {
            _selectedConfederationId = 0;
            txtNameConfederation.Clear();
            txtAcronym.Clear();
            txtFoundationYear.Clear();
            dgvConfederations.ClearSelection();
        }

        private void dgvConfederations_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvConfederations.Rows[e.RowIndex];
                _selectedConfederationId = Convert.ToInt32(row.Cells["confederation_id"].Value);
                txtNameConfederation.Text = row.Cells["name"].Value.ToString();
                txtAcronym.Text = row.Cells["acronym"].Value.ToString();
                txtFoundationYear.Text = row.Cells["foundation_year"].Value.ToString();
            }
        }

        private void btnDeleteConfederation_Click(object sender, EventArgs e)
        {
            // 1. Verificar selección
            if (_selectedConfederationId == 0)
            {
                MessageBox.Show("Por favor, selecciona un registro para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Confirmar acción
            var confirmResult = MessageBox.Show(
                "¿Estás seguro de que deseas eliminar este registro?\n(Se marcará como inactivo y dejará de ser visible)",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                // 3. Consulta SQL para "Soft Delete" en PostgreSQL
                // En lugar de borrar, actualizamos el estado y la fecha de modificación
                string query = $@"
            UPDATE ""confederation"" SET 
                is_active = false,              -- Aquí está la clave
                updated_at = CURRENT_TIMESTAMP,  -- Registramos cuándo ocurrió
                updated_by = @updaterId          -- Registramos quién lo hizo
            WHERE 
                confederation_id = @confederationId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@confederationId", _selectedConfederationId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Registro eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // 4. Importante: Recargar la lista para que el item "desaparezca" del grid
                            LoadConfederations();

                            // Limpiar los campos de texto
                            btnClearConfederation_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadCountries()
        {
            // We use a JOIN to show the Confederation Name instead of just the ID number
            string query = @"
            SELECT 
                ""country"".country_id, 
                ""country"".name, 
                ""country"".iso_code, 
                ""country"".confederation_id,
                ""confederation"".name AS confederation_name 
            FROM ""country""
            INNER JOIN ""confederation"" ON ""country"".confederation_id = ""confederation"".confederation_id
            WHERE ""country"".is_active = true";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvCountries.DataSource = dt;

                    // Hide the raw ID columns if you want the grid to look cleaner
                    if (dgvCountries.Columns["id"] != null) dgvCountries.Columns["id"].Visible = false;
                    if (dgvCountries.Columns["confederation_id"] != null) dgvCountries.Columns["confederation_id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading countries: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddCountry_Click(object sender, EventArgs e)
        {
            string name = txtNameCountry.Text.Trim();
            string isoCode = txtIsoCode.Text.Trim();

            // Validate that a valid confederation is selected
            // (We assume ID -1 is the "-- Select --" option)
            if (cmbConfederation.SelectedValue == null || (int)cmbConfederation.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Confederation.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int confederationId = (int)cmbConfederation.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(isoCode))
            {
                MessageBox.Show("Name and ISO Code are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = $@"
            INSERT INTO ""country"" (name, iso_code, confederation_id, created_by) 
            VALUES (@name, @isoCode, @confederationId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@isoCode", isoCode);
                    command.Parameters.AddWithValue("@confederationId", confederationId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Country added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCountries();
                        btnClearCountry_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                // 23505 = Unique Violation
                if (ex.SqlState == "23505")
                {
                    MessageBox.Show("This Country Name or ISO Code already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void btnClearCountry_Click(object sender, EventArgs e)
        {
            _selectedCountryId = 0;
            txtNameCountry.Clear();
            txtIsoCode.Clear();

            // Reset ComboBox to the first item (usually "-- Select --")
            if (cmbConfederation.Items.Count > 0)
                cmbConfederation.SelectedIndex = 0;

            dgvCountries.ClearSelection();
        }

        private void btnUpdateCountry_Click(object sender, EventArgs e)
        {
            if (_selectedCountryId == 0)
            {
                MessageBox.Show("Please select a country to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameCountry.Text.Trim();
            string isoCode = txtIsoCode.Text.Trim();

            if (cmbConfederation.SelectedValue == null || (int)cmbConfederation.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Confederation.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int confederationId = (int)cmbConfederation.SelectedValue;

            string query = $@"
            UPDATE ""country"" SET 
                name = @name, 
                iso_code = @isoCode, 
                confederation_id = @confederationId,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                country_id = @countryId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@isoCode", isoCode);
                    command.Parameters.AddWithValue("@confederationId", confederationId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@countryId", _selectedCountryId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Country updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCountries();
                        btnClearCountry_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                {
                    MessageBox.Show("This Country Name or ISO Code already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Database Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnDeleteCountry_Click(object sender, EventArgs e)
        {
            if (_selectedCountryId == 0)
            {
                MessageBox.Show("Please select a country to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "Are you sure you want to delete this country?\nIt will be marked as inactive.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = $@"
                UPDATE ""country"" SET 
                    is_active = false,
                    updated_at = CURRENT_TIMESTAMP,
                    updated_by = @updaterId
                WHERE 
                    country_id = @countryId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@countryId", _selectedCountryId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Country deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCountries();
                            btnClearCountry_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting country: " + ex.Message);
                }
            }
        }

        private void dgvCountries_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCountries.Rows[e.RowIndex];

                // 1. Get the Country ID
                _selectedCountryId = Convert.ToInt32(row.Cells["id"].Value);

                // 2. Fill TextBoxes
                txtNameCountry.Text = row.Cells["name"].Value.ToString();
                txtIsoCode.Text = row.Cells["iso_code"].Value.ToString();

                // 3. Set ComboBox Selection
                // We take the 'confederation_id' from the grid and tell the ComboBox to select that Value
                if (row.Cells["confederation_id"].Value != DBNull.Value)
                {
                    cmbConfederation.SelectedValue = Convert.ToInt32(row.Cells["confederation_id"].Value);
                }
            }
        }

        private void tabConfederation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabConfederation.SelectedTab == tabCountries)  // Cambia a tu TabPage
            {
                LoadCountries();
            }
        }
    }
}
