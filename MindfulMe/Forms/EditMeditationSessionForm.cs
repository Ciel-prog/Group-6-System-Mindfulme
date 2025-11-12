using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MindfulMe.Models;

namespace MindfulMe.Forms
{
    public class EditMeditationSessionForm : Form
    {
        private readonly MeditationSession _session;
        public MeditationSession UpdatedSession { get; private set; }

        private DateTimePicker dtpSessionDate = new DateTimePicker();
        private NumericUpDown nudDuration = new NumericUpDown();
        private ComboBox cmbType = new ComboBox();
        private NumericUpDown nudQuality = new NumericUpDown();
        private Button btnSave = new Button();
        private Button btnCancel = new Button();

        // Enhanced theme
        private readonly Color PrimaryBlue = Color.FromArgb(24, 119, 242);
        private readonly Color HoverBlue = Color.FromArgb(8, 102, 220);
        private readonly Color LightBlueBg = Color.FromArgb(247, 250, 252);
        private readonly Color DarkText = Color.FromArgb(26, 32, 44);
        private readonly Color GrayText = Color.FromArgb(74, 85, 104);
        private readonly Color BorderColor = Color.FromArgb(226, 232, 240);

        private readonly Font TitleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        private readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font LabelFont = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        private readonly Font ButtonFont = new Font("Segoe UI", 10F, FontStyle.Bold);

        public EditMeditationSessionForm(MeditationSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            UpdatedSession = new MeditationSession
            {
                Id = _session.Id,
                UserId = _session.UserId,
                SessionDate = _session.SessionDate,
                DurationMinutes = _session.DurationMinutes,
                MeditationType = _session.MeditationType,
                QualityRating = _session.QualityRating
            };

            InitializeComponents();
            LoadValues();
        }

        private void InitializeComponents()
        {
            Text = "Edit Meditation Session";
            Width = 600;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = LightBlueBg;
            Font = BodyFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Padding = new Padding(20);

            // Card
            var card = new Panel
            {
                Size = new Size(520, 400),
                Location = new Point((ClientSize.Width - 520) / 2, (ClientSize.Height - 400) / 2),
                BackColor = Color.White,
                Padding = new Padding(30)
            };
            card.Anchor = AnchorStyles.None;
            card.Paint += (s, e) =>
            {
                using (var shadow = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadow, 3, 3, card.Width - 3, card.Height - 3);
                }
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 16))
                using (var pen = new Pen(BorderColor, 1))
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };

            var lblTitle = new Label
            {
                Text = "🧘 Edit Meditation Session",
                Font = TitleFont,
                ForeColor = DarkText,
                Location = new Point(15, 15),
                Size = new Size(460, 35),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Date
            var lblDate = new Label { Text = "Session Date", Location = new Point(15, 70), Size = new Size(460, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            dtpSessionDate.Location = new Point(15, 95);
            dtpSessionDate.Size = new Size(280, 32);
            dtpSessionDate.Font = BodyFont;

            // Duration
            var lblDuration = new Label { Text = "Duration (minutes)", Location = new Point(15, 145), Size = new Size(150, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            nudDuration.Location = new Point(15, 170);
            nudDuration.Size = new Size(120, 32);
            nudDuration.Minimum = 1; nudDuration.Maximum = 120; nudDuration.Font = BodyFont;

            // Type
            var lblType = new Label { Text = "Meditation Type", Location = new Point(165, 145), Size = new Size(150, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            cmbType.Location = new Point(165, 170);
            cmbType.Size = new Size(280, 32);
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Font = BodyFont;
            cmbType.Items.AddRange(new string[] { "Breathing", "Guided", "Mindfulness", "Body Scan", "Loving-Kindness" });

            // Quality
            var lblQuality = new Label { Text = "Quality Rating (1-5 stars)", Location = new Point(15, 220), Size = new Size(200, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            nudQuality.Location = new Point(15, 245);
            nudQuality.Size = new Size(120, 32);
            nudQuality.Minimum = 1; nudQuality.Maximum = 5; nudQuality.Font = BodyFont;

            // Buttons
            btnSave.Text = "💾 Save Changes";
            btnSave.Size = new Size(170, 48);
            btnSave.Location = new Point(15, 310);
            btnSave.BackColor = PrimaryBlue;
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = ButtonFont;
            btnSave.Cursor = Cursors.Hand;
            btnSave.MouseEnter += (s, e) => btnSave.BackColor = HoverBlue;
            btnSave.MouseLeave += (s, e) => btnSave.BackColor = PrimaryBlue;
            btnSave.Click += BtnSave_Click;

            btnCancel.Text = "Cancel";
            btnCancel.Size = new Size(150, 48);
            btnCancel.Location = new Point(200, 310);
            btnCancel.BackColor = Color.White;
            btnCancel.ForeColor = PrimaryBlue;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 2;
            btnCancel.FlatAppearance.BorderColor = PrimaryBlue;
            btnCancel.Font = ButtonFont;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.MouseEnter += (s, e) => btnCancel.BackColor = Color.FromArgb(240, 247, 255);
            btnCancel.MouseLeave += (s, e) => btnCancel.BackColor = Color.White;
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            // Keyboard
            AcceptButton = btnSave;
            CancelButton = btnCancel;

            // Add controls
            card.Controls.AddRange(new Control[] {
                lblTitle,
                lblDate, dtpSessionDate,
                lblDuration, nudDuration,
                lblType, cmbType,
                lblQuality, nudQuality,
                btnSave, btnCancel
            });

            Controls.Add(card);
        }

        private void LoadValues()
        {
            dtpSessionDate.Value = _session.SessionDate;
            nudDuration.Value = Math.Max(nudDuration.Minimum, Math.Min(nudDuration.Maximum, _session.DurationMinutes));

            // Set meditation type
            var typeIndex = cmbType.FindStringExact(_session.MeditationType);
            cmbType.SelectedIndex = typeIndex >= 0 ? typeIndex : 0;

            nudQuality.Value = Math.Max(nudQuality.Minimum, Math.Min(nudQuality.Maximum, _session.QualityRating));
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (cmbType.SelectedItem == null)
            {
                MessageBox.Show("Please select a meditation type.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UpdatedSession.SessionDate = dtpSessionDate.Value;
            UpdatedSession.DurationMinutes = (int)nudDuration.Value;
            UpdatedSession.MeditationType = cmbType.SelectedItem.ToString() ?? "Breathing";
            UpdatedSession.QualityRating = (int)nudQuality.Value;

            DialogResult = DialogResult.OK;
            Close();
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