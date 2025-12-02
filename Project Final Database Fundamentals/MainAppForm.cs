namespace Project_Final_Database_Fundamentals
{
    public partial class MainAppForm : Form
    {
        private Button currentButton;
        private int _currentUserId;
        public MainAppForm(int userId, string username)
        {
            InitializeComponent();
            _currentUserId = userId;
            lblWelcome.Text = $"Bienvenido, {username} (ID: {_currentUserId})";
        }



        // Tus colores (ajusta los códigos RGB a tu diseño)
        // Color de fondo normal (el gris oscuro de tu menú)
        private Color colorDefault = Color.FromArgb(42, 43, 46);
        // Color cuando está seleccionado (ej. un azul o gris más claro)
        private Color colorActive = Color.FromArgb(77, 161, 103);
        private void ActivateButton(object btnSender)
        {
            if (btnSender != null)
            {
                if (currentButton != (Button)btnSender)
                {
                    // 1. Primero apagamos el botón anterior (si existe)
                    DisableButton();

                    // 2. Activamos el botón nuevo
                    currentButton = (Button)btnSender;
                    currentButton.BackColor = colorActive;
                    currentButton.ForeColor = Color.White; // Opcional: cambiar color de letra
                    currentButton.Font = new System.Drawing.Font("Outfit Semibold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                }
            }
        }


        private void DisableButton()
        {
            // Si hay un botón activo, lo regresamos a su color original
            if (currentButton != null)
            {
                currentButton.BackColor = colorDefault;
                currentButton.ForeColor = Color.Gainsboro; // O el color original de tu letra
                currentButton.Font = new System.Drawing.Font("Outfit Semibold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            }
        }
        private void CargarControl(UserControl control)
        {
            // Limpia cualquier control que esté actualmente en el panel
            panelContenido.Controls.Clear();

            // Configura el control para que llene todo el panel
            control.Dock = DockStyle.Fill;

            // Agrega el nuevo control al panel
            panelContenido.Controls.Add(control);
        }

        private void btnSystemAdmnistration_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            // Crea una NUEVA instancia del módulo de equipos y lo carga
            ucAdministrationAndConfiguration moduloEquipos = new ucAdministrationAndConfiguration(_currentUserId);
            CargarControl(moduloEquipos);
        }

        private void MainAppForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnCompetitionsAndSeasons_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            ucCompetitionsAndSeasons moduloCompetitions = new ucCompetitionsAndSeasons(_currentUserId);
            CargarControl(moduloCompetitions);
        }

        private void btnTeamsAndClubs_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            ucTeamsAndClubs moduloTeamsAndClubs = new ucTeamsAndClubs(_currentUserId);
            CargarControl(moduloTeamsAndClubs);
        }

        private void btnHumanResources_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            ucHumanResources moduloHumanResources = new ucHumanResources(_currentUserId);
            CargarControl(moduloHumanResources);
        }

        private void btnLineupsAndFormations_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            ucLineupsAndFormations moduloLineupsAndFormations = new ucLineupsAndFormations(_currentUserId);
            CargarControl(moduloLineupsAndFormations);
        }

        private void btnGameMatchesAndEvents_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            ucGameMatchesAndEvents moduloGameMatchesAndEvents = new ucGameMatchesAndEvents(_currentUserId);
            CargarControl(moduloGameMatchesAndEvents);
        }

        private void btnPlayerPerfomance_Click(object sender, EventArgs e)
        {
            ActivateButton(sender);
            ucPlayerPerfomanceAndHealth moduloPlayerPerfomanceAndHealth = new ucPlayerPerfomanceAndHealth(_currentUserId);
            CargarControl(moduloPlayerPerfomanceAndHealth);
        }
    }
}
