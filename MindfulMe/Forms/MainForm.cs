using MindfulMe.Forms;
using MindfulMe.Models;
using MindfulMe.Services;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.Json;

namespace MindfulMe
{
    public partial class MainForm : Form
    {
        private User _currentUser;
        private readonly ApiService _apiService;
        private readonly AnalyticsService _analyticsService;
        private readonly RecommendationService _recommendationService;

        private TabControl tabControl;
        private DataGridView dgvJournal;
        private DataGridView dgvMeditation;
        private DataGridView dgvGoals;
        private Label lblRecommendation;
        private Label lblStats;

        // Goals controls
        private NumericUpDown numJournalGoal;
        private NumericUpDown numMeditationGoal;
        private Button btnSaveGoals;
        private Button btnResetGoals;
        private Label lblJournalProgress;
        private Label lblMeditationProgress;
        private ProgressBar pbJournal;
        private ProgressBar pbMeditation;

        // Panic/Anxiety helper fields
        private PanicAttackHelper _panicHelper;
        private TextBox txtGrounding;
        private Button btnNextGrounding;
        private Button btnStartGrounding;
        private int _currentGroundingIndex;
        private NumericUpDown numBreathsPerMinute;
        private Button btnStartPacer;
        private Button btnStopPacer;
        private Label lblPacerDisplay;
        private System.Windows.Forms.Timer _breathingTimer;
        private bool _isInhale;
        private int _pacerIntervalMs;

        // Enhanced theme colors
        private readonly Color PrimaryBlue = Color.FromArgb(24, 119, 242);
        private readonly Color HoverBlue = Color.FromArgb(8, 102, 220);
        private readonly Color AccentBlue = Color.FromArgb(66, 153, 225);
        private readonly Color LightBlueBg = Color.FromArgb(247, 250, 252);
        private readonly Color DarkText = Color.FromArgb(26, 32, 44);
        private readonly Color GrayText = Color.FromArgb(74, 85, 104);
        private readonly Color BorderColor = Color.FromArgb(226, 232, 240);

        // Enhanced fonts
        private readonly Font TitleFont = new Font("Segoe UI", 18F, FontStyle.Bold);
        private readonly Font SubtitleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Font BodyFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font SmallFont = new Font("Segoe UI", 9F, FontStyle.Regular);
        private readonly Font ButtonFont = new Font("Segoe UI", 10F, FontStyle.Bold);

        public MainForm(User user)
        {
            _currentUser = user;
            _apiService = new ApiService();
            _analyticsService = new AnalyticsService(_apiService);
            _recommendationService = new RecommendationService();

            _panicHelper = new PanicAttackHelper();
            _breathingTimer = new System.Windows.Forms.Timer();
            _breathingTimer.Tick += BreathingTimer_Tick;

            InitializeComponent();
            _ = LoadDataAsync();
            _ = UpdateRecommendationAsync();
            _ = UpdateStatsAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Main form setup
            this.Text = $"MindfulMe - Welcome, {_currentUser.Username}";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = LightBlueBg;
            this.Font = BodyFont;
            this.Padding = new Padding(20);
            this.MinimumSize = new Size(1000, 700);

            // Tab control (modern look)
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(160, 42),
                Padding = new Point(16, 8),
                Font = SubtitleFont
            };
            tabControl.DrawItem += TabControl_DrawItem;

            // Dashboard Tab
            var tabDashboard = new TabPage { Text = "Dashboard", BackColor = LightBlueBg };
            InitializeDashboardTab(tabDashboard);

            // Journal Tab
            var tabJournal = new TabPage { Text = "Journal", BackColor = LightBlueBg };
            InitializeJournalTab(tabJournal);

            // Meditation Tab
            var tabMeditation = new TabPage { Text = "Meditation", BackColor = LightBlueBg };
            InitializeMeditationTab(tabMeditation);

            // Analytics Tab
            var tabAnalytics = new TabPage { Text = "Analytics", BackColor = LightBlueBg };
            InitializeAnalyticsTab(tabAnalytics);

            // Goals Tab
            var tabGoals = new TabPage { Text = "Goals", BackColor = LightBlueBg };
            InitializeGoalsTab(tabGoals);

            // Anxiety & Panic Helper Tab
            var tabAnxiety = new TabPage { Text = "Calm & Breathe", BackColor = LightBlueBg };
            InitializeAnxietyTab(tabAnxiety);

            tabControl.TabPages.AddRange(new TabPage[] { tabDashboard, tabJournal, tabMeditation, tabAnalytics, tabGoals, tabAnxiety });
            this.Controls.Add(tabControl);

            this.ResumeLayout(false);
        }

