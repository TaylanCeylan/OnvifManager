using OnvifManager.Models;
using System.Net;

namespace OnvifManager.Forms
{
    public partial class UCEditCameras : UserControl
    {
        private readonly FormMain _mainForm;
        private readonly DataGridView _grid = new();
        private readonly TextBox _name = new();
        private readonly TextBox _ip = new();
        private readonly NumericUpDown _port = new();
        private readonly TextBox _username = new();
        private readonly TextBox _password = new();
        private readonly Button _discover = new();
        private readonly Button _add = new();
        private readonly Button _delete = new();
        private readonly Button _save = new();
        private readonly Label _status = new();
        private bool _busy;

        public UCEditCameras(FormMain parentForm)
        {
            _mainForm = parentForm;
            InitializeComponent();
            SelectCamera(_mainForm.cameraList.FirstOrDefault());
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(243, 246, 250);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = BackColor,
                Margin = Padding.Empty
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var listCard = CreateCard();
            listCard.Margin = new Padding(0, 0, 16, 0);
            layout.Controls.Add(listCard, 0, 0);

            var listTitle = new Label
            {
                Text = "Kayıtlı kameralar",
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 46, 66),
                Location = new Point(20, 17),
                AutoSize = true
            };
            listCard.Controls.Add(listTitle);

            StyleButton(_discover, "Ağda ONVIF kamera ara", primary: true);
            _discover.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _discover.Location = new Point(455, 13);
            _discover.Size = new Size(210, 36);
            _discover.Click += Discover_Click;
            listCard.Controls.Add(_discover);
            listCard.Resize += (_, _) => _discover.Left = listCard.ClientSize.Width - _discover.Width - 18;

            _grid.Dock = DockStyle.None;
            _grid.BackgroundColor = Color.White;
            _grid.BorderStyle = BorderStyle.None;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(246, 248, 251);
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(84, 101, 124);
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 0, 0);
            _grid.DefaultCellStyle.BackColor = Color.White;
            _grid.DefaultCellStyle.ForeColor = Color.FromArgb(43, 57, 76);
            _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(233, 241, 251);
            _grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(37, 81, 135);
            _grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            _grid.DefaultCellStyle.Padding = new Padding(8, 3, 4, 3);
            _grid.RowTemplate.Height = 38;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.MultiSelect = false;
            _grid.ReadOnly = true;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoGenerateColumns = false;
            _grid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            _grid.Location = new Point(18, 63);
            _grid.Size = new Size(647, 504);
            _grid.Columns.Add(CreateGridColumn("Name", "Kamera adı", 27));
            _grid.Columns.Add(CreateGridColumn("Ip", "IP adresi", 31));
            _grid.Columns.Add(CreateGridColumn("Port", "Port", 14));
            _grid.Columns.Add(CreateGridColumn("Username", "Kullanıcı", 28));
            _grid.DataSource = new BindingSource { DataSource = _mainForm.cameraList };
            _grid.SelectionChanged += Grid_SelectionChanged;
            listCard.Controls.Add(_grid);
            listCard.Resize += (_, _) => _grid.Size = new Size(Math.Max(100, listCard.ClientSize.Width - 36), Math.Max(100, listCard.ClientSize.Height - 82));

            var editorCard = CreateCard();
            layout.Controls.Add(editorCard, 1, 0);

            var editorTitle = new Label
            {
                Text = "Kamera bilgileri",
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(32, 46, 66),
                Location = new Point(20, 19),
                AutoSize = true
            };
            editorCard.Controls.Add(editorTitle);

            StyleField("Kamera adı", _name, editorCard, 59);
            StyleField("IP adresi veya ana bilgisayar adı", _ip, editorCard, 134);

            var portLabel = CreateFieldLabel("ONVIF portu");
            portLabel.Location = new Point(22, 208);
            editorCard.Controls.Add(portLabel);
            _port.Minimum = 1;
            _port.Maximum = 65535;
            _port.Value = 80;
            _port.Font = new Font("Segoe UI", 9.5F);
            _port.Location = new Point(21, 233);
            _port.Size = new Size(140, 30);
            _port.ThousandsSeparator = false;
            editorCard.Controls.Add(_port);

            StyleField("Kullanıcı adı", _username, editorCard, 276);
            StyleField("Parola", _password, editorCard, 350);
            _password.UseSystemPasswordChar = true;

