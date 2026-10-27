using OnvifManager.Models;

namespace OnvifManager.Forms
{
    public partial class UCHome : UserControl
    {
        private readonly FormMain _mainForm;
        private readonly ListBox _cameraList = new();
        private readonly Label _emptyCameras = new();
        private readonly Label _cameraName = new();
        private readonly Label _cameraAddress = new();
        private readonly Label _statusLabel = new();
        private readonly ComboBox _profiles = new();
        private readonly Button _loadProfiles = new();
        private readonly Button _connect = new();
        private readonly Button _snapshot = new();
        private readonly Button _check = new();
        private bool _busy;

        public UCHome(FormMain parentForm)
        {
            _mainForm = parentForm;
            InitializeComponent();
            BindCameras();
        }

        private void InitializeComponent()
        {
            BackColor = Color.FromArgb(243, 246, 250);
            Dock = DockStyle.Fill;
            Padding = new Padding(0);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = BackColor,
                Margin = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var camerasCard = CreateCard();
            camerasCard.Margin = new Padding(0, 0, 18, 0);
            layout.Controls.Add(camerasCard, 0, 0);

            var cameraHeader = new Label
            {
                Text = "Kayıtlı kameralar",
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 46, 66),
                Location = new Point(19, 17),
                AutoSize = true
            };
            camerasCard.Controls.Add(cameraHeader);

            var cameraHint = new Label
            {
                Text = "Görüntülemek için bir kamera seçin",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(120, 132, 150),
                Location = new Point(20, 43),
                AutoSize = true
            };
            camerasCard.Controls.Add(cameraHint);

            _cameraList.DrawMode = DrawMode.OwnerDrawFixed;
            _cameraList.ItemHeight = 62;
            _cameraList.BorderStyle = BorderStyle.None;
            _cameraList.IntegralHeight = false;
            _cameraList.Font = new Font("Segoe UI", 9.5F);
            _cameraList.BackColor = Color.White;
            _cameraList.ForeColor = Color.FromArgb(32, 46, 66);
            _cameraList.DrawItem += DrawCameraItem;
            _cameraList.SelectedIndexChanged += CameraSelectionChanged;
            _cameraList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _cameraList.Location = new Point(16, 77);
            _cameraList.Size = new Size(296, 490);
            camerasCard.Controls.Add(_cameraList);

            _emptyCameras.Text = "Henüz kamera eklenmedi.\nKamera yönetiminden ekleyin veya ağda arayın.";
            _emptyCameras.Font = new Font("Segoe UI", 9F);
            _emptyCameras.ForeColor = Color.FromArgb(122, 135, 153);
            _emptyCameras.TextAlign = ContentAlignment.MiddleCenter;
            _emptyCameras.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            _emptyCameras.Location = new Point(23, 250);
            _emptyCameras.Size = new Size(280, 78);
            camerasCard.Controls.Add(_emptyCameras);

            var rightColumn = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = Padding.Empty,
                BackColor = BackColor
            };
            rightColumn.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            rightColumn.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
            layout.Controls.Add(rightColumn, 1, 0);

            var detailsCard = CreateCard();
            detailsCard.Margin = new Padding(0, 0, 0, 18);
            rightColumn.Controls.Add(detailsCard, 0, 0);

            var eyebrow = new Label
            {
                Text = "KAMERA DETAYLARI",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(96, 126, 164),
                Location = new Point(25, 23),
                AutoSize = true
            };
            detailsCard.Controls.Add(eyebrow);

            _cameraName.Text = "Kamera seçin";
            _cameraName.Font = new Font("Segoe UI Semibold", 19F, FontStyle.Bold);
            _cameraName.ForeColor = Color.FromArgb(30, 43, 61);
            _cameraName.Location = new Point(23, 52);
            _cameraName.Size = new Size(590, 38);
            _cameraName.AutoEllipsis = true;
            detailsCard.Controls.Add(_cameraName);

