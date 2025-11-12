using MindfulMe.Services;
using MindfulMe.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MindfulMe.Forms
{
    public partial class SignupForm : Form
    {
        private readonly ApiService _apiService;

        public User? CreatedUser { get; private set; }

        // Enhanced theme colors
        private readonly Color PrimaryBlue = Color.FromArgb(24, 119, 242);
        private readonly Color HoverBlue = Color.FromArgb(8, 102, 220);
        private readonly Color LightBlueBg = Color.FromArgb(247, 250, 252);
        private readonly Color DarkText = Color.FromArgb(26, 32, 44);
        private readonly Color GrayText = Color.FromArgb(74, 85, 104);
        private readonly Color BorderColor = Color.FromArgb(226, 232, 240);

        // Enhanced fonts
        private readonly Font TitleFont = new Font("Segoe UI", 22F, FontStyle.Bold);
        private readonly Font SubtitleFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font LabelFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font ButtonFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font SmallFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);

        public SignupForm()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "MindfulMe - Create Account";
            this.Size = new Size(550, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = LightBlueBg;
            this.Font = BodyFont;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Padding = new Padding(20);

            // Card panel
            var card = new Panel
            {
                Size = new Size(480, 640),
                Location = new Point((this.ClientSize.Width - 480) / 2, (this.ClientSize.Height - 640) / 2),
                BackColor = Color.White,
                Padding = new Padding(40, 40, 40, 40)
            };
            card.Anchor = AnchorStyles.None;
            card.Paint += (s, e) =>
            {
                // Draw shadow
                using (var shadow = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadow, 3, 3, card.Width - 3, card.Height - 3);
                }
                // Draw rounded card
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 16))
                using (var pen = new Pen(BorderColor, 1))
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // Icon
            var lblIcon = new Label
            {
                Text = "✨",
                Location = new Point(170, 15),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 40F),
                BackColor = Color.Transparent
            };

            // Title
            var lblTitle = new Label
            {
                Text = "Create Your Account",
                Location = new Point(0, 75),
                AutoSize = false,
                Size = new Size(400, 35),
                Font = TitleFont,
                ForeColor = DarkText,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = "Start your journey to mindfulness and wellness",
                Location = new Point(0, 110),
                AutoSize = false,
                Size = new Size(400, 25),
                Font = SubtitleFont,
                ForeColor = GrayText,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Username
            var lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(0, 155),
                Size = new Size(400, 20),
                ForeColor = DarkText,
                Font = LabelFont,
                BackColor = Color.Transparent
            };
            var txtUsername = new TextBox
            {
                Location = new Point(0, 180),
                Size = new Size(400, 38),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(BodyFont.FontFamily, 11F),
                BackColor = Color.White
            };
            txtUsername.GotFocus += (s, e) => txtUsername.BackColor = Color.FromArgb(240, 247, 255);
            txtUsername.LostFocus += (s, e) => txtUsername.BackColor = Color.White;

            // Email
            var lblEmail = new Label
            {
                Text = "Email Address",
                Location = new Point(0, 230),
                Size = new Size(400, 20),
                ForeColor = DarkText,
                Font = LabelFont,
                BackColor = Color.Transparent
            };
            var txtEmail = new TextBox
            {
                Location = new Point(0, 255),
                Size = new Size(400, 38),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(BodyFont.FontFamily, 11F),
                BackColor = Color.White
            };
            txtEmail.GotFocus += (s, e) => txtEmail.BackColor = Color.FromArgb(240, 247, 255);
            txtEmail.LostFocus += (s, e) => txtEmail.BackColor = Color.White;

            // Password
            var lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(0, 305),
                Size = new Size(400, 20),
                ForeColor = DarkText,
                Font = LabelFont,
                BackColor = Color.Transparent
            };
            var txtPassword = new TextBox
            {
                Location = new Point(0, 330),
                Size = new Size(400, 38),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(BodyFont.FontFamily, 11F),
                UseSystemPasswordChar = true,
                BackColor = Color.White
            };
            txtPassword.GotFocus += (s, e) => txtPassword.BackColor = Color.FromArgb(240, 247, 255);
            txtPassword.LostFocus += (s, e) => txtPassword.BackColor = Color.White;

            // Confirm Password
            var lblConfirm = new Label
            {
                Text = "Confirm Password",
                Location = new Point(0, 380),
                Size = new Size(400, 20),
                ForeColor = DarkText,
                Font = LabelFont,
                BackColor = Color.Transparent
            };
            var txtConfirm = new TextBox
            {
                Location = new Point(0, 405),
                Size = new Size(400, 38),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(BodyFont.FontFamily, 11F),
                UseSystemPasswordChar = true,
                BackColor = Color.White
            };
            txtConfirm.GotFocus += (s, e) => txtConfirm.BackColor = Color.FromArgb(240, 247, 255);
            txtConfirm.LostFocus += (s, e) => txtConfirm.BackColor = Color.White;

            // Register button
            var btnRegister = new Button
            {
                Text = "Create Account",
                Location = new Point(0, 470),
                Size = new Size(400, 48),
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                TabStop = true
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.MouseEnter += (s, e) => btnRegister.BackColor = HoverBlue;
            btnRegister.MouseLeave += (s, e) => btnRegister.BackColor = PrimaryBlue;
            btnRegister.Click += async (s, e) =>
            {
                var username = txtUsername.Text.Trim();
                var email = txtEmail.Text.Trim();
                var password = txtPassword.Text;
                var confirm = txtConfirm.Text;

                // Validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirm))
                {
                    MessageBox.Show("Please fill in all fields.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (password != confirm)
                {
                    MessageBox.Show("Passwords do not match.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (password.Length < 4)
                {
                    MessageBox.Show("Password must be at least 4 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!email.Contains("@"))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnRegister.Enabled = false;
                btnRegister.Text = "Creating account...";

                try
                {
                    var (success, message, user) = await _apiService.RegisterAsync(username, email, password);

                    if (!success)
                    {
                        MessageBox.Show(message ?? "Registration failed.", "Registration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    CreatedUser = user;
                    MessageBox.Show("Account created successfully! Welcome to MindfulMe! 🎉", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnRegister.Enabled = true;
                    btnRegister.Text = "Create Account";
                }
            };

            // Cancel button
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(0, 535),
                Size = new Size(400, 48),
                BackColor = Color.White,
                ForeColor = PrimaryBlue,
                FlatStyle = FlatStyle.Flat,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                TabStop = true
            };
            btnCancel.FlatAppearance.BorderSize = 2;
            btnCancel.FlatAppearance.BorderColor = PrimaryBlue;
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(240, 247, 255);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.White;
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            // Info label
            var lblInfo = new Label
            {
                Text = "🔒 Your data is secure and will be synced across devices",
                Location = new Point(0, 595),
                Size = new Size(400, 30),
                ForeColor = GrayText,
                Font = SmallFont,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Keyboard shortcuts
            this.AcceptButton = btnRegister;
            this.CancelButton = btnCancel;

            // Add controls to card
            card.Controls.AddRange(new Control[] {
                lblIcon, lblTitle, lblSubtitle,
                lblUsername, txtUsername,
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                lblConfirm, txtConfirm,
                btnRegister, btnCancel,
                lblInfo
            });

            this.Controls.Add(card);
            this.ResumeLayout(false);
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}