using OnvifManager.Models;
using System.ComponentModel;

namespace OnvifManager.Forms
{
    public partial class FormMain : Form
    {
        public readonly BindingList<CameraDevice> cameraList;
        public readonly string connectorPath;

        public FormMain()
        {
            InitializeComponent();

            cameraList = CameraStore.Load();
            connectorPath = ResolveConnectorPath();

            Text = "ONVIF Camera Manager";
            MinimumSize = new Size(980, 650);
            StartPosition = FormStartPosition.CenterScreen;
            ConfigureNavigation();
            LoadPage(new UCHome(this), "Canlı izleme", "Kameralarınızı tek bir yerden yönetin ve izleyin.", btnHome);
        }

        public void LoadPage(UserControl userControl, string title, string subtitle, Button activeButton)
        {
            while (pnlContainer.Controls.Count > 0)
            {
                Control previousPage = pnlContainer.Controls[0];
                pnlContainer.Controls.RemoveAt(0);
                previousPage.Dispose();
            }
            userControl.Dock = DockStyle.Fill;
            pnlContainer.Controls.Add(userControl);
            lblPageTitle.Text = title;
            lblPageSubtitle.Text = subtitle;
            SetActiveNavigation(activeButton);
        }

        public void RefreshCameraCount()
        {
            lblCameraCount.Text = $"{cameraList.Count} kamera";
        }

        private void ConfigureNavigation()
        {
            StyleNavigationButton(btnHome);
            StyleNavigationButton(btnManage);
            RefreshCameraCount();
        }

        private void StyleNavigationButton(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(34, 50, 73);
            button.BackColor = Color.FromArgb(21, 34, 52);
            button.ForeColor = Color.FromArgb(205, 215, 230);
            button.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(18, 0, 0, 0);
            button.Cursor = Cursors.Hand;
        }

        private void SetActiveNavigation(Button activeButton)
        {
            foreach (Button button in new[] { btnHome, btnManage })
            {
                button.BackColor = button == activeButton
                    ? Color.FromArgb(38, 87, 153)
                    : Color.FromArgb(21, 34, 52);
                button.ForeColor = button == activeButton ? Color.White : Color.FromArgb(205, 215, 230);
            }
        }

        private static string ResolveConnectorPath()
        {
            string packagedPath = Path.Combine(AppContext.BaseDirectory, "SPCConnector", "SPCConnector.exe");
            if (File.Exists(packagedPath))
                return packagedPath;

            string developmentPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..",
                "SPCConnector", "bin", "Debug", "net10.0-windows", "SPCConnector.exe"));

            return File.Exists(developmentPath) ? developmentPath : packagedPath;
        }

        private void btnHome_Click(object? sender, EventArgs e)
        {
            LoadPage(new UCHome(this), "Canlı izleme", "Kameralarınızı tek bir yerden yönetin ve izleyin.", btnHome);
        }

        private void btnManage_Click(object? sender, EventArgs e)
        {
            LoadPage(new UCEditCameras(this), "Kamera yönetimi", "Kamera bilgilerini düzenleyin veya ağınızda ONVIF cihazlarını arayın.", btnManage);
        }

        private void FormMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                CameraStore.Save(cameraList);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kamera ayarları kaydedilemedi.\n{ex.Message}", "Kaydetme hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
