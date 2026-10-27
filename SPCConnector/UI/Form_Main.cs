using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using SPCConnector.Classes;
using SPCConnector.Services;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.Json;

namespace SPCConnector.UI
{
    public sealed class Form_Main : Form
    {
        private readonly System.Windows.Forms.Timer _reconnectTimer = new() { Interval = 2500 };
        private readonly Panel _videoPanel = new();
        private readonly Label _statusLabel = new();
        private readonly Label _locationLabel = new();
        private readonly Button _snapshotButton = new();
        private readonly Button _fullscreenButton = new();
        private readonly Button _ptzUp = new();
        private readonly Button _ptzDown = new();
        private readonly Button _ptzLeft = new();
        private readonly Button _ptzRight = new();
        private readonly Button _zoomIn = new();
        private readonly Button _zoomOut = new();
        private LibVLC? _libVLC;
        private VideoView? _videoViewer;
        private SPIPCamera? _ipCamera;
        private Tuple<int, string, int, string, string, string, string>? _cameraInfo;
        private bool _connecting;
        private bool _isFullscreen;
        private FormBorderStyle _previousBorderStyle;
        private FormWindowState _previousWindowState;

        public Form_Main(string[] args)
        {
            InitializeComponent();
            Load += Form_Main_Load;
            FormClosing += Form_Main_FormClosing;
            KeyDown += Form_Main_KeyDown;
            _reconnectTimer.Tick += ReconnectTimer_Tick;
        }

        private void InitializeComponent()
        {
            Text = "ONVIF canlı görüntü";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(860, 560);
            ClientSize = new Size(1240, 760);
            BackColor = Color.FromArgb(13, 20, 30);
            Font = new Font("Segoe UI", 9F);
            KeyPreview = true;

            var topBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 66,
                BackColor = Color.FromArgb(20, 31, 46),
                Padding = new Padding(20, 0, 20, 0)
            };
            Controls.Add(topBar);

            var title = new Label
            {
                Text = "CANLI GÖRÜNTÜ",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 12)
            };
            topBar.Controls.Add(title);

            _locationLabel.Text = string.Empty;
            _locationLabel.AutoSize = true;
            _locationLabel.Font = new Font("Segoe UI", 8.5F);
            _locationLabel.ForeColor = Color.FromArgb(165, 180, 200);
            _locationLabel.Location = new Point(21, 36);
            topBar.Controls.Add(_locationLabel);

            _statusLabel.Text = "Bağlantı hazırlanıyor…";
            _statusLabel.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            _statusLabel.ForeColor = Color.FromArgb(244, 193, 89);
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            _statusLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _statusLabel.Location = new Point(ClientSize.Width - 470, 18);
            _statusLabel.Size = new Size(200, 30);
            topBar.Controls.Add(_statusLabel);

            StyleButton(_snapshotButton, "Görüntü kaydet", Color.FromArgb(39, 55, 75));
            _snapshotButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _snapshotButton.Size = new Size(130, 36);
            _snapshotButton.Location = new Point(ClientSize.Width - 245, 15);
            _snapshotButton.Enabled = false;
            _snapshotButton.Click += async (_, _) => await SaveSnapshotAsync();
            topBar.Controls.Add(_snapshotButton);

            StyleButton(_fullscreenButton, "Tam ekran", Color.FromArgb(39, 55, 75));
            _fullscreenButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _fullscreenButton.Size = new Size(95, 36);
            _fullscreenButton.Location = new Point(ClientSize.Width - 105, 15);
            _fullscreenButton.Click += (_, _) => ToggleFullscreen();
            topBar.Controls.Add(_fullscreenButton);

            topBar.Resize += (_, _) =>
            {
                _fullscreenButton.Left = topBar.ClientSize.Width - _fullscreenButton.Width - 16;
                _snapshotButton.Left = _fullscreenButton.Left - _snapshotButton.Width - 10;
                _statusLabel.Left = _snapshotButton.Left - _statusLabel.Width - 14;
            };

            _videoPanel.Dock = DockStyle.Fill;
            _videoPanel.BackColor = Color.Black;
            _videoPanel.SizeChanged += (_, _) => UpdateVideoAspectRatio();
            Controls.Add(_videoPanel);
            _videoPanel.SendToBack();
            topBar.BringToFront();

            var ptzPanel = new Panel
            {
                Size = new Size(208, 190),
                BackColor = Color.FromArgb(215, 20, 31, 46),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                Location = new Point(ClientSize.Width - 228, ClientSize.Height - 210),
                Padding = new Padding(12)
            };
            _videoPanel.Controls.Add(ptzPanel);

            var ptzTitle = new Label
            {
                Text = "KAMERA KONTROLÜ",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(152, 171, 193),
                Location = new Point(12, 10),
                AutoSize = true
            };
            ptzPanel.Controls.Add(ptzTitle);

