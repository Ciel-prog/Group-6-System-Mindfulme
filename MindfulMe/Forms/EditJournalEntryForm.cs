using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MindfulMe.Models;

namespace MindfulMe.Forms
{
    public class EditJournalEntryForm : Form
    {
        private readonly JournalEntry _entry;
        public JournalEntry UpdatedEntry { get; private set; }

        private DateTimePicker dtpEntryDate = new DateTimePicker();
        private TextBox txtContent = new TextBox();
        private NumericUpDown nudMood = new NumericUpDown();
        private NumericUpDown nudStress = new NumericUpDown();
        private NumericUpDown nudHours = new NumericUpDown();
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

        public EditJournalEntryForm(JournalEntry entry)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            UpdatedEntry = new JournalEntry
            {
                Id = _entry.Id,
                UserId = _entry.UserId,
                EntryDate = _entry.EntryDate,
                Content = _entry.Content,
                MoodScore = _entry.MoodScore,
                StressLevel = _entry.StressLevel,
                HoursSlept = _entry.HoursSlept
            };

            InitializeComponents();
            LoadValues();
        }

        private void InitializeComponents()
        {
            Text = "Edit Journal Entry";
            Width = 700;
            Height = 640;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = LightBlueBg;
            Font = BodyFont;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Padding = new Padding(20);

            // Card
            var card = new Panel
            {
                Size = new Size(620, 540),
                Location = new Point((ClientSize.Width - 620) / 2, (ClientSize.Height - 540) / 2),
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
                Text = "✍️ Edit Journal Entry",
                Font = TitleFont,
                ForeColor = DarkText,
                Location = new Point(15, 15),
                Size = new Size(560, 35),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Date
            var lblDate = new Label { Text = "Entry Date", Location = new Point(15, 70), Size = new Size(560, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            dtpEntryDate.Location = new Point(15, 95);
            dtpEntryDate.Size = new Size(280, 32);
            dtpEntryDate.Font = BodyFont;

            // Content
            var lblContent = new Label { Text = "Journal Content", Location = new Point(15, 145), Size = new Size(560, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            txtContent.Location = new Point(15, 170);
            txtContent.Size = new Size(560, 180);
            txtContent.Multiline = true;
            txtContent.ScrollBars = ScrollBars.Vertical;
            txtContent.BorderStyle = BorderStyle.FixedSingle;
            txtContent.Font = BodyFont;
            txtContent.ForeColor = DarkText;
            txtContent.BackColor = Color.White;
            txtContent.GotFocus += (s, e) => txtContent.BackColor = Color.FromArgb(240, 247, 255);
            txtContent.LostFocus += (s, e) => txtContent.BackColor = Color.White;

            // Mood / Stress / Hours row
            var lblMood = new Label { Text = "Mood (1-10)", Location = new Point(15, 370), Size = new Size(100, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            nudMood.Location = new Point(15, 395);
            nudMood.Size = new Size(90, 32);
            nudMood.Minimum = 1; nudMood.Maximum = 10; nudMood.Font = BodyFont;

            var lblStress = new Label { Text = "Stress (1-10)", Location = new Point(135, 370), Size = new Size(110, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            nudStress.Location = new Point(135, 395);
            nudStress.Size = new Size(90, 32);
            nudStress.Minimum = 1; nudStress.Maximum = 10; nudStress.Font = BodyFont;

            var lblHours = new Label { Text = "Hours Slept", Location = new Point(255, 370), Size = new Size(100, 20), ForeColor = DarkText, Font = LabelFont, BackColor = Color.Transparent };
            nudHours.Location = new Point(255, 395);
            nudHours.Size = new Size(110, 32);
            nudHours.Minimum = 0; nudHours.Maximum = 24; nudHours.DecimalPlaces = 1; nudHours.Increment = 0.5M; nudHours.Font = BodyFont;

            // Buttons
            btnSave.Text = "💾 Save Changes";
            btnSave.Size = new Size(170, 48);
            btnSave.Location = new Point(15, 455);
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
            btnCancel.Location = new Point(200, 455);
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
                lblDate, dtpEntryDate,
                lblContent, txtContent,
                lblMood, nudMood,
                lblStress, nudStress,
                lblHours, nudHours,
                btnSave, btnCancel
            });

            Controls.Add(card);
        }

        private void LoadValues()
        {
            dtpEntryDate.Value = _entry.EntryDate;
            txtContent.Text = _entry.Content;
            nudMood.Value = Math.Max(nudMood.Minimum, Math.Min(nudMood.Maximum, _entry.MoodScore));
            nudStress.Value = Math.Max(nudStress.Minimum, Math.Min(nudStress.Maximum, _entry.StressLevel));
            nudHours.Value = (decimal)Math.Max((double)nudHours.Minimum, Math.Min((double)nudHours.Maximum, _entry.HoursSlept));
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtContent.Text))
            {
                MessageBox.Show("Content cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            UpdatedEntry.EntryDate = dtpEntryDate.Value;
            UpdatedEntry.Content = txtContent.Text.Trim();
            UpdatedEntry.MoodScore = (int)nudMood.Value;
            UpdatedEntry.StressLevel = (int)nudStress.Value;
            UpdatedEntry.HoursSlept = (double)nudHours.Value;

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