            _cameraAddress.Text = "Kamera bilgileri burada görüntülenir.";
            _cameraAddress.Font = new Font("Segoe UI", 9.5F);
            _cameraAddress.ForeColor = Color.FromArgb(111, 124, 143);
            _cameraAddress.Location = new Point(26, 96);
            _cameraAddress.AutoSize = true;
            detailsCard.Controls.Add(_cameraAddress);

            var divider = new Panel
            {
                BackColor = Color.FromArgb(232, 237, 243),
                Location = new Point(25, 137),
                Height = 1,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            detailsCard.Controls.Add(divider);
            detailsCard.Resize += (_, _) => divider.Width = Math.Max(40, detailsCard.ClientSize.Width - 50);
            divider.Width = 590;

            var profileLabel = new Label
            {
                Text = "Görüntü profili",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 65, 85),
                Location = new Point(25, 162),
                AutoSize = true
            };
            detailsCard.Controls.Add(profileLabel);

            _profiles.DropDownStyle = ComboBoxStyle.DropDownList;
            _profiles.Font = new Font("Segoe UI", 9.5F);
            _profiles.Location = new Point(25, 190);
            _profiles.Size = new Size(360, 31);
            _profiles.Enabled = false;
            detailsCard.Controls.Add(_profiles);

            StyleButton(_loadProfiles, "Profilleri getir", secondary: true);
            _loadProfiles.Location = new Point(397, 189);
            _loadProfiles.Size = new Size(154, 34);
            _loadProfiles.Click += LoadProfiles_Click;
            detailsCard.Controls.Add(_loadProfiles);

            StyleButton(_connect, "Canlı görüntüyü aç", secondary: false);
            _connect.Location = new Point(25, 244);
            _connect.Size = new Size(210, 42);
            _connect.Click += Connect_Click;
            detailsCard.Controls.Add(_connect);

            _statusLabel.Text = "Bağlanmak için bir kamera seçin.";
            _statusLabel.Font = new Font("Segoe UI", 9F);
            _statusLabel.ForeColor = Color.FromArgb(111, 124, 143);
            _statusLabel.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            _statusLabel.Location = new Point(25, 310);
            _statusLabel.Size = new Size(580, 28);
            detailsCard.Controls.Add(_statusLabel);

            var actionsCard = CreateCard();
            actionsCard.Padding = new Padding(18);
            rightColumn.Controls.Add(actionsCard, 0, 1);