            var pad = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                Location = new Point(13, 36),
                Size = new Size(116, 116),
                BackColor = Color.Transparent
            };
            for (int i = 0; i < 3; i++)
            {
                pad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
                pad.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            }
            ptzPanel.Controls.Add(pad);
            ConfigurePtzButton(_ptzUp, "▲", "up");
            ConfigurePtzButton(_ptzLeft, "◀", "left");
            ConfigurePtzButton(_ptzRight, "▶", "right");
            ConfigurePtzButton(_ptzDown, "▼", "down");
            pad.Controls.Add(_ptzUp, 1, 0);
            pad.Controls.Add(_ptzLeft, 0, 1);
            pad.Controls.Add(_ptzRight, 2, 1);
            pad.Controls.Add(_ptzDown, 1, 2);
            var center = new Label
            {
                Text = "●",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(118, 142, 171),
                BackColor = Color.FromArgb(36, 53, 74)
            };
            pad.Controls.Add(center, 1, 1);

            ConfigurePtzButton(_zoomIn, "+", "in");
            ConfigurePtzButton(_zoomOut, "−", "out");
            _zoomIn.Location = new Point(145, 64);
            _zoomOut.Location = new Point(145, 106);
            _zoomIn.Size = _zoomOut.Size = new Size(43, 36);
            ptzPanel.Controls.Add(_zoomIn);
            ptzPanel.Controls.Add(_zoomOut);

            var cameraOverlay = new Label
            {
                Text = "",
                ForeColor = Color.White,
                BackColor = Color.FromArgb(185, 20, 31, 46),
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                MaximumSize = new Size(500, 60),
                Padding = new Padding(11, 7, 11, 7),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Location = new Point(20, ClientSize.Height - 112)
            };
            _locationLabel.Tag = cameraOverlay;
            _videoPanel.Controls.Add(cameraOverlay);

            _videoPanel.Resize += (_, _) =>
            {
                ptzPanel.Left = _videoPanel.ClientSize.Width - ptzPanel.Width - 20;
                ptzPanel.Top = _videoPanel.ClientSize.Height - ptzPanel.Height - 20;
                cameraOverlay.Left = 20;
                cameraOverlay.Top = _videoPanel.ClientSize.Height - cameraOverlay.Height - 20;
            };

            _reconnectTimer.Start();
        }

        private static void StyleButton(Button button, string text, Color background)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(63, 80, 102);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(53, 73, 97);
            button.BackColor = background;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        private void ConfigurePtzButton(Button button, string text, string direction)
        {
            StyleButton(button, text, Color.FromArgb(36, 53, 74));
            button.Dock = DockStyle.Fill;
            button.Margin = new Padding(2);
            button.Tag = direction;
            button.Enabled = false;
            button.MouseDown += CameraMouseDownMove;
            button.MouseUp += CameraMouseUpMove;
            button.MouseLeave += CameraMouseUpMove;
        }

        private async void Form_Main_Load(object? sender, EventArgs e)
        {
            try
            {
                Core.Initialize();
                _libVLC = new LibVLC();
                _videoViewer = new VideoView { Dock = DockStyle.Fill, BackColor = Color.Black };
                _videoPanel.Controls.Add(_videoViewer);
                _videoViewer.SendToBack();
                _statusLabel.BringToFront();
                _snapshotButton.BringToFront();
                _fullscreenButton.BringToFront();

                _cameraInfo = ReadCameraInfo();
                if (_cameraInfo == null)
                {
                    SetConnectionStatus("Kamera bilgisi alınamadı", Color.FromArgb(239, 111, 111));
                    return;
                }

                Text = $"{_cameraInfo.Item7} • ONVIF canlı görüntü";
                _locationLabel.Text = $"{_cameraInfo.Item7}  ·  {_cameraInfo.Item2}:{_cameraInfo.Item3}";
                if (_locationLabel.Tag is Label overlay)
                    overlay.Text = _locationLabel.Text;
                await ConnectCameraAsync(_cameraInfo);
            }
            catch (Exception ex)
            {
                SetConnectionStatus("Görüntü başlatılamadı", Color.FromArgb(239, 111, 111));
#if LOGACTIVE
                Logger.Log($"Canlı görüntü başlatılamadı: {ex}");
#endif
                MessageBox.Show($"Video bileşeni başlatılamadı.\n{ex.Message}", "Başlatma hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Tuple<int, string, int, string, string, string, string>? ReadCameraInfo()
        {
            using var mmf = MemoryMappedFile.OpenExisting("SPCameraViewer");
            using var stream = mmf.CreateViewStream();
            using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: false);
            int length = reader.ReadInt32();
            if (length <= 0 || length > stream.Length - sizeof(int) || length > 1024 * 1024)
                return null;

            byte[] payload = reader.ReadBytes(length);
            return payload.Length == length
                ? JsonSerializer.Deserialize<Tuple<int, string, int, string, string, string, string>>(payload)
                : null;
        }

        private async Task ConnectCameraAsync(Tuple<int, string, int, string, string, string, string>? camera)
        {
            if (camera == null || _libVLC == null || _videoViewer == null || _connecting)
                return;

            _connecting = true;
            try
            {
                _ipCamera?.Disconnect();
                _ipCamera = new SPIPCamera(_libVLC, _videoViewer);
                SetConnectionStatus("Kameraya bağlanılıyor…", Color.FromArgb(244, 193, 89));
                SetPtzEnabled(false);
                _snapshotButton.Enabled = false;

                bool connected = await _ipCamera.ConnectAsync(camera);
                if (connected)
                {
                    SetConnectionStatus("Canlı bağlantı", Color.FromArgb(110, 214, 157));
                    SetPtzEnabled(true);
                    _snapshotButton.Enabled = true;
                }
                else
                {
                    SetConnectionStatus("Bağlantı kurulamadı · yeniden denenecek", Color.FromArgb(239, 148, 112));
                }

                UpdateVideoAspectRatio();
            }
            catch (Exception ex)
            {
                SetConnectionStatus("Bağlantı hatası · yeniden denenecek", Color.FromArgb(239, 111, 111));
#if LOGACTIVE
                Logger.Log($"Kamera bağlantı hatası: {ex}");
#endif
            }
            finally
            {
                _connecting = false;
            }
        }

        private void SetConnectionStatus(string text, Color color)
        {
            if (IsDisposed || !IsHandleCreated)
                return;
            _statusLabel.Text = text;
            _statusLabel.ForeColor = color;
        }

        private void SetPtzEnabled(bool enabled)
        {
            _ptzUp.Enabled = enabled;
            _ptzDown.Enabled = enabled;
            _ptzLeft.Enabled = enabled;
            _ptzRight.Enabled = enabled;
            _zoomIn.Enabled = enabled;
            _zoomOut.Enabled = enabled;
        }

        private async Task SaveSnapshotAsync()
        {
            if (_cameraInfo == null || !_snapshotButton.Enabled)
                return;

            using var dialog = new SaveFileDialog
            {
                Title = "Anlık görüntüyü kaydet",
                Filter = "JPEG görüntüsü (*.jpg)|*.jpg",
                DefaultExt = "jpg",
                AddExtension = true,
                FileName = $"{_cameraInfo.Item2}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _snapshotButton.Enabled = false;
            try
            {
                var service = new OnvifDeviceService();
                await service.TakeSnapshot(_cameraInfo.Item2, _cameraInfo.Item3, _cameraInfo.Item4, _cameraInfo.Item5, dialog.FileName);
                SetConnectionStatus("Görüntü kaydedildi", Color.FromArgb(110, 214, 157));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Anlık görüntü kaydedilemedi.\n{ex.Message}", "Görüntü alma hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _snapshotButton.Enabled = _ipCamera != null;
            }
        }

        private void CameraMouseDownMove(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && sender is Button { Tag: string direction } && _ipCamera != null)
                _ipCamera.MoveCamera(direction);
        }

        private void CameraMouseUpMove(object? sender, EventArgs e) => _ipCamera?.CameraMouseUpMove();

        private async void ReconnectTimer_Tick(object? sender, EventArgs e)
        {
            if (_ipCamera == null || _connecting || _cameraInfo == null)
                return;

            if (_videoViewer?.MediaPlayer?.IsPlaying == true)
            {
                _ipCamera.ResetTimeOutCounter();
                return;
            }

            if (_ipCamera.IsTimedOut())
            {
                _ipCamera.ResetTimeOutCounter();
                await ConnectCameraAsync(_cameraInfo);
            }
        }

        private void UpdateVideoAspectRatio()
        {
            if (_videoViewer?.MediaPlayer is not { } player || _videoPanel.ClientSize.Width <= 0 || _videoPanel.ClientSize.Height <= 0)
                return;

            player.AspectRatio = $"{_videoPanel.ClientSize.Width}:{_videoPanel.ClientSize.Height}";
        }

        private void ToggleFullscreen()
        {
            if (!_isFullscreen)
            {
                _previousBorderStyle = FormBorderStyle;
                _previousWindowState = WindowState;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
                _fullscreenButton.Text = "Pencereye dön";
                _isFullscreen = true;
            }
            else
            {
                FormBorderStyle = _previousBorderStyle;
                WindowState = _previousWindowState;
                _fullscreenButton.Text = "Tam ekran";
                _isFullscreen = false;
            }
        }

        private void Form_Main_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _isFullscreen)
            {
                ToggleFullscreen();
                e.Handled = true;
            }
        }

        private void Form_Main_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _reconnectTimer.Stop();
            _ipCamera?.Disconnect();
            _videoViewer?.Dispose();
            _libVLC?.Dispose();
        }
    }
}