            _status.Text = "Parolalar bu Windows hesabına özel şifrelenerek saklanır.";
            _status.Font = new Font("Segoe UI", 8F);
            _status.ForeColor = Color.FromArgb(117, 131, 151);
            _status.Location = new Point(22, 425);
            _status.Size = new Size(340, 30);
            _status.AutoEllipsis = true;
            editorCard.Controls.Add(_status);

            StyleButton(_add, "Yeni kamera", primary: false);
            _add.Location = new Point(20, 0);
            _add.Size = new Size(114, 38);
            _add.Click += Add_Click;
            editorCard.Controls.Add(_add);

            StyleButton(_delete, "Sil", primary: false);
            _delete.Location = new Point(142, 0);
            _delete.Size = new Size(76, 38);
            _delete.Click += Delete_Click;
            editorCard.Controls.Add(_delete);

            StyleButton(_save, "Değişiklikleri kaydet", primary: true);
            _save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _save.Location = new Point(230, 0);
            _save.Size = new Size(186, 38);
            _save.Click += Save_Click;
            editorCard.Controls.Add(_save);
            editorCard.Resize += (_, _) =>
            {
                int top = editorCard.ClientSize.Height - 58;
                _add.Top = top;
                _delete.Top = top;
                _save.Left = editorCard.ClientSize.Width - _save.Width - 20;
                _save.Top = top;
            };
        }

        private static DataGridViewTextBoxColumn CreateGridColumn(string property, string header, int fillWeight) => new()
        {
            DataPropertyName = property,
            HeaderText = header,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.Automatic
        };

        private static Panel CreateCard() => new()
        {
            BackColor = Color.White,
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = Padding.Empty
        };

        private static void StyleField(string label, TextBox input, Control parent, int top)
        {
            var caption = CreateFieldLabel(label);
            caption.Location = new Point(22, top);
            parent.Controls.Add(caption);
            input.Font = new Font("Segoe UI", 9.5F);
            input.BorderStyle = BorderStyle.FixedSingle;
            input.Location = new Point(21, top + 25);
            input.Size = new Size(300, 30);
            input.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            parent.Controls.Add(input);
            parent.Resize += (_, _) => input.Width = Math.Max(120, parent.ClientSize.Width - 43);
        }

        private static Label CreateFieldLabel(string text) => new()
        {
            Text = text,
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(72, 88, 110),
            AutoSize = true
        };

        private static void StyleButton(Button button, string text, bool primary)
        {
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(214, 222, 232);
            button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(40, 94, 162) : Color.FromArgb(241, 246, 252);
            button.BackColor = primary ? Color.FromArgb(47, 105, 177) : Color.White;
            button.ForeColor = primary ? Color.White : Color.FromArgb(49, 67, 91);
            button.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        private void Grid_SelectionChanged(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is CameraDevice camera)
                SelectCamera(camera);
        }

        private void SelectCamera(CameraDevice? camera)
        {
            if (camera == null)
            {
                _name.Text = string.Empty;
                _ip.Text = string.Empty;
                _port.Value = 80;
                _username.Text = string.Empty;
                _password.Text = string.Empty;
                _delete.Enabled = false;
                _save.Enabled = false;
                return;
            }

            _name.Text = camera.Name;
            _ip.Text = camera.Ip;
            _port.Value = camera.Port is > 0 and <= 65535 ? camera.Port : 80;
            _username.Text = camera.Username;
            _password.Text = camera.Password;
            _delete.Enabled = !_busy;
            _save.Enabled = !_busy;
        }

        private void Add_Click(object? sender, EventArgs e)
        {
            var camera = new CameraDevice { Name = "Yeni kamera" };
            _mainForm.cameraList.Add(camera);
            _mainForm.RefreshCameraCount();
            int index = _mainForm.cameraList.Count - 1;
            _grid.ClearSelection();
            _grid.CurrentCell = _grid.Rows[index].Cells[0];
            _grid.Rows[index].Selected = true;
            SelectCamera(camera);
            _name.SelectAll();
            _name.Focus();
            _status.ForeColor = Color.FromArgb(92, 113, 141);
            _status.Text = "Kamera bilgilerini girip değişiklikleri kaydedin.";
        }

        private void Delete_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not CameraDevice camera)
                return;

            if (MessageBox.Show($"'{camera.Name}' kamerasını silmek istiyor musunuz?", "Kamerayı sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _mainForm.cameraList.Remove(camera);
            _mainForm.RefreshCameraCount();
            SelectCamera(_mainForm.cameraList.FirstOrDefault());
            PersistCameras();
            _status.Text = "Kamera silindi.";
        }

        private void Save_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not CameraDevice camera)
            {
                MessageBox.Show("Önce yeni kamera ekleyin veya listeden bir kamera seçin.", "Kamera seçilmedi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string name = _name.Text.Trim();
            string host = _ip.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Kamera adı boş bırakılamaz.", "Eksik bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _name.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(host) || (IPAddress.TryParse(host, out _) == false && Uri.CheckHostName(host) == UriHostNameType.Unknown))
            {
                MessageBox.Show("Geçerli bir IP adresi veya ana bilgisayar adı girin.", "Geçersiz adres", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ip.Focus();
                return;
            }

            bool duplicate = _mainForm.cameraList.Any(other => other != camera && other.Ip.Equals(host, StringComparison.OrdinalIgnoreCase) && other.Port == _port.Value);
            if (duplicate)
            {
                MessageBox.Show("Bu adres ve port için başka bir kamera zaten kayıtlı.", "Tekrarlanan kamera", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _ip.Focus();
                return;
            }

            camera.Name = name;
            camera.Ip = host;
            camera.Port = (int)_port.Value;
            camera.Username = _username.Text.Trim();
            camera.Password = _password.Text;
            _mainForm.cameraList.ResetBindings();
            _mainForm.RefreshCameraCount();

            try
            {
                PersistCameras();
                _status.ForeColor = Color.FromArgb(35, 135, 91);
                _status.Text = "Değişiklikler güvenli şekilde kaydedildi.";
            }
            catch (Exception ex)
            {
                _status.ForeColor = Color.FromArgb(180, 62, 62);
                _status.Text = "Kayıt başarısız.";
                MessageBox.Show($"Kamera bilgileri kaydedilemedi.\n{ex.Message}", "Kaydetme hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void Discover_Click(object? sender, EventArgs e)
        {
            SetBusy(true, "Yerel ağda ONVIF kameralar aranıyor…");
            try
            {
                var launcher = new SPCConnectorLauncher(_mainForm.connectorPath);
                var foundCameras = await launcher.DiscoverNetworkCamerasAsync();
                int added = 0;
                foreach (var found in foundCameras)
                {
                    bool exists = _mainForm.cameraList.Any(camera =>
                        camera.Ip.Equals(found.Item2, StringComparison.OrdinalIgnoreCase) && camera.Port == found.Item3);
                    if (exists)
                        continue;

                    _mainForm.cameraList.Add(new CameraDevice
                    {
                        Name = string.IsNullOrWhiteSpace(found.Item1) ? found.Item2 : found.Item1,
                        Ip = found.Item2,
                        Port = found.Item3
                    });
                    added++;
                }

                _mainForm.RefreshCameraCount();
                _status.ForeColor = Color.FromArgb(35, 135, 91);
                _status.Text = foundCameras.Count == 0
                    ? "Ağ taraması tamamlandı; ONVIF cihazı bulunamadı."
                    : $"{foundCameras.Count} cihaz bulundu, {added} yeni kamera eklendi. Giriş bilgileri kamera ayrıntılarından girilebilir.";
                if (added > 0)
                {
                    _grid.ClearSelection();
                    _grid.CurrentCell = _grid.Rows[_grid.Rows.Count - 1].Cells[0];
                    _grid.Rows[_grid.Rows.Count - 1].Selected = true;
                }
            }
            catch (Exception ex)
            {
                _status.ForeColor = Color.FromArgb(180, 62, 62);
                _status.Text = "Ağ taraması başarısız oldu.";
                MessageBox.Show($"Kamera taraması tamamlanamadı.\n{ex.Message}", "Tarama hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            _discover.Enabled = !busy;
            _add.Enabled = !busy;
            _delete.Enabled = !busy && _grid.CurrentRow?.DataBoundItem is CameraDevice;
            _save.Enabled = !busy && _grid.CurrentRow?.DataBoundItem is CameraDevice;
            if (message != null)
            {
                _status.ForeColor = Color.FromArgb(92, 113, 141);
                _status.Text = message;
            }
        }

        private void PersistCameras() => CameraStore.Save(_mainForm.cameraList);
    }
}
