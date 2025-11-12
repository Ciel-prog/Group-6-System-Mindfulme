using MindfulMe.Services;
using MindfulMe.Models;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MindfulMe.Forms
{
    public partial class LoginForm : Form
    {
        private readonly ApiService _apiService;

        public User? LoggedInUser { get; private set; }

        // Enhanced theme colors
        private readonly Color PrimaryBlue = Color.FromArgb(24, 119, 242);
        private readonly Color HoverBlue = Color.FromArgb(8, 102, 220);
        private readonly Color LightBlueBg = Color.FromArgb(247, 250, 252);
        private readonly Color DarkText = Color.FromArgb(26, 32, 44);
        private readonly Color GrayText = Color.FromArgb(74, 85, 104);
        private readonly Color BorderColor = Color.FromArgb(226, 232, 240);

        // Enhanced fonts
        private readonly Font TitleFont = new Font("Segoe UI", 24F, FontStyle.Bold);
        private readonly Font SubtitleFont = new Font("Segoe UI", 11F, FontStyle.Regular);
        private readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font LabelFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font ButtonFont = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font SmallFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);

        public LoginForm()
        {
            InitializeComponent();
            _apiService = new ApiService();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form setup
            this.Text = "MindfulMe - Login";
            this.Size = new Size(500, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = LightBlueBg;
            this.Font = BodyFont;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Padding = new Padding(20);

            // Card panel (centered with shadow)
            var card = new Panel
            {
                Size = new Size(420, 540),
                Location = new Point((this.ClientSize.Width - 420) / 2, (this.ClientSize.Height - 540) / 2 - 10),
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

            // App icon/emoji
            var lblIcon = new Label
            {
                Text = "🧘‍♀️",
                Location = new Point(160, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Emoji", 48F),
                BackColor = Color.Transparent
            };

            // Title
            var lblTitle = new Label
            {
                Text = "Welcome Back",
                Location = new Point(40, 100),
                AutoSize = false,
                Size = new Size(300, 40),
                Font = TitleFont,
                ForeColor = DarkText,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = "Sign in to continue your wellness journey",
                Location = new Point(40, 140),
                AutoSize = false,
                Size = new Size(300, 25),
                Font = SubtitleFont,
                ForeColor = GrayText,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Username section
            var lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(40, 185),
                Size = new Size(300, 20),
                ForeColor = DarkText,
                Font = LabelFont,
                BackColor = Color.Transparent
            };

            var txtUsername = new TextBox
            {
                Location = new Point(40, 210),
                Size = new Size(300, 38),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(BodyFont.FontFamily, 11F),
                Text = "demo",
                BackColor = Color.White
            };
            txtUsername.GotFocus += (s, e) => txtUsername.BackColor = Color.FromArgb(240, 247, 255);
            txtUsername.LostFocus += (s, e) => txtUsername.BackColor = Color.White;

            // Password section
            var lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(40, 265),
                Size = new Size(300, 20),
                ForeColor = DarkText,
                Font = LabelFont,
                BackColor = Color.Transparent
            };

            var txtPassword = new TextBox
            {
                Location = new Point(40, 290),
                Size = new Size(300, 38),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font(BodyFont.FontFamily, 11F),
                UseSystemPasswordChar = true,
                Text = "demo",
                BackColor = Color.White
            };
            txtPassword.GotFocus += (s, e) => txtPassword.BackColor = Color.FromArgb(240, 247, 255);
            txtPassword.LostFocus += (s, e) => txtPassword.BackColor = Color.White;

            // Login button
            var btnLogin = new Button
            {
                Text = "Sign In",
                Location = new Point(40, 350),
                Size = new Size(300, 48),
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                TabStop = true
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = HoverBlue;
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = PrimaryBlue;
            btnLogin.Click += async (s, e) =>
            {
                btnLogin.Enabled = false;
                btnLogin.Text = "Signing in...";

                try
                {
                    var (success, message, user) = await _apiService.LoginAsync(txtUsername.Text, txtPassword.Text);

                    if (success && user != null)
                    {
                        LoggedInUser = user;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(message ?? "Invalid username or password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Sign In";
                }
            };

            // Divider with text
            var pnlDivider = new Panel
            {
                Location = new Point(40, 415),
                Size = new Size(300, 1),
                BackColor = BorderColor
            };

            var lblOr = new Label
            {
                Text = "OR",
                Location = new Point(165, 407),
                Size = new Size(50, 20),
                BackColor = Color.White,
                ForeColor = GrayText,
                Font = SmallFont,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Sign up button
            var btnSignUp = new Button
            {
                Text = "Create New Account",
                Location = new Point(40, 435),
                Size = new Size(300, 48),
                BackColor = Color.White,
                ForeColor = PrimaryBlue,
                FlatStyle = FlatStyle.Flat,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                TabStop = true
            };
            btnSignUp.FlatAppearance.BorderSize = 2;
            btnSignUp.FlatAppearance.BorderColor = PrimaryBlue;
            btnSignUp.MouseEnter += (s, e) => btnSignUp.BackColor = Color.FromArgb(240, 247, 255);
            btnSignUp.MouseLeave += (s, e) => btnSignUp.BackColor = Color.White;
            btnSignUp.Click += async (s, e) =>
            {
                using var signup = new SignupForm();
                var dr = signup.ShowDialog(this);
                if (dr == DialogResult.OK && signup.CreatedUser != null)
                {
                    LoggedInUser = await _apiService.GetUserByIdAsync(signup.CreatedUser.Id);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            };

            // Demo info label (now inside card)
            var lblDemo = new Label
            {
                Text = "💡 Demo: username='demo', password='demo'",
                Location = new Point(40, 495),
                Size = new Size(300, 30),
                ForeColor = Color.FromArgb(138, 116, 30),
                Font = SmallFont,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(255, 248, 225),
                Padding = new Padding(8, 6, 8, 6)
            };
            lblDemo.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(new Rectangle(0, 0, lblDemo.Width - 1, lblDemo.Height - 1), 6))
                using (var brush = new SolidBrush(lblDemo.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            };

            // Make Enter trigger login
            this.AcceptButton = btnLogin;

            // Add controls to card
            card.Controls.AddRange(new Control[]
            {
                lblIcon, lblTitle, lblSubtitle,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnLogin,
                pnlDivider, lblOr,
                btnSignUp,
                lblDemo
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