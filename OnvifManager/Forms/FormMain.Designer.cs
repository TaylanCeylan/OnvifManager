#nullable enable

namespace OnvifManager.Forms
{
    partial class FormMain
    {
        private System.ComponentModel.IContainer? components;
        private Panel pnlSidebar = null!;
        private Panel pnlWorkspace = null!;
        private Panel pnlTopbar = null!;
        private Panel pnlContainer = null!;
        private Label lblBrand = null!;
        private Label lblBrandCaption = null!;
        private Label lblNavSection = null!;
        private Label lblPageTitle = null!;
        private Label lblPageSubtitle = null!;
        private Label lblCameraCount = null!;
        private Label lblFooter = null!;
        private Button btnHome = null!;
        private Button btnManage = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlSidebar = new Panel();
            lblFooter = new Label();
            btnManage = new Button();
            btnHome = new Button();
            lblNavSection = new Label();
            lblBrandCaption = new Label();
            lblBrand = new Label();
            pnlWorkspace = new Panel();
            pnlContainer = new Panel();
            pnlTopbar = new Panel();
            lblCameraCount = new Label();
            lblPageSubtitle = new Label();
            lblPageTitle = new Label();
            pnlSidebar.SuspendLayout();
            pnlWorkspace.SuspendLayout();
            pnlTopbar.SuspendLayout();
            SuspendLayout();
            // 
            // pnlSidebar
            // 
            pnlSidebar.BackColor = Color.FromArgb(21, 34, 52);
            pnlSidebar.Controls.Add(lblFooter);
            pnlSidebar.Controls.Add(btnManage);
            pnlSidebar.Controls.Add(btnHome);
            pnlSidebar.Controls.Add(lblNavSection);
            pnlSidebar.Controls.Add(lblBrandCaption);
            pnlSidebar.Controls.Add(lblBrand);
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Padding = new Padding(0, 0, 0, 12);
            pnlSidebar.Size = new Size(232, 760);
            // 
            // lblFooter
            // 
            lblFooter.Dock = DockStyle.Bottom;
            lblFooter.Font = new Font("Segoe UI", 8.5F);
            lblFooter.ForeColor = Color.FromArgb(140, 157, 180);
            lblFooter.Padding = new Padding(22, 0, 0, 0);
            lblFooter.Size = new Size(232, 32);
            lblFooter.Text = "ONVIF  •  Yerel ağ yönetimi";
            lblFooter.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnManage
            // 
            btnManage.Dock = DockStyle.Top;
            btnManage.Location = new Point(0, 196);
            btnManage.Name = "btnManage";
            btnManage.Size = new Size(232, 48);
            btnManage.TabIndex = 4;
            btnManage.Text = "  ▦    Kamera yönetimi";
            btnManage.UseVisualStyleBackColor = false;
            btnManage.Click += btnManage_Click;
            // 
            // btnHome
            // 
            btnHome.Dock = DockStyle.Top;
            btnHome.Location = new Point(0, 148);
            btnHome.Name = "btnHome";
            btnHome.Size = new Size(232, 48);
            btnHome.TabIndex = 3;
            btnHome.Text = "  ◉    Canlı izleme";
            btnHome.UseVisualStyleBackColor = false;
            btnHome.Click += btnHome_Click;
            // 
            // lblNavSection
            // 
            lblNavSection.Dock = DockStyle.Top;
            lblNavSection.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblNavSection.ForeColor = Color.FromArgb(131, 149, 173);
            lblNavSection.Padding = new Padding(22, 18, 0, 0);
            lblNavSection.Size = new Size(232, 42);
            lblNavSection.Text = "ÇALIŞMA ALANI";
            lblNavSection.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBrandCaption
            // 
            lblBrandCaption.Dock = DockStyle.Top;
            lblBrandCaption.Font = new Font("Segoe UI", 8.5F);
            lblBrandCaption.ForeColor = Color.FromArgb(142, 163, 191);
            lblBrandCaption.Padding = new Padding(23, 0, 0, 0);
            lblBrandCaption.Size = new Size(232, 25);
            lblBrandCaption.Text = "CAMERA MANAGER";
            lblBrandCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblBrand
            // 
            lblBrand.Dock = DockStyle.Top;
            lblBrand.Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold);
            lblBrand.ForeColor = Color.White;
            lblBrand.Padding = new Padding(20, 24, 0, 0);
            lblBrand.Size = new Size(232, 66);
            lblBrand.Text = "ONVIF";
            lblBrand.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlWorkspace
            // 
            pnlWorkspace.BackColor = Color.FromArgb(243, 246, 250);
            pnlWorkspace.Controls.Add(pnlContainer);
            pnlWorkspace.Controls.Add(pnlTopbar);
            pnlWorkspace.Dock = DockStyle.Fill;
            pnlWorkspace.Padding = new Padding(0);
            // 
            // pnlContainer
            // 
            pnlContainer.Dock = DockStyle.Fill;
            pnlContainer.Padding = new Padding(26, 22, 26, 24);
            pnlContainer.BackColor = Color.FromArgb(243, 246, 250);
            // 
            // pnlTopbar
            // 
            pnlTopbar.BackColor = Color.White;
            pnlTopbar.Controls.Add(lblCameraCount);
            pnlTopbar.Controls.Add(lblPageSubtitle);
            pnlTopbar.Controls.Add(lblPageTitle);
            pnlTopbar.Dock = DockStyle.Top;
            pnlTopbar.Padding = new Padding(28, 0, 28, 0);
            pnlTopbar.Size = new Size(1048, 94);
            // 
            // lblCameraCount
            // 
            lblCameraCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblCameraCount.BackColor = Color.FromArgb(238, 243, 250);
            lblCameraCount.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblCameraCount.ForeColor = Color.FromArgb(55, 83, 118);
            lblCameraCount.Location = new Point(897, 28);
            lblCameraCount.Name = "lblCameraCount";
            lblCameraCount.Size = new Size(122, 34);
            lblCameraCount.TabIndex = 2;
            lblCameraCount.Text = "0 kamera";
            lblCameraCount.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblPageSubtitle
            // 
            lblPageSubtitle.AutoSize = true;
            lblPageSubtitle.Font = new Font("Segoe UI", 9F);
            lblPageSubtitle.ForeColor = Color.FromArgb(111, 124, 143);
            lblPageSubtitle.Location = new Point(31, 56);
            lblPageSubtitle.Name = "lblPageSubtitle";
            lblPageSubtitle.Size = new Size(0, 15);
            lblPageSubtitle.TabIndex = 1;
            // 
            // lblPageTitle
            // 
            lblPageTitle.AutoSize = true;
            lblPageTitle.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            lblPageTitle.ForeColor = Color.FromArgb(30, 43, 61);
            lblPageTitle.Location = new Point(28, 16);
            lblPageTitle.Name = "lblPageTitle";
            lblPageTitle.Size = new Size(0, 31);
            lblPageTitle.TabIndex = 0;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 246, 250);
            ClientSize = new Size(1280, 760);
            Controls.Add(pnlWorkspace);
            Controls.Add(pnlSidebar);
            Font = new Font("Segoe UI", 9F);
            Name = "FormMain";
            Text = "ONVIF Camera Manager";
            FormClosing += FormMain_FormClosing;
            pnlSidebar.ResumeLayout(false);
            pnlWorkspace.ResumeLayout(false);
            pnlTopbar.ResumeLayout(false);
            pnlTopbar.PerformLayout();
            ResumeLayout(false);
        }
    }
}
