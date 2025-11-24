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
        private int _selectedCityId = 0;
        private int _selectedStadiumId = 0;
        private int _selectedAwardId = 0;
        private int _selectedEventTypeId = 0;
        private int _selectedAgencyId = 0;
        private int _selectedSponsorshipTypeId = 0;
        private int _selectedSponsorId = 0;
        private int _selectedSocialMediaPlatformId = 0;


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
                _selectedCountryId = Convert.ToInt32(row.Cells["country_id"].Value);

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

        private void LoadCities()
        {
            // Join to get the Country Name
            // NOTE: We use full table names in quotes to avoid the 42P01 error
            string query = @"
            SELECT 
                city_id, 
                city.name, 
                city.country_id, 
                country.name AS country_name 
            FROM city
            INNER JOIN country ON city.country_id = country.country_id
            WHERE city.is_active = true
            ORDER BY city_id";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvCity.DataSource = dt;

                    // Hide internal IDs
                    if (dgvCity.Columns["id"] != null) dgvCity.Columns["id"].Visible = false;
                    if (dgvCity.Columns["country_id"] != null) dgvCity.Columns["country_id"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cities: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadCountriesForComboBox()
        {
            // Solo traemos ID y Nombre, ordenados alfabéticamente
            string query = @"SELECT country_id, name FROM ""country"" WHERE is_active = true ORDER BY name";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Crear una fila por defecto "-- Select --"
                    DataRow row = dt.NewRow();
                    row["country_id"] = -1;
                    row["name"] = "-- Select Country --";
                    dt.Rows.InsertAt(row, 0);

                    // Asignar al ComboBox (Asegúrate de que tu ComboBox se llame cmbCountry)
                    cmbCountryCity.DataSource = dt;
                    cmbCountryCity.DisplayMember = "name";
                    cmbCountryCity.ValueMember = "country_id";

                    cmbCountryCity.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading countries list: " + ex.Message);
            }
        }

        private async void tabConfederation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabConfederation.SelectedTab == tabCountries)
            {
                LoadCountries();
            }
            else if (tabConfederation.SelectedTab == tabCity)
            {
                LoadCities();
                LoadCountriesForComboBox();
            }
            else if (tabConfederation.SelectedTab == tabStadium)
            {
                var task1 = LoadStadiumsAsync();
                var task2 = LoadCitiesForComboBoxAsync();

                await Task.WhenAll(task1, task2);
            }
            else if (tabConfederation.SelectedTab == tabAwards)
            {
                await LoadAwardsAsync();
            }
            else if (tabConfederation.SelectedTab == tabEventTypes)
            {
                await LoadEventTypesAsync();
            }
            else if (tabConfederation.SelectedTab == tabAgencies)
            {
                await LoadAgenciesAsync();
                await LoadCountriesForAgencyComboBoxAsync();
            }
            else if (tabConfederation.SelectedTab == tabSponsorshipTypes)
            {
                await LoadSponsorshipTypesAsync();
            }
            else if (tabConfederation.SelectedTab == tabSponsor)
            {
                await LoadSponsorsAsync();
                await LoadCountriesForSponsorComboBoxAsync();
            }
            else if (tabConfederation.SelectedTab == tabSocialMediaPlatform)
            {
                await LoadSocialMediaPlatformsAsync();
            }
        }

        private void btnAddCity_Click(object sender, EventArgs e)
        {
            string name = txtNameCity.Text.Trim(); // Asegúrate de que el TextBox se llame txtNameCity

            // Validar ComboBox
            if (cmbCountryCity.SelectedValue == null || (int)cmbCountryCity.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int countryId = (int)cmbCountryCity.SelectedValue;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("City Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO city (name, country_id, created_by) 
        VALUES (@name, @countryId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("City added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCities();
                        btnClearCity_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                // 23505 = Unique Violation (Si tienes constraint unique en nombre+pais)
                if (ex.SqlState == "23505")
                    MessageBox.Show("This City already exists in the selected Country.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private void btnUpdateCity_Click(object sender, EventArgs e)
        {
            if (_selectedCityId == 0)
            {
                MessageBox.Show("Please select a city to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameCity.Text.Trim();

            if (cmbCountryCity.SelectedValue == null || (int)cmbCountryCity.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int countryId = (int)cmbCountryCity.SelectedValue;

            string query = @"
        UPDATE city SET 
            name = @name, 
            country_id = @countryId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            city_id = @cityId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@cityId", _selectedCityId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("City updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCities();
                        btnClearCity_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating city: " + ex.Message);
            }
        }

        private void btnDeleteCity_Click(object sender, EventArgs e)
        {
            if (_selectedCityId == 0)
            {
                MessageBox.Show("Please select a city to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this city?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE city SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                city_id = @cityId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@cityId", _selectedCityId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("City deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCities();
                            btnClearCity_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting city: " + ex.Message);
                }
            }
        }

        private void dgvCity_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCity.Rows[e.RowIndex];

                // 1. Obtener ID
                _selectedCityId = Convert.ToInt32(row.Cells["city_id"].Value);

                // 2. Llenar Nombre
                txtNameCity.Text = row.Cells["name"].Value.ToString();

                // 3. Seleccionar País en el ComboBox
                if (row.Cells["country_id"].Value != DBNull.Value)
                {
                    cmbCountryCity.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);
                }
            }
        }

        private void btnClearCity_Click(object sender, EventArgs e)
        {
            _selectedCityId = 0;
            txtNameCity.Clear();

            if (cmbCountryCity.Items.Count > 0)
                cmbCountryCity.SelectedIndex = 0; // Volver a "-- Select --"

            dgvCity.ClearSelection();
        }

        // --- ASYNC: Cargar ComboBox de Ciudades ---
        private async Task LoadCitiesForComboBoxAsync()
        {
            //string query = @"SELECT city_id, name FROM city WHERE is_active = true ORDER BY name";
            string query = @"SELECT city_id,city.name
                            FROM city
                            inner join country on city.country_id=country.country_id
                            WHERE country.name IN ('Argentina', 'Mexico', 'Spain', 'Italy', 'United Kingdom', 'France', 'Germany','Japan')
                            AND city.is_active = true ORDER BY city.name;";

            try
            {
                // 1. Conexión asíncrona
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync(); // ¡No bloquea la UI!

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync()) // ¡Consulta asíncrona!
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader); // Carga los datos del lector a la tabla

                        // Agregar fila por defecto
                        DataRow row = dt.NewRow();
                        row["city_id"] = -1;
                        row["name"] = "-- Select City --";
                        dt.Rows.InsertAt(row, 0);

                        // Asignar al ComboBox (Esto debe hacerse en el hilo principal, lo cual es automático aquí)
                        cmbStadiumCity.DisplayMember = "name";
                        cmbStadiumCity.ValueMember = "city_id";
                        cmbStadiumCity.DataSource = dt;

                        cmbStadiumCity.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading cities list: " + ex.Message);
            }
        }

        // --- ASYNC: Cargar Grid de Estadios ---
        private async Task LoadStadiumsAsync()
        {
            // Opcional: Mostrar un cursor de "Cargando"
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            stadium_id, 
            stadium.name, 
            stadium.capacity,
            stadium.city_id, 
            city.name AS city_name 
        FROM stadium
        INNER JOIN city ON stadium.city_id = city.city_id
        WHERE stadium.is_active = true
        ORDER BY stadium_id DESC";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync(); // Espera conexión sin congelar

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync()) // Espera datos sin congelar
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader); // Vuelca los datos en memoria

                        dgvStadiums.DataSource = dt;

                        // Ocultar columnas internas
                        if (dgvStadiums.Columns["stadium_id"] != null) dgvStadiums.Columns["stadium_id"].Visible = false;
                        if (dgvStadiums.Columns["city_id"] != null) dgvStadiums.Columns["city_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stadiums: " + ex.Message);
            }
            finally
            {
                // Restaurar el cursor normal pase lo que pase
                this.Cursor = Cursors.Default;
            }
        }

        private void btnAddStadium_Click(object sender, EventArgs e)
        {
            string name = txtNameStadium.Text.Trim();
            string capacityStr = txtCapacity.Text.Trim();

            // 1. Validate ComboBox
            if (cmbStadiumCity.SelectedValue == null || (int)cmbStadiumCity.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid City.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int cityId = (int)cmbStadiumCity.SelectedValue;

            // 2. Validate Strings
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(capacityStr))
            {
                MessageBox.Show("Name and Capacity are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Validate Integer (Capacity)
            if (!int.TryParse(capacityStr, out int capacity))
            {
                MessageBox.Show("Capacity must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO stadium (name, capacity, city_id, created_by) 
        VALUES (@name, @capacity, @cityId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@capacity", capacity);
                    command.Parameters.AddWithValue("@cityId", cityId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stadium added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadStadiumsAsync();
                        btnClearStadium_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Stadium already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private void btnUpdateStadium_Click(object sender, EventArgs e)
        {
            if (_selectedStadiumId == 0)
            {
                MessageBox.Show("Please select a stadium to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameStadium.Text.Trim();
            string capacityStr = txtCapacity.Text.Trim();

            if (cmbStadiumCity.SelectedValue == null || (int)cmbStadiumCity.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid City.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int cityId = (int)cmbStadiumCity.SelectedValue;

            if (!int.TryParse(capacityStr, out int capacity))
            {
                MessageBox.Show("Capacity must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE stadium SET 
            name = @name, 
            capacity = @capacity,
            city_id = @cityId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            stadium_id = @stadiumId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@capacity", capacity);
                    command.Parameters.AddWithValue("@cityId", cityId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@stadiumId", _selectedStadiumId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Stadium updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadStadiumsAsync();
                        btnClearStadium_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating stadium: " + ex.Message);
            }
        }

        private void btnDeleteStadium_Click(object sender, EventArgs e)
        {
            if (_selectedStadiumId == 0)
            {
                MessageBox.Show("Please select a stadium to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this stadium?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE stadium SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                stadium_id = @stadiumId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@stadiumId", _selectedStadiumId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Stadium deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadStadiumsAsync();
                            btnClearStadium_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting stadium: " + ex.Message);
                }
            }
        }

        private void btnClearStadium_Click(object sender, EventArgs e)
        {
            _selectedStadiumId = 0;
            txtNameStadium.Clear();
            txtCapacity.Clear();

            if (cmbStadiumCity.Items.Count > 0)
                cmbStadiumCity.SelectedIndex = 0;

            dgvStadiums.ClearSelection();
        }

        private void dgvStadiums_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStadiums.Rows[e.RowIndex];

                // 1. Get ID
                _selectedStadiumId = Convert.ToInt32(row.Cells["stadium_id"].Value);

                // 2. Fill TextFields
                txtNameStadium.Text = row.Cells["name"].Value.ToString();
                txtCapacity.Text = row.Cells["capacity"].Value.ToString();

                // 3. Set ComboBox Selection
                if (row.Cells["city_id"].Value != DBNull.Value)
                {
                    cmbStadiumCity.SelectedValue = Convert.ToInt32(row.Cells["city_id"].Value);
                }
            }
        }
        private async Task LoadAwardsAsync()
        {
            // Opcional: Change cursor to indicate loading
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            award_id, 
            name, 
            scope 
        FROM award
        WHERE is_active = true
        ORDER BY award_id DESC";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync(); // Non-blocking connection

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync()) // Non-blocking query
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        dgvAwards.DataSource = dt;

                        // Hide internal ID
                        if (dgvAwards.Columns["award_id"] != null) dgvAwards.Columns["award_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading awards: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddAward_Click(object sender, EventArgs e)
        {
            string name = txtNameAward.Text.Trim();
            string scope = txtScope.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(scope))
            {
                MessageBox.Show("Name and Scope are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO award (name, scope, created_by) 
        VALUES (@name, @scope, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@scope", scope);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery(); // Insert is fast, usually synchronous is fine here, but keep logic consistent

                    if (result > 0)
                    {
                        MessageBox.Show("Award added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAwardsAsync(); // Refresh Grid Asynchronously
                        btnClearAward_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding award: " + ex.Message);
            }
        }

        private async void btnUpdateAward_Click(object sender, EventArgs e)
        {
            if (_selectedAwardId == 0)
            {
                MessageBox.Show("Please select an award to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameAward.Text.Trim();
            string scope = txtScope.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(scope))
            {
                MessageBox.Show("Name and Scope are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE award SET 
            name = @name, 
            scope = @scope,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            award_id = @awardId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@scope", scope);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@awardId", _selectedAwardId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Award updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAwardsAsync(); // Refresh Grid Asynchronously
                        btnClearAward_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating award: " + ex.Message);
            }
        }

        private async void btnDeleteAward_Click(object sender, EventArgs e)
        {
            if (_selectedAwardId == 0)
            {
                MessageBox.Show("Please select an award to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this award?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE award SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                award_id = @awardId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@awardId", _selectedAwardId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Award deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadAwardsAsync(); // Refresh Grid Asynchronously
                            btnClearAward_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting award: " + ex.Message);
                }
            }
        }

        private void btnClearAward_Click(object sender, EventArgs e)
        {
            _selectedAwardId = 0;
            txtNameAward.Clear();
            txtScope.Clear();
            dgvAwards.ClearSelection();
        }

        private void dgvAwards_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvAwards.Rows[e.RowIndex];

                _selectedAwardId = Convert.ToInt32(row.Cells["award_id"].Value);
                txtNameAward.Text = row.Cells["name"].Value.ToString();
                txtScope.Text = row.Cells["scope"].Value.ToString();
            }
        }

        private async Task LoadEventTypesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // Standard query fetching ID and Name
            string query = @"
        SELECT 
            event_type_id, 
            name 
        FROM event_type
        WHERE is_active = true
        ORDER BY event_type_id DESC";

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

                        dgvEventTypes.DataSource = dt;

                        // Hide the ID column
                        if (dgvEventTypes.Columns["event_type_id"] != null)
                            dgvEventTypes.Columns["event_type_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading event types: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddEventType_Click(object sender, EventArgs e)
        {
            string name = txtNameEventType.Text.Trim(); // Ensure TextBox is named txtNameEventType

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO event_type (name, created_by) 
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
                        MessageBox.Show("Event Type added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadEventTypesAsync(); // Async refresh
                        btnClerEventType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") // Unique violation
                    MessageBox.Show("This Event Type name already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateEventType_Click(object sender, EventArgs e)
        {
            if (_selectedEventTypeId == 0)
            {
                MessageBox.Show("Please select an event type to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameEventType.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE event_type SET 
            name = @name, 
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            event_type_id = @eventTypeId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@eventTypeId", _selectedEventTypeId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Event Type updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadEventTypesAsync();
                        btnClerEventType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Event Type name already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating event type: " + ex.Message);
            }
        }

        private async void btnDeleteEventType_Click(object sender, EventArgs e)
        {
            if (_selectedEventTypeId == 0)
            {
                MessageBox.Show("Please select an event type to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this event type?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE event_type SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                event_type_id = @eventTypeId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@eventTypeId", _selectedEventTypeId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Event Type deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadEventTypesAsync();
                            btnClerEventType_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting event type: " + ex.Message);
                }
            }
        }

        private void btnClerEventType_Click(object sender, EventArgs e)
        {
            _selectedEventTypeId = 0;
            txtNameEventType.Clear();
            dgvEventTypes.ClearSelection();
        }

        private void dgvEventTypes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEventTypes.Rows[e.RowIndex];

                _selectedEventTypeId = Convert.ToInt32(row.Cells["event_type_id"].Value);
                txtNameEventType.Text = row.Cells["name"].Value.ToString();
            }
        }

        private async Task LoadCountriesForAgencyComboBoxAsync()
        {
            // Reuse the same query logic to fetch active countries
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

                        // Default row
                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        // Bind to the specific ComboBox for Agencies
                        // Ensure your ComboBox in the UI is named 'cmbAgencyCountry'
                        cmbAgencyCountry.DataSource = dt;
                        cmbAgencyCountry.DisplayMember = "name";
                        cmbAgencyCountry.ValueMember = "country_id";

                        cmbAgencyCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading countries for agency: " + ex.Message);
            }
        }

        private async Task LoadAgenciesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // JOIN to get Country Name
            string query = @"
        SELECT 
            agency.agency_id, 
            agency.name, 
            agency.country_id, 
            country.name AS country_name 
        FROM agency
        INNER JOIN ""country"" ON agency.country_id = ""country"".country_id
        WHERE agency.is_active = true
        ORDER BY agency.agency_id DESC";

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

                        dgvAgencies.DataSource = dt;

                        // Hide internal IDs
                        if (dgvAgencies.Columns["agency_id"] != null) dgvAgencies.Columns["agency_id"].Visible = false;
                        if (dgvAgencies.Columns["country_id"] != null) dgvAgencies.Columns["country_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading agencies: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddAgency_Click(object sender, EventArgs e)
        {
            string name = txtNameAgency.Text.Trim(); // TextBox for Agency Name

            // 1. Validate ComboBox
            if (cmbAgencyCountry.SelectedValue == null || (int)cmbAgencyCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int countryId = (int)cmbAgencyCountry.SelectedValue;

            // 2. Validate Name
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Agency Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO agency (name, country_id, created_by) 
        VALUES (@name, @countryId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Agency added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAgenciesAsync(); // Async refresh
                        btnClearAgency_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Agency already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateAgency_Click(object sender, EventArgs e)
        {
            if (_selectedAgencyId == 0)
            {
                MessageBox.Show("Please select an agency to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameAgency.Text.Trim();

            if (cmbAgencyCountry.SelectedValue == null || (int)cmbAgencyCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int countryId = (int)cmbAgencyCountry.SelectedValue;

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Agency Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE agency SET 
            name = @name, 
            country_id = @countryId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            agency_id = @agencyId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@agencyId", _selectedAgencyId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Agency updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadAgenciesAsync();
                        btnClearAgency_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating agency: " + ex.Message);
            }
        }

        private async void btnDeleteAgency_Click(object sender, EventArgs e)
        {
            if (_selectedAgencyId == 0)
            {
                MessageBox.Show("Please select an agency to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this agency?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE agency SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                agency_id = @agencyId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@agencyId", _selectedAgencyId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Agency deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadAgenciesAsync();
                            btnClearAgency_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting agency: " + ex.Message);
                }
            }
        }

        private async void btnClearAgency_Click(object sender, EventArgs e)
        {
            _selectedAgencyId = 0;
            txtNameAgency.Clear();

            if (cmbAgencyCountry.Items.Count > 0)
                cmbAgencyCountry.SelectedIndex = 0;

            dgvAgencies.ClearSelection();
        }

        private void dgvAgencies_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvAgencies.Rows[e.RowIndex];

                // 1. Get ID
                _selectedAgencyId = Convert.ToInt32(row.Cells["agency_id"].Value);

                // 2. Fill Name
                txtNameAgency.Text = row.Cells["name"].Value.ToString();

                // 3. Set ComboBox Selection
                if (row.Cells["country_id"].Value != DBNull.Value)
                {
                    cmbAgencyCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);
                }
            }
        }
        private async Task LoadSponsorshipTypesAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            sponsorship_type_id, 
            type_name 
        FROM sponsorship_type
        WHERE is_active = true
        ORDER BY sponsorship_type_id DESC";

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

                        dgvSponsorshipTypes.DataSource = dt;

                        // Hide the ID column
                        if (dgvSponsorshipTypes.Columns["sponsorship_type_id"] != null)
                            dgvSponsorshipTypes.Columns["sponsorship_type_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sponsorship types: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddSponsorshipType_Click(object sender, EventArgs e)
        {
            string typeName = txtTypeName.Text.Trim(); // Ensure TextBox is named correctly

            if (string.IsNullOrEmpty(typeName))
            {
                MessageBox.Show("Type Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO sponsorship_type (type_name, created_by) 
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
                        MessageBox.Show("Sponsorship Type added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSponsorshipTypesAsync(); // Async refresh
                        btnClearSponsorshipType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") // Unique violation
                    MessageBox.Show("This Sponsorship Type already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateSponsorshipType_Click(object sender, EventArgs e)
        {
            if (_selectedSponsorshipTypeId == 0)
            {
                MessageBox.Show("Please select a sponsorship type to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string typeName = txtTypeName.Text.Trim();

            if (string.IsNullOrEmpty(typeName))
            {
                MessageBox.Show("Type Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE sponsorship_type SET 
            type_name = @typeName, 
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            sponsorship_type_id = @sponsorshipTypeId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@typeName", typeName);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@sponsorshipTypeId", _selectedSponsorshipTypeId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Sponsorship Type updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSponsorshipTypesAsync();
                        btnClearSponsorshipType_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Sponsorship Type already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating sponsorship type: " + ex.Message);
            }
        }

        private async void btnDeleteSponsorshipType_Click(object sender, EventArgs e)
        {
            if (_selectedSponsorshipTypeId == 0)
            {
                MessageBox.Show("Please select a sponsorship type to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this sponsorship type?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE sponsorship_type SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                sponsorship_type_id = @sponsorshipTypeId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@sponsorshipTypeId", _selectedSponsorshipTypeId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Sponsorship Type deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSponsorshipTypesAsync();
                            btnClearSponsorshipType_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting sponsorship type: " + ex.Message);
                }
            }
        }

        private async void btnClearSponsorshipType_Click(object sender, EventArgs e)
        {
            _selectedSponsorshipTypeId = 0;
            txtTypeName.Clear();
            dgvSponsorshipTypes.ClearSelection();
        }

        private void dgvSponsorshipTypes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSponsorshipTypes.Rows[e.RowIndex];

                _selectedSponsorshipTypeId = Convert.ToInt32(row.Cells["sponsorship_type_id"].Value);
                txtTypeName.Text = row.Cells["type_name"].Value.ToString();
            }
        }

        private async Task LoadCountriesForSponsorComboBoxAsync()
        {
            // Fetch active countries
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

                        // Add default "Select" row
                        DataRow row = dt.NewRow();
                        row["country_id"] = -1;
                        row["name"] = "-- Select Country --";
                        dt.Rows.InsertAt(row, 0);

                        // Bind to the specific ComboBox for Sponsors
                        cmbSponsorCountry.DataSource = dt;
                        cmbSponsorCountry.DisplayMember = "name";
                        cmbSponsorCountry.ValueMember = "country_id";

                        cmbSponsorCountry.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading countries for sponsor: " + ex.Message);
            }
        }

        private async Task LoadSponsorsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            // JOIN to get Country Name
            string query = @"
        SELECT 
            s.sponsor_id, 
            s.name, 
            s.industry,
            s.country_id, 
            c.name AS country_name 
        FROM sponsor s
        INNER JOIN ""country"" c ON s.country_id = c.country_id
        WHERE s.is_active = true
        ORDER BY s.sponsor_id DESC";

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

                        dgvSponsors.DataSource = dt;

                        // Hide internal IDs
                        if (dgvSponsors.Columns["sponsor_id"] != null) dgvSponsors.Columns["sponsor_id"].Visible = false;
                        if (dgvSponsors.Columns["country_id"] != null) dgvSponsors.Columns["country_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sponsors: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddSponsor_Click(object sender, EventArgs e)
        {
            string name = txtSponsorName.Text.Trim();
            string industry = txtSponsorIndustry.Text.Trim();

            // 1. Validate ComboBox
            if (cmbSponsorCountry.SelectedValue == null || (int)cmbSponsorCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int countryId = (int)cmbSponsorCountry.SelectedValue;

            // 2. Validate Text Fields
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(industry))
            {
                MessageBox.Show("Name and Industry are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO sponsor (name, industry, country_id, created_by) 
        VALUES (@name, @industry, @countryId, @creatorId);";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@industry", industry);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@creatorId", _adminUserId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Sponsor added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSponsorsAsync(); // Async refresh
                        btnClearSponsor_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Sponsor already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateSponsor_Click(object sender, EventArgs e)
        {
            if (_selectedSponsorId == 0)
            {
                MessageBox.Show("Please select a sponsor to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtSponsorName.Text.Trim();
            string industry = txtSponsorIndustry.Text.Trim();

            if (cmbSponsorCountry.SelectedValue == null || (int)cmbSponsorCountry.SelectedValue == -1)
            {
                MessageBox.Show("Please select a valid Country.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int countryId = (int)cmbSponsorCountry.SelectedValue;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(industry))
            {
                MessageBox.Show("Name and Industry are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE sponsor SET 
            name = @name, 
            industry = @industry,
            country_id = @countryId,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            sponsor_id = @sponsorId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@industry", industry);
                    command.Parameters.AddWithValue("@countryId", countryId);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@sponsorId", _selectedSponsorId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Sponsor updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSponsorsAsync();
                        btnClearSponsor_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating sponsor: " + ex.Message);
            }
        }

        private async void btnDeleteSponsor_Click(object sender, EventArgs e)
        {
            if (_selectedSponsorId == 0)
            {
                MessageBox.Show("Please select a sponsor to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this sponsor?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE sponsor SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                sponsor_id = @sponsorId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@sponsorId", _selectedSponsorId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Sponsor deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSponsorsAsync();
                            btnClearSponsor_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting sponsor: " + ex.Message);
                }
            }
        }

        private void dgvSponsors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSponsors.Rows[e.RowIndex];

                // 1. Get ID
                _selectedSponsorId = Convert.ToInt32(row.Cells["sponsor_id"].Value);

                // 2. Fill TextBoxes
                txtSponsorName.Text = row.Cells["name"].Value.ToString();
                txtSponsorIndustry.Text = row.Cells["industry"].Value.ToString();
                // 3. Set ComboBox Selection
                if (row.Cells["country_id"].Value != DBNull.Value)
                {
                    cmbSponsorCountry.SelectedValue = Convert.ToInt32(row.Cells["country_id"].Value);
                }
            }
        }

        private void btnClearSponsor_Click(object sender, EventArgs e)
        {
            _selectedSponsorId = 0;
            txtSponsorName.Clear();
            txtSponsorIndustry.Clear();

            if (cmbSponsorCountry.Items.Count > 0)
                cmbSponsorCountry.SelectedIndex = 0;

            dgvSponsors.ClearSelection();
        }

        private async Task LoadSocialMediaPlatformsAsync()
        {
            this.Cursor = Cursors.WaitCursor;

            string query = @"
        SELECT 
            social_media_platform_id, 
            name 
        FROM social_media_platform
        WHERE is_active = true
        ORDER BY social_media_platform_id DESC";

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

                        dgvSocialMediaPlatform.DataSource = dt;

                        // Hide the ID column
                        if (dgvSocialMediaPlatform.Columns["social_media_platform_id"] != null)
                            dgvSocialMediaPlatform.Columns["social_media_platform_id"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading social media platforms: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async void btnAddSocialMediaPlatform_Click(object sender, EventArgs e)
        {
            string name = txtNameSocialMediaPlatform.Text.Trim(); // Ensure TextBox is named correctly

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        INSERT INTO social_media_platform (name, created_by) 
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
                        MessageBox.Show("Social Media Platform added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSocialMediaPlatformsAsync(); // Async refresh
                        btnClearSocialMediaPlatform_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505") // Unique violation
                    MessageBox.Show("This Platform Name already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected Error: " + ex.Message);
            }
        }

        private async void btnUpdateSocialMediaPlatform_Click(object sender, EventArgs e)
        {
            if (_selectedSocialMediaPlatformId == 0)
            {
                MessageBox.Show("Please select a platform to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string name = txtNameSocialMediaPlatform.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = @"
        UPDATE social_media_platform SET 
            name = @name, 
            updated_at = CURRENT_TIMESTAMP,
            updated_by = @updaterId
        WHERE 
            social_media_platform_id = @platformId;";

            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@updaterId", _adminUserId);
                    command.Parameters.AddWithValue("@platformId", _selectedSocialMediaPlatformId);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Social Media Platform updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadSocialMediaPlatformsAsync();
                        btnClearSocialMediaPlatform_Click(sender, e);
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("This Platform Name already exists.", "Duplicate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show("Database Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating platform: " + ex.Message);
            }
        }

        private async void btnDeleteSocialMediaPlatform_Click(object sender, EventArgs e)
        {
            if (_selectedSocialMediaPlatformId == 0)
            {
                MessageBox.Show("Please select a platform to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show("Are you sure you want to delete this platform?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                string query = @"
            UPDATE social_media_platform SET 
                is_active = false,
                updated_at = CURRENT_TIMESTAMP,
                updated_by = @updaterId
            WHERE 
                social_media_platform_id = @platformId;";

                try
                {
                    using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@updaterId", _adminUserId);
                        command.Parameters.AddWithValue("@platformId", _selectedSocialMediaPlatformId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Social Media Platform deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            await LoadSocialMediaPlatformsAsync();
                            btnClearSocialMediaPlatform_Click(sender, e);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting platform: " + ex.Message);
                }
            }
        }

        private void btnClearSocialMediaPlatform_Click(object sender, EventArgs e)
        {
            _selectedSocialMediaPlatformId = 0;
            txtNameSocialMediaPlatform.Clear();
            dgvSocialMediaPlatform.ClearSelection();
        }

        private void dgvSocialMediaPlatform_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvSocialMediaPlatform_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvSocialMediaPlatform.Rows[e.RowIndex];

                _selectedSocialMediaPlatformId = Convert.ToInt32(row.Cells["social_media_platform_id"].Value);
                txtNameSocialMediaPlatform.Text = row.Cells["name"].Value.ToString();
            }
        }
    }
}