            var actionTitle = new Label
            {
                Text = "Hızlı işlemler",
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 46, 66),
                Location = new Point(18, 14),
                AutoSize = true
            };
            actionsCard.Controls.Add(actionTitle);

            StyleButton(_snapshot, "Tüm kameralardan görüntü al", secondary: true);
            _snapshot.Location = new Point(18, 52);
            _snapshot.Size = new Size(235, 40);
            _snapshot.Click += Snapshot_Click;
            actionsCard.Controls.Add(_snapshot);

            StyleButton(_check, "Bağlantıları denetle", secondary: true);
            _check.Location = new Point(265, 52);
            _check.Size = new Size(195, 40);
            _check.Click += CheckCameras_Click;
            actionsCard.Controls.Add(_check);

            actionsCard.Resize += (_, _) =>
            {
                int available = Math.Max(150, actionsCard.ClientSize.Width - 48);
                _snapshot.Width = Math.Min(235, available / 2 - 6);
                _check.Left = _snapshot.Right + 12;
                _check.Width = Math.Min(195, Math.Max(120, available - _snapshot.Width - 12));
            };
        }

        private void BindCameras()
        {
            _cameraList.DataSource = _mainForm.cameraList;
            _emptyCameras.Visible = _mainForm.cameraList.Count == 0;
            if (_mainForm.cameraList.Count > 0)
                _cameraList.SelectedIndex = 0;
        }

        private static Panel CreateCard() => new()
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = Padding.Empty
        };

        private static void StyleButton(Button button, string text, bool secondary)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = secondary ? 1 : 0;
            button.FlatAppearance.BorderColor = Color.FromArgb(214, 222, 232);
            button.FlatAppearance.MouseOverBackColor = secondary ? Color.FromArgb(241, 246, 252) : Color.FromArgb(40, 94, 162);
            button.BackColor = secondary ? Color.White : Color.FromArgb(47, 105, 177);
            button.ForeColor = secondary ? Color.FromArgb(49, 67, 91) : Color.White;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        private void DrawCameraItem(object? sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= _cameraList.Items.Count)
                return;

            if (_cameraList.Items[e.Index] is not CameraDevice camera)
                return;

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color background = selected ? Color.FromArgb(234, 242, 252) : Color.White;
            using (var brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, e.Bounds);

            Color titleColor = selected ? Color.FromArgb(37, 91, 157) : Color.FromArgb(38, 51, 69);
            TextRenderer.DrawText(e.Graphics, string.IsNullOrWhiteSpace(camera.Name) ? "Adsız kamera" : camera.Name,
                new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                new Rectangle(e.Bounds.Left + 13, e.Bounds.Top + 12, e.Bounds.Width - 24, 20),
                titleColor, TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);
            TextRenderer.DrawText(e.Graphics, $"{camera.Ip}:{camera.Port}",
                new Font("Segoe UI", 8.5F),
                new Rectangle(e.Bounds.Left + 13, e.Bounds.Top + 34, e.Bounds.Width - 24, 18),
                Color.FromArgb(119, 132, 150), TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter);

            using var pen = new Pen(Color.FromArgb(238, 241, 246));
            e.Graphics.DrawLine(pen, e.Bounds.Left + 10, e.Bounds.Bottom - 1, e.Bounds.Right - 10, e.Bounds.Bottom - 1);
            e.DrawFocusRectangle();
        }

        private void CameraSelectionChanged(object? sender, EventArgs e)
        {
            _profiles.DataSource = null;
            _profiles.Enabled = false;
            if (_cameraList.SelectedItem is not CameraDevice camera)
            {
                _cameraName.Text = "Kamera seçin";
                _cameraAddress.Text = "Kamera bilgileri burada görüntülenir.";
                _connect.Enabled = false;
                _loadProfiles.Enabled = false;
                return;
            }

            _cameraName.Text = string.IsNullOrWhiteSpace(camera.Name) ? "Adsız kamera" : camera.Name;
            _cameraAddress.Text = $"{camera.Ip}:{camera.Port}   •   Kullanıcı: {(string.IsNullOrWhiteSpace(camera.Username) ? "belirtilmedi" : camera.Username)}";
            _statusLabel.Text = "Bağlanmadan önce bu kamera için görüntü profillerini getirin.";
            _connect.Enabled = true;
            _loadProfiles.Enabled = !_busy;
        }

        private async void LoadProfiles_Click(object? sender, EventArgs e)
        {
            if (_cameraList.SelectedItem is not CameraDevice camera)
                return;

            SetBusy(true, "Kamera profilleri getiriliyor…");
            try
            {
                var launcher = new SPCConnectorLauncher(_mainForm.connectorPath);
                var profiles = await launcher.GetCameraProfilesAsync(camera.Ip, camera.Port, camera.Username, camera.Password);
                _profiles.DataSource = profiles.Select(profile => new CameraProfile(profile.Item1, profile.Item2)).ToList();
                _profiles.DisplayMember = nameof(CameraProfile.Name);
                _profiles.ValueMember = nameof(CameraProfile.Token);
                _profiles.Enabled = profiles.Count > 0;
                _statusLabel.ForeColor = profiles.Count > 0 ? Color.FromArgb(35, 135, 91) : Color.FromArgb(179, 91, 41);
                _statusLabel.Text = profiles.Count > 0
                    ? $"{profiles.Count} görüntü profili hazır. Kullanmak istediğinizi seçin."
                    : "Profil bulunamadı. Kamera adresini ve giriş bilgilerini kontrol edin.";
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.FromArgb(180, 62, 62);
                _statusLabel.Text = "Profil bilgileri alınamadı.";
                MessageBox.Show($"Kamera profilleri alınamadı.\n{ex.Message}", "Bağlantı hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void Connect_Click(object? sender, EventArgs e)
        {
            if (_cameraList.SelectedItem is not CameraDevice camera)
            {
                MessageBox.Show("Önce bir kamera seçin.", "Kamera seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_profiles.SelectedItem is not CameraProfile profile)
            {
                MessageBox.Show("Canlı görüntüyü açmadan önce kamera profillerini getirip bir profil seçin.", "Profil seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var launcher = new SPCConnectorLauncher(_mainForm.connectorPath);
                using var process = launcher.LaunchLiveView(camera.Ip, camera.Port, camera.Username, camera.Password, profile.Token, camera.Name);
                _statusLabel.ForeColor = Color.FromArgb(35, 135, 91);
                _statusLabel.Text = "Canlı görüntü ayrı pencerede açıldı.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Canlı görüntü başlatılamadı.\n{ex.Message}", "Başlatma hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Snapshot_Click(object? sender, EventArgs e)
        {
            if (_mainForm.cameraList.Count == 0)
            {
                MessageBox.Show("Önce kamera ekleyin.", "Kamera bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true, "Kameralardan anlık görüntü alınıyor…");
            try
            {
                string eventId = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + Guid.NewGuid().ToString("N")[..6];
                var launcher = new SPCConnectorLauncher(_mainForm.connectorPath);
                var results = await launcher.TakeSnapshotsAsync(eventId, _mainForm.cameraList);
                int succeeded = results.Count(result => result.Item1 > 0 && !result.Item2.StartsWith("Error:", StringComparison.OrdinalIgnoreCase));
                string folder = Path.Combine(Path.GetDirectoryName(_mainForm.connectorPath)!, "Snapshots", eventId);
                MessageBox.Show($"{succeeded} / {_mainForm.cameraList.Count} kameradan görüntü alındı.\n\nKayıt klasörü:\n{folder}",
                    "Görüntü alma tamamlandı", MessageBoxButtons.OK,
                    succeeded > 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                _statusLabel.Text = $"Görüntü alma tamamlandı: {succeeded} / {_mainForm.cameraList.Count} başarılı.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Görüntüler alınamadı.\n{ex.Message}", "İşlem hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void CheckCameras_Click(object? sender, EventArgs e)
        {
            if (_mainForm.cameraList.Count == 0)
            {
                MessageBox.Show("Önce kamera ekleyin.", "Kamera bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetBusy(true, "Kamera bağlantıları denetleniyor…");
            try
            {
                var launcher = new SPCConnectorLauncher(_mainForm.connectorPath);
                var results = await launcher.RunConnectionScannerAsync(_mainForm.cameraList);
                var report = string.Join(Environment.NewLine, results.Select(result =>
                    $"{result.Item1,-20} {(result.Item2 == 1 ? "Bağlı" : "Yanıt vermiyor")}"));
                MessageBox.Show(report.Length == 0 ? "Tarama sonucu alınamadı." : report,
                    "Bağlantı denetimi", MessageBoxButtons.OK,
                    results.Any(result => result.Item2 == 1) ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                _statusLabel.Text = $"Bağlantı denetimi tamamlandı: {results.Count(result => result.Item2 == 1)} / {results.Count} kamera yanıt verdi.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kamera bağlantıları denetlenemedi.\n{ex.Message}", "İşlem hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool busy, string? message = null)
        {
            _busy = busy;
            UseWaitCursor = busy;
            _cameraList.Enabled = !busy;
            _loadProfiles.Enabled = !busy && _cameraList.SelectedItem is CameraDevice;
            _connect.Enabled = !busy && _cameraList.SelectedItem is CameraDevice;
            _snapshot.Enabled = !busy;
            _check.Enabled = !busy;
            if (message != null)
            {
                _statusLabel.ForeColor = Color.FromArgb(92, 113, 141);
                _statusLabel.Text = message;
            }
        }

        private sealed record CameraProfile(string Name, string Token);
    }
}
