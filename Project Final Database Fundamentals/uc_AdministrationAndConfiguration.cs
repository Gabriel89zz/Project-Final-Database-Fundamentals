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

    }
}
