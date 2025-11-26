namespace Project_Final_Database_Fundamentals
{
    public partial class MainAppForm : Form
    {
        private int _currentUserId;
        public MainAppForm(int userId, string username)
        {
            InitializeComponent();
            _currentUserId = userId;
            lblWelcome.Text = $"Bienvenido, {username} (ID: {_currentUserId})";
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
            // Crea una NUEVA instancia del módulo de equipos y lo carga
            uc_AdministrationAndConfiguration moduloEquipos = new uc_AdministrationAndConfiguration(_currentUserId);
            CargarControl(moduloEquipos);
        }

        private void MainAppForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void btnCompetitionsAndSeasons_Click(object sender, EventArgs e)
        {
            uc_CompetitionsAndSeasons moduloCompetitions = new uc_CompetitionsAndSeasons(_currentUserId);
            CargarControl(moduloCompetitions);
        }

        private void btnTeamsAndClubs_Click(object sender, EventArgs e)
        {
            ucTeamsAndClubs moduloTeamsAndClubs = new ucTeamsAndClubs(_currentUserId);
            CargarControl(moduloTeamsAndClubs);
        }
    }
}