        private Panel CreateCard(int x, int y, int width, int height)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            panel.Paint += (s, e) =>
            {
                // Draw subtle shadow
                using (var shadow = new SolidBrush(Color.FromArgb(15, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(shadow, 2, 2, width - 2, height - 2);
                }
                // Draw rounded rectangle
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(new Rectangle(0, 0, width - 1, height - 1), 12))
                using (var pen = new Pen(BorderColor, 1))
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            return panel;
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

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private Button CreateModernButton(string text, EventHandler onClick, bool isPrimary = true)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(140, 42),
                BackColor = isPrimary ? PrimaryBlue : Color.White,
                ForeColor = isPrimary ? Color.White : PrimaryBlue,
                FlatStyle = FlatStyle.Flat,
                Font = ButtonFont,
                Cursor = Cursors.Hand,
                TabStop = true
            };
            btn.FlatAppearance.BorderSize = isPrimary ? 0 : 2;
            btn.FlatAppearance.BorderColor = PrimaryBlue;

            // Hover effects
            btn.MouseEnter += (s, e) => {
                btn.BackColor = isPrimary ? HoverBlue : Color.FromArgb(240, 247, 255);
            };
            btn.MouseLeave += (s, e) => {
                btn.BackColor = isPrimary ? PrimaryBlue : Color.White;
            };

            if (onClick != null)
                btn.Click += onClick;

            return btn;
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = BorderColor;
            dgv.RowTemplate.Height = 36;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.Font = BodyFont;

            // Header styling
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 252);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = DarkText;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(12, 0, 0, 0);
            dgv.RowHeadersVisible = false;

            // Row styling
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = DarkText;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 244, 255);
            dgv.DefaultCellStyle.SelectionForeColor = DarkText;
            dgv.DefaultCellStyle.Padding = new Padding(12, 4, 4, 4);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(252, 252, 253);

            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        private void TabControl_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tc = (TabControl)sender!;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = e.Bounds;
            var tab = tc.TabPages[e.Index];
            bool selected = (tc.SelectedIndex == e.Index);

            // Background with rounded top
            using (var path = new GraphicsPath())
            {
                int radius = 8;
                path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
                path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
                path.AddLine(bounds.Right, bounds.Bottom, bounds.X, bounds.Bottom);
                path.CloseFigure();

                using (var backBrush = new SolidBrush(selected ? Color.White : LightBlueBg))
                {
                    g.FillPath(backBrush, path);
                }

                if (selected)
                {
                    using (var pen = new Pen(BorderColor, 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            }

            // Text
            var text = tab.Text;
            var textColor = selected ? PrimaryBlue : GrayText;
            var textFont = selected ? new Font(SubtitleFont.FontFamily, 11F, FontStyle.Bold) : SubtitleFont;
            var textRect = new Rectangle(bounds.X + 16, bounds.Y + 10, bounds.Width - 16, bounds.Height - 10);
            TextRenderer.DrawText(g, text, textFont, textRect, textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
        }

        private void InitializeDashboardTab(TabPage tab)
        {
            var cardRec = CreateCard(20, 20, 760, 140);

            var lblRecTitle = new Label
            {
                Text = "💡 Daily Recommendation",
                Location = new Point(20, 20),
                Size = new Size(720, 30),
                Font = SubtitleFont,
                ForeColor = GrayText
            };

            lblRecommendation = new Label
            {
                Location = new Point(20, 55),
                Size = new Size(720, 60),
                Font = TitleFont,
                ForeColor = DarkText,
                AutoSize = false
            };

            cardRec.Controls.AddRange(new Control[] { lblRecTitle, lblRecommendation });

            var cardStats = CreateCard(20, 180, 760, 160);

            var lblStatsTitle = new Label
            {
                Text = "📊 Your Statistics",
                Location = new Point(20, 20),
                Size = new Size(720, 30),
                Font = SubtitleFont,
                ForeColor = GrayText
            };

            lblStats = new Label
            {
                Location = new Point(20, 55),
                Size = new Size(720, 80),
                Font = BodyFont,
                ForeColor = DarkText,
                AutoSize = false
            };

            var btnRefresh = CreateModernButton("🔄 Refresh", async (s, e) => await UpdateRecommendationAsync());
            btnRefresh.Location = new Point(20, 360);
            btnRefresh.Size = new Size(180, 48);

            cardStats.Controls.AddRange(new Control[] { lblStatsTitle, lblStats });
            tab.Controls.AddRange(new Control[] { cardRec, cardStats, btnRefresh });
        }

        private void InitializeJournalTab(TabPage tab)
        {
            var cardInput = CreateCard(20, 20, 560, 420);

            var lblTitle = new Label
            {
                Text = "✍️ New Journal Entry",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            // Input controls
            var lblMood = new Label { Text = "Mood (1-10):", Location = new Point(20, 70), Size = new Size(110, 24), Font = BodyFont, ForeColor = GrayText };
            var numMood = new NumericUpDown { Location = new Point(140, 68), Size = new Size(80, 28), Minimum = 1, Maximum = 10, Value = 5, Font = BodyFont };

            var lblStress = new Label { Text = "Stress (1-10):", Location = new Point(250, 70), Size = new Size(110, 24), Font = BodyFont, ForeColor = GrayText };
            var numStress = new NumericUpDown { Location = new Point(370, 68), Size = new Size(80, 28), Minimum = 1, Maximum = 10, Value = 5, Font = BodyFont };

            var lblSleep = new Label { Text = "Hours Slept:", Location = new Point(20, 115), Size = new Size(110, 24), Font = BodyFont, ForeColor = GrayText };
            var numSleep = new NumericUpDown { Location = new Point(140, 113), Size = new Size(80, 28), Minimum = 0, Maximum = 24, Value = 7, DecimalPlaces = 1, Font = BodyFont };

            var lblContent = new Label { Text = "Journal Content:", Location = new Point(20, 160), Size = new Size(520, 24), Font = BodyFont, ForeColor = GrayText };
            var txtContent = new TextBox
            {
                Location = new Point(20, 190),
                Size = new Size(520, 140),
                Multiline = true,
                Font = BodyFont,
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };

            var btnSave = CreateModernButton("💾 Save Entry", null);
            btnSave.Location = new Point(20, 350);
            btnSave.Size = new Size(160, 48);
            btnSave.Click += async (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtContent.Text))
                {
                    MessageBox.Show("Please enter journal content.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                btnSave.Enabled = false;
                btnSave.Text = "Saving...";

                try
                {
                    var entry = await _apiService.CreateJournalEntryAsync(
                        _currentUser.Id,
                        txtContent.Text,
                        (int)numMood.Value,
                        (int)numStress.Value,
                        (double)numSleep.Value
                    );

                    if (entry != null)
                    {
                        MessageBox.Show("Journal entry saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtContent.Clear();
                        await LoadDataAsync();
                        await UpdateRecommendationAsync();
                        await UpdateStatsAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to save entry. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSave.Enabled = true;
                    btnSave.Text = "💾 Save Entry";
                }
            };

            cardInput.Controls.AddRange(new Control[] { lblTitle, lblMood, numMood, lblStress, numStress, lblSleep, numSleep, lblContent, txtContent, btnSave });

            // Data grid view card
            var cardGrid = CreateCard(600, 20, 560, 540);

            var lblGridTitle = new Label
            {
                Text = "📖 Journal Entries",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            dgvJournal = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(520, 400),
                ReadOnly = true
            };

            dgvJournal.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvJournal.MultiSelect = false;
            dgvJournal.AllowUserToAddRows = false;
            StyleDataGridView(dgvJournal);

            var btnEditJournal = CreateModernButton("✏️ Edit", btnEditJournal_Click);
            btnEditJournal.Location = new Point(20, 475);
            btnEditJournal.Size = new Size(130, 42);

            var btnDeleteJournal = CreateModernButton("🗑️ Delete", btnDeleteJournal_Click, false);
            btnDeleteJournal.Location = new Point(165, 475);
            btnDeleteJournal.Size = new Size(130, 42);

            cardGrid.Controls.AddRange(new Control[] { lblGridTitle, dgvJournal, btnEditJournal, btnDeleteJournal });
            tab.Controls.AddRange(new Control[] { cardInput, cardGrid });
        }

        private void InitializeMeditationTab(TabPage tab)
        {
            var cardInput = CreateCard(20, 20, 560, 340);

            var lblTitle = new Label
            {
                Text = "🧘 New Meditation Session",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            var lblDuration = new Label { Text = "Duration (minutes):", Location = new Point(20, 75), Size = new Size(150, 24), Font = BodyFont, ForeColor = GrayText };
            var numDuration = new NumericUpDown { Location = new Point(180, 73), Size = new Size(100, 28), Minimum = 1, Maximum = 120, Value = 10, Font = BodyFont };

            var lblType = new Label { Text = "Meditation Type:", Location = new Point(20, 120), Size = new Size(150, 24), Font = BodyFont, ForeColor = GrayText };
            var cmbType = new ComboBox { Location = new Point(180, 118), Size = new Size(220, 28), Font = BodyFont, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.AddRange(new string[] { "Breathing", "Guided", "Mindfulness", "Body Scan", "Loving-Kindness" });
            cmbType.SelectedIndex = 0;

            var lblQuality = new Label { Text = "Quality (1-5 stars):", Location = new Point(20, 165), Size = new Size(150, 24), Font = BodyFont, ForeColor = GrayText };
            var numQuality = new NumericUpDown { Location = new Point(180, 163), Size = new Size(100, 28), Minimum = 1, Maximum = 5, Value = 3, Font = BodyFont };

            var btnSave = CreateModernButton("💾 Log Session", null);
            btnSave.Location = new Point(20, 230);
            btnSave.Size = new Size(180, 48);
            btnSave.Click += async (s, e) =>
            {
                btnSave.Enabled = false;
                btnSave.Text = "Logging...";

                try
                {
                    var session = await _apiService.CreateMeditationSessionAsync(
                        _currentUser.Id,
                        (int)numDuration.Value,
                        cmbType.SelectedItem?.ToString() ?? "Breathing",
                        (int)numQuality.Value
                    );

                    if (session != null)
                    {
                        MessageBox.Show("Meditation session logged!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadDataAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to log session. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnSave.Enabled = true;
                    btnSave.Text = "💾 Log Session";
                }
            };

            cardInput.Controls.AddRange(new Control[] { lblTitle, lblDuration, numDuration, lblType, cmbType, lblQuality, numQuality, btnSave });

            var cardGrid = CreateCard(600, 20, 560, 540);

            var lblGridTitle = new Label
            {
                Text = "🕉️ Meditation Sessions",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            dgvMeditation = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(520, 400),
                ReadOnly = true
            };

            dgvMeditation.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMeditation.MultiSelect = false;
            dgvMeditation.AllowUserToAddRows = false;
            StyleDataGridView(dgvMeditation);

            var btnEditMeditation = CreateModernButton("✏️ Edit", btnEditMeditation_Click);
            btnEditMeditation.Location = new Point(20, 475);
            btnEditMeditation.Size = new Size(130, 42);

            var btnDeleteMeditation = CreateModernButton("🗑️ Delete", btnDeleteMeditation_Click, false);
            btnDeleteMeditation.Location = new Point(165, 475);
            btnDeleteMeditation.Size = new Size(130, 42);

            cardGrid.Controls.AddRange(new Control[] { lblGridTitle, dgvMeditation, btnEditMeditation, btnDeleteMeditation });
            tab.Controls.AddRange(new Control[] { cardInput, cardGrid });
        }

        private void InitializeAnalyticsTab(TabPage tab)
        {
            var card = CreateCard(20, 20, 1120, 600);

            var lblTitle = new Label
            {
                Text = "📈 Weekly Wellness Report",
                Location = new Point(20, 20),
                Size = new Size(1080, 36),
                Font = TitleFont,
                ForeColor = DarkText
            };

            var btnWeeklySummary = CreateModernButton("📊 Generate Report", null);
            btnWeeklySummary.Location = new Point(20, 70);
            btnWeeklySummary.Size = new Size(200, 48);

            var txtAnalytics = new TextBox
            {
                Location = new Point(20, 135),
                Size = new Size(1080, 440),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };

            btnWeeklySummary.Click += async (s, e) =>
            {
                btnWeeklySummary.Enabled = false;
                btnWeeklySummary.Text = "Loading...";

                try
                {
                    var summary = await _analyticsService.GetWeeklySummaryAsync(_currentUser.Id);
                    var stats = await _analyticsService.GetOverallStatsAsync(_currentUser.Id);

                    var report = "═══════════════════════════════════════════════════════════════\n";
                    report += "                    WEEKLY WELLNESS REPORT                      \n";
                    report += "═══════════════════════════════════════════════════════════════\n\n";
                    report += string.Join("\n", summary);
                    report += $"\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
                    report += $"                    OVERALL STATISTICS\n";
                    report += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
                    report += $"  Average Mood:         {stats.avgMood:F1}/10\n";
                    report += $"  Average Sleep:        {stats.avgSleep:F1} hours\n";
                    report += $"  Total Entries:        {stats.totalEntries}\n\n";

                    var moodHistory = await _analyticsService.GetMoodHistoryAsync(_currentUser.Id);
                    if (moodHistory.Any())
                    {
                        report += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n";
                        report += "                   RECENT MOOD HISTORY\n";
                        report += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
                        report += string.Join("\n", moodHistory.Take(5).Select(x => $"  {x.date:MM/dd/yyyy}  →  {x.moodDescription}"));
                    }

                    report += "\n\n═══════════════════════════════════════════════════════════════\n";

                    txtAnalytics.Text = report;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading analytics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnWeeklySummary.Enabled = true;
                    btnWeeklySummary.Text = "📊 Generate Report";
                }
            };

            card.Controls.AddRange(new Control[] { lblTitle, btnWeeklySummary, txtAnalytics });
            tab.Controls.Add(card);
        }

        private void InitializeAnxietyTab(TabPage tab)
        {
            // Grounding panel
            var cardGrounding = CreateCard(20, 20, 560, 360);

            var lblGroundingTitle = new Label
            {
                Text = "🌿 Grounding Exercise (5-4-3-2-1)",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            txtGrounding = new TextBox
            {
                Location = new Point(20, 65),
                Size = new Size(520, 200),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = BodyFont,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 252)
            };

            btnStartGrounding = CreateModernButton("▶ Start", null);
            btnStartGrounding.Location = new Point(20, 285);
            btnStartGrounding.Size = new Size(140, 48);

            btnNextGrounding = CreateModernButton("Next Step →", null, false);
            btnNextGrounding.Location = new Point(175, 285);
            btnNextGrounding.Size = new Size(140, 48);
            btnNextGrounding.Enabled = false;

            btnStartGrounding.Click += (s, e) =>
            {
                _currentGroundingIndex = 0;
                txtGrounding.Text = _panicHelper.GetGroundingStep(_currentGroundingIndex);
                btnNextGrounding.Enabled = true;
            };

            btnNextGrounding.Click += (s, e) =>
            {
                _currentGroundingIndex++;
                if (_currentGroundingIndex < _panicHelper.GetGroundingExercises().Length)
                {
                    txtGrounding.Text = _panicHelper.GetGroundingStep(_currentGroundingIndex);
                }
                else
                {
                    txtGrounding.Text = "✅ Grounding complete!\n\nTake a few deep breaths and notice how you feel.\n\nYou did great! 🌟";
                    btnNextGrounding.Enabled = false;
                }
            };

            cardGrounding.Controls.AddRange(new Control[] { lblGroundingTitle, txtGrounding, btnStartGrounding, btnNextGrounding });

            // Breathing pacer panel
            var cardPacer = CreateCard(600, 20, 560, 360);

            var lblPacerTitle = new Label
            {
                Text = "🫁 Breathing Pacer",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            var lblBreathsLabel = new Label
            {
                Text = "Breaths per minute:",
                Location = new Point(20, 70),
                Size = new Size(160, 24),
                Font = BodyFont,
                ForeColor = GrayText
            };

            numBreathsPerMinute = new NumericUpDown
            {
                Location = new Point(190, 68),
                Size = new Size(80, 28),
                Minimum = 2,
                Maximum = 20,
                Value = 6,
                Font = BodyFont
            };

            btnStartPacer = CreateModernButton("▶ Start", null);
            btnStartPacer.Location = new Point(290, 63);
            btnStartPacer.Size = new Size(120, 38);

            btnStopPacer = CreateModernButton("⏹ Stop", null, false);
            btnStopPacer.Location = new Point(425, 63);
            btnStopPacer.Size = new Size(100, 38);
            btnStopPacer.Enabled = false;

            lblPacerDisplay = new Label
            {
                Text = "Press Start to Begin",
                Location = new Point(20, 125),
                Size = new Size(520, 180),
                BorderStyle = BorderStyle.None,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                BackColor = Color.FromArgb(240, 247, 255)
            };
            lblPacerDisplay.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = GetRoundedRect(new Rectangle(0, 0, lblPacerDisplay.Width - 1, lblPacerDisplay.Height - 1), 12))
                using (var brush = new SolidBrush(lblPacerDisplay.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            };

            btnStartPacer.Click += (s, e) =>
            {
                int bpm = (int)numBreathsPerMinute.Value;
                _pacerIntervalMs = _panicHelper.GetMillisPerBreath(bpm) / 2;
                _isInhale = true;
                lblPacerDisplay.Text = "Breathe In...";
                lblPacerDisplay.BackColor = Color.FromArgb(232, 245, 233);
                _breathingTimer.Interval = Math.Max(200, _pacerIntervalMs);
                _breathingTimer.Start();
                btnStartPacer.Enabled = false;
                btnStopPacer.Enabled = true;
            };

            btnStopPacer.Click += (s, e) =>
            {
                _breathingTimer.Stop();
                lblPacerDisplay.Text = "Paused";
                lblPacerDisplay.BackColor = Color.FromArgb(240, 247, 255);
                btnStartPacer.Enabled = true;
                btnStopPacer.Enabled = false;
            };

            cardPacer.Controls.AddRange(new Control[] { lblPacerTitle, lblBreathsLabel, numBreathsPerMinute, btnStartPacer, btnStopPacer, lblPacerDisplay });

            tab.Controls.AddRange(new Control[] { cardGrounding, cardPacer });
        }

        private void InitializeGoalsTab(TabPage tab)
        {
            var cardSettings = CreateCard(20, 20, 560, 280);

            var lblTitle = new Label
            {
                Text = "🎯 Set Your Goals",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            var lblJournalGoal = new Label { Text = "Journal entries per week:", Location = new Point(20, 75), Size = new Size(200, 24), Font = BodyFont, ForeColor = GrayText };
            numJournalGoal = new NumericUpDown { Location = new Point(230, 73), Size = new Size(100, 28), Minimum = 0, Maximum = 100, Value = 3, Font = BodyFont };

            var lblMeditationGoal = new Label { Text = "Meditation minutes per week:", Location = new Point(20, 120), Size = new Size(230, 24), Font = BodyFont, ForeColor = GrayText };
            numMeditationGoal = new NumericUpDown { Location = new Point(260, 118), Size = new Size(100, 28), Minimum = 0, Maximum = 10000, Value = 60, Font = BodyFont };

            btnSaveGoals = CreateModernButton("💾 Save Goals", null);
            btnSaveGoals.Location = new Point(20, 180);
            btnSaveGoals.Size = new Size(160, 48);
            btnSaveGoals.Click += async (s, e) =>
            {
                var gs = new GoalsState((int)numJournalGoal.Value, (int)numMeditationGoal.Value);
                SaveGoals(gs);
                await UpdateGoalsUIAsync(gs);
                MessageBox.Show("Goals saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnResetGoals = CreateModernButton("↺ Reset", null, false);
            btnResetGoals.Location = new Point(195, 180);
            btnResetGoals.Size = new Size(135, 48);
            btnResetGoals.Click += (s, e) =>
            {
                var path = GetGoalsFilePath();
                try
                {
                    if (File.Exists(path)) File.Delete(path);
                }
                catch { }

                var defaults = new GoalsState(3, 60);
                _ = UpdateGoalsUIAsync(defaults);
                MessageBox.Show("Goals reset to defaults.", "Reset", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            cardSettings.Controls.AddRange(new Control[] { lblTitle, lblJournalGoal, numJournalGoal, lblMeditationGoal, numMeditationGoal, btnSaveGoals, btnResetGoals });

            // Progress visualization panel
            var cardProgress = CreateCard(600, 20, 560, 280);

            var lblProgressTitle = new Label
            {
                Text = "📊 Your Progress (Last 7 Days)",
                Location = new Point(20, 20),
                Size = new Size(520, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            lblJournalProgress = new Label { Text = "Journal progress:", Location = new Point(20, 70), Size = new Size(520, 24), Font = BodyFont, ForeColor = GrayText };
            pbJournal = new ProgressBar { Location = new Point(20, 100), Size = new Size(520, 24) };

            lblMeditationProgress = new Label { Text = "Meditation progress:", Location = new Point(20, 145), Size = new Size(520, 24), Font = BodyFont, ForeColor = GrayText };
            pbMeditation = new ProgressBar { Location = new Point(20, 175), Size = new Size(520, 24) };

            cardProgress.Controls.AddRange(new Control[] { lblProgressTitle, lblJournalProgress, pbJournal, lblMeditationProgress, pbMeditation });

            // DataGridView with styling
            var cardGrid = CreateCard(20, 320, 1140, 280);

            var lblGridTitle = new Label
            {
                Text = "📋 Goals Summary",
                Location = new Point(20, 20),
                Size = new Size(1100, 32),
                Font = SubtitleFont,
                ForeColor = DarkText
            };

            dgvGoals = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(1100, 200),
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            StyleDataGridView(dgvGoals);

            cardGrid.Controls.AddRange(new Control[] { lblGridTitle, dgvGoals });

            tab.Controls.AddRange(new Control[] { cardSettings, cardProgress, cardGrid });

            var loaded = LoadGoals();
            _ = UpdateGoalsUIAsync(loaded);
            PopulateGoalsGrid(loaded);
        }

        private string GetGoalsFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"goals_{_currentUser.Id}.json");
        }

        private record GoalsState(int JournalEntriesPerWeekGoal, int MeditationMinutesPerWeekGoal);

        private GoalsState LoadGoals()
        {
            var path = GetGoalsFilePath();
            if (!File.Exists(path))
                return new GoalsState(3, 60);

            try
            {
                var json = File.ReadAllText(path);
                var gs = JsonSerializer.Deserialize<GoalsState>(json);
                return gs ?? new GoalsState(3, 60);
            }
            catch
            {
                return new GoalsState(3, 60);
            }
        }

        private void SaveGoals(GoalsState gs)
        {
            var path = GetGoalsFilePath();
            try
            {
                var json = JsonSerializer.Serialize(gs, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(path, json);
            }
            catch { }

            PopulateGoalsGrid(gs);
        }

        private void PopulateGoalsGrid(GoalsState gs)
        {
            dgvGoals.DataSource = new[] {
                new { Goal = "📖 Journal entries per week", Target = gs.JournalEntriesPerWeekGoal.ToString() },
                new { Goal = "🧘 Meditation minutes per week", Target = gs.MeditationMinutesPerWeekGoal.ToString() }
            }.ToList();

            if (dgvGoals.Columns.Contains("Goal")) dgvGoals.Columns["Goal"].ReadOnly = true;
            if (dgvGoals.Columns.Contains("Target")) dgvGoals.Columns["Target"].ReadOnly = true;
        }

        private void UpdateGoalsUI(GoalsState gs)
        {
            if (numJournalGoal != null) numJournalGoal.Value = Math.Min(numJournalGoal.Maximum, Math.Max(numJournalGoal.Minimum, gs.JournalEntriesPerWeekGoal));
            if (numMeditationGoal != null) numMeditationGoal.Value = Math.Min(numMeditationGoal.Maximum, Math.Max(numMeditationGoal.Minimum, gs.MeditationMinutesPerWeekGoal));
            PopulateGoalsGrid(gs);
        }

        private async Task UpdateGoalsUIAsync(GoalsState gs)
        {
            try
            {
                if (numJournalGoal != null) numJournalGoal.Value = Math.Min(numJournalGoal.Maximum, Math.Max(numJournalGoal.Minimum, gs.JournalEntriesPerWeekGoal));
                if (numMeditationGoal != null) numMeditationGoal.Value = Math.Min(numMeditationGoal.Maximum, Math.Max(numMeditationGoal.Minimum, gs.MeditationMinutesPerWeekGoal));

                var journalEntries = await _apiService.GetJournalEntriesAsync(_currentUser.Id);
                int journalCount = journalEntries?.Where(e => e.EntryDate >= DateTime.Now.AddDays(-7)).Count() ?? 0;

                var medSessions = await _apiService.GetMeditationSessionsAsync(_currentUser.Id);
                int meditationMinutes = medSessions?
                    .Where(s => s.SessionDate >= DateTime.Now.AddDays(-7))
                    .Sum(s => s.DurationMinutes) ?? 0;

                int journalGoal = Math.Max(1, gs.JournalEntriesPerWeekGoal);
                pbJournal.Maximum = journalGoal;
                pbJournal.Value = Math.Min(journalGoal, journalCount);
                lblJournalProgress.Text = $"Journal progress: {journalCount}/{gs.JournalEntriesPerWeekGoal} entries {(journalCount >= journalGoal ? "✅" : "")}";

                int meditationGoal = Math.Max(1, gs.MeditationMinutesPerWeekGoal);
                pbMeditation.Maximum = meditationGoal;
                pbMeditation.Value = Math.Min(meditationGoal, meditationMinutes);
                lblMeditationProgress.Text = $"Meditation progress: {meditationMinutes}/{gs.MeditationMinutesPerWeekGoal} minutes {(meditationMinutes >= meditationGoal ? "✅" : "")}";

                PopulateGoalsGrid(gs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating goals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BreathingTimer_Tick(object? sender, EventArgs e)
        {
            _isInhale = !_isInhale;
            lblPacerDisplay.Text = _isInhale ? "Breathe In..." : "Breathe Out...";
            lblPacerDisplay.BackColor = _isInhale ? Color.FromArgb(232, 245, 233) : Color.FromArgb(255, 243, 224);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var journalEntries = await _apiService.GetJournalEntriesAsync(_currentUser.Id);
                if (journalEntries != null)
                {
                    dgvJournal.DataSource = journalEntries.Select(j => new
                    {
                        j.Id,
                        Date = j.EntryDate.ToString("MM/dd/yyyy"),
                        j.Content,
                        Mood = $"{j.MoodScore}/10",
                        Stress = $"{j.StressLevel}/10",
                        Sleep = $"{j.HoursSlept}h"
                    }).ToList();

                    if (dgvJournal.Columns.Contains("Id")) dgvJournal.Columns["Id"].Visible = false;
                }

                var meditationSessions = await _apiService.GetMeditationSessionsAsync(_currentUser.Id);
                if (meditationSessions != null)
                {
                    dgvMeditation.DataSource = meditationSessions.Select(m => new
                    {
                        m.Id,
                        Date = m.SessionDate.ToString("MM/dd/yyyy"),
                        Type = m.MeditationType,
                        Duration = $"{m.DurationMinutes} min",
                        Quality = $"{m.QualityRating}/5 ⭐"
                    }).ToList();

                    if (dgvMeditation.Columns.Contains("Id")) dgvMeditation.Columns["Id"].Visible = false;
                }

                try
                {
                    var gs = LoadGoals();
                    await UpdateGoalsUIAsync(gs);
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task UpdateRecommendationAsync()
        {
            try
            {
                var journalEntries = await _apiService.GetJournalEntriesAsync(_currentUser.Id);
                var latestEntry = journalEntries?.FirstOrDefault();

                if (latestEntry != null)
                {
                    try
                    {
                        var prologRecs = await _recommendationService.GetRecommendationsFromPrologAsync(latestEntry).ConfigureAwait(true);
                        if (prologRecs != null && prologRecs.Count > 0)
                        {
                            if (prologRecs.Any(r => r.StartsWith("Prolog error:")))
                            {
                                var errorMsg = string.Join(", ", prologRecs);
                                System.Diagnostics.Debug.WriteLine($"Prolog failed: {errorMsg}");
                            }
                            else
                            {
                                lblRecommendation.Text = string.Join(", ", prologRecs);
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Prolog exception: {ex.Message}");
                    }

                    var recommendation = _recommendationService.GetRecommendation(latestEntry);
                    lblRecommendation.Text = recommendation;
                }
                else
                {
                    lblRecommendation.Text = "Start your wellness journey by logging your first journal entry! ✨";
                }
            }
            catch (Exception ex)
            {
                lblRecommendation.Text = $"Unable to load recommendations: {ex.Message}";
            }
        }

        private async Task UpdateStatsAsync()
        {
            try
            {
                var stats = await _analyticsService.GetOverallStatsAsync(_currentUser.Id);
                lblStats.Text = $"📊 Average Mood:  {stats.avgMood:F1}/10\n" +
                              $"😴 Average Sleep: {stats.avgSleep:F1} hours\n" +
                              $"📝 Total Entries:  {stats.totalEntries}";
            }
            catch (Exception ex)
            {
                lblStats.Text = $"Unable to load stats: {ex.Message}";
            }
        }

        private async Task RefreshJournalListAsync()
        {
            try
            {
                var entries = await _apiService.GetJournalEntriesAsync(_currentUser.Id);
                if (entries != null)
                {
                    dgvJournal.DataSource = entries.Select(j => new
                    {
                        j.Id,
                        Date = j.EntryDate.ToString("MM/dd/yyyy"),
                        j.Content,
                        Mood = $"{j.MoodScore}/10",
                        Stress = $"{j.StressLevel}/10",
                        Sleep = $"{j.HoursSlept}h"
                    }).ToList();

                    if (dgvJournal.Columns.Contains("Id")) dgvJournal.Columns["Id"].Visible = false;
                }

                try
                {
                    await UpdateGoalsUIAsync(LoadGoals());
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing journal list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RefreshMeditationListAsync()
        {
            try
            {
                var sessions = await _apiService.GetMeditationSessionsAsync(_currentUser.Id);
                if (sessions != null)
                {
                    dgvMeditation.DataSource = sessions.Select(m => new
                    {
                        m.Id,
                        Date = m.SessionDate.ToString("MM/dd/yyyy"),
                        Type = m.MeditationType,
                        Duration = $"{m.DurationMinutes} min",
                        Quality = $"{m.QualityRating}/5 ⭐"
                    }).ToList();

                    if (dgvMeditation.Columns.Contains("Id")) dgvMeditation.Columns["Id"].Visible = false;
                }

                try
                {
                    await UpdateGoalsUIAsync(LoadGoals());
                }
                catch { }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing meditation list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetSelectedIdFromGrid(DataGridView grid)
        {
            DataGridViewRow? row = null;
            if (grid.SelectedRows.Count > 0) row = grid.SelectedRows[0];
            else if (grid.CurrentRow != null) row = grid.CurrentRow;

            if (row == null) return -1;

            if (row.Cells["Id"] != null && row.Cells["Id"].Value is int id1) return id1;

            foreach (DataGridViewCell cell in row.Cells)
            {
                if (cell.Value is int id2) return id2;
                if (cell.Value is long id3) return (int)id3;
            }

            return -1;
        }

        private async void btnEditJournal_Click(object sender, EventArgs e)
        {
            var id = GetSelectedIdFromGrid(dgvJournal);
            if (id <= 0) { MessageBox.Show("Select an entry first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                var entry = await _apiService.GetJournalEntryByIdAsync(id);
                if (entry == null) { MessageBox.Show("Entry not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                using var dlg = new EditJournalEntryForm(entry);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    dlg.UpdatedEntry.UserId = entry.UserId;
                    var (success, error) = await _apiService.UpdateJournalEntryAsync(dlg.UpdatedEntry);
                    if (!success) MessageBox.Show($"Failed to update: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await RefreshJournalListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDeleteJournal_Click(object sender, EventArgs e)
        {
            var id = GetSelectedIdFromGrid(dgvJournal);
            if (id <= 0) { MessageBox.Show("Select an entry first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show("Delete this journal entry?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                var (success, error) = await _apiService.DeleteJournalEntryAsync(id, _currentUser.Id);
                if (!success) MessageBox.Show($"Failed to delete: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await RefreshJournalListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnEditMeditation_Click(object sender, EventArgs e)
        {
            var id = GetSelectedIdFromGrid(dgvMeditation);
            if (id <= 0) { MessageBox.Show("Select a session first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                var session = await _apiService.GetMeditationSessionByIdAsync(id);
                if (session == null) { MessageBox.Show("Session not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

                using var dlg = new EditMeditationSessionForm(session);
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    dlg.UpdatedSession.UserId = session.UserId;
                    var (success, error) = await _apiService.UpdateMeditationSessionAsync(dlg.UpdatedSession);
                    if (!success) MessageBox.Show($"Failed to update: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    await RefreshMeditationListAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDeleteMeditation_Click(object sender, EventArgs e)
        {
            var id = GetSelectedIdFromGrid(dgvMeditation);
            if (id <= 0) { MessageBox.Show("Select a session first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show("Delete this session?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                var (success, error) = await _apiService.DeleteMeditationSessionAsync(id, _currentUser.Id);
                if (!success) MessageBox.Show($"Failed to delete: {error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await RefreshMeditationListAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}