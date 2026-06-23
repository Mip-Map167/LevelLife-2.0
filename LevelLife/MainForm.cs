using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace LevelLife
{
    public partial class MainForm : Form
    {
        private Player player;
        private List<Task> tasks;
        private List<Task> deletedTasks;
        private Timer statusTimer;
        private Timer xpAnimationTimer;
        private Timer confettiTimer;
        private int xpAnimationValue;
        private int xpAnimationTarget;
        private Label lblXpAnimation;
        private Random rand = new Random();
        private List<ConfettiParticle> confettiParticles = new List<ConfettiParticle>();

        private FlowLayoutPanel flowTasks;
        private Panel panelAvatar;
        private Label lblName, lblLevel, lblXP, lblToNext, lblGreeting;
        private ProgressBar progressBar;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Button btnAdd, btnEdit, btnDelete, btnComplete, btnReset;
        private MenuStrip menuStrip;
        private Chart chartProgress;

        private readonly Color bgColor = Color.FromArgb(28, 30, 38);
        private readonly Color panelColor = Color.FromArgb(38, 40, 50);
        private readonly Color accentColor = Color.FromArgb(235, 190, 120);
        private readonly Color textColor = Color.FromArgb(235, 235, 240);
        private readonly Color textMuted = Color.FromArgb(160, 160, 175);
        private readonly Color progressStart = Color.FromArgb(200, 160, 100);
        private readonly Color progressEnd = Color.FromArgb(235, 200, 130);
        private readonly Color completedColor = Color.FromArgb(60, 120, 70);
        private readonly Color cardBgColor = Color.FromArgb(45, 47, 60);

        // Звуки (используем встроенные системные звуки)
        private readonly SoundPlayer soundComplete = new SoundPlayer(Properties.Resources.CompleteSound ?? new byte[0]);
        private readonly SoundPlayer soundLevelUp = new SoundPlayer(Properties.Resources.LevelUpSound ?? new byte[0]);
        private readonly SoundPlayer soundAdd = new SoundPlayer(Properties.Resources.AddSound ?? new byte[0]);
        private readonly SoundPlayer soundDelete = new SoundPlayer(Properties.Resources.DeleteSound ?? new byte[0]);

        public MainForm()
        {
            InitializeComponent();
            InitializeKeyboardShortcuts();

            player = new Player("Игрок");
            player.Goal = "Стать лучше";
            tasks = new List<Task>();
            deletedTasks = new List<Task>();

            ShowGreetingDialog();
            LoadData();
            UpdatePlayerUI();
            RenderTaskCards();
            UpdateChart();

            this.FormClosing += MainForm_FormClosing;
            InitializeStatusTimer();
            InitializeXpAnimation();
            InitializeConfettiTimer();

            ShowStatusMessage("Добро пожаловать в LevelLife!", false);
        }

        private void InitializeComponent()
        {
            this.Text = "LevelLife — твой прогресс";
            this.Size = new Size(1100, 820);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            // ---- Меню ----
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(28, 30, 38),
                ForeColor = textColor,
                Font = new Font("Segoe UI", 9.5F)
            };
            menuStrip.Renderer = new CustomMenuRenderer();

            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Справка");
            ToolStripMenuItem guideItem = new ToolStripMenuItem("Краткое руководство", null, (s, e) =>
            {
                MessageBox.Show(
                    "1. Добавьте задачу — укажите название и награду в опыте.\n" +
                    "2. Выполняйте задачи — получайте опыт и повышайте уровень.\n" +
                    "3. Удаляйте задачи — они сохраняются в истории.\n" +
                    "4. Следите за прогрессом на панели персонажа.\n\n" +
                    "Горячие клавиши: Ctrl+N — добавить, Delete — удалить, Enter — выполнить.",
                    "Краткое руководство",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            });
            helpMenu.DropDownItems.Add(guideItem);

            ToolStripMenuItem shortcutsItem = new ToolStripMenuItem("Горячие клавиши", null, (s, e) =>
            {
                MessageBox.Show(
                    "Ctrl + N — Добавить задачу\n" +
                    "Delete — Удалить выбранную задачу\n" +
                    "Enter — Выполнить выбранную задачу\n" +
                    "Ctrl + S — Сохранить данные вручную",
                    "Горячие клавиши",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            });
            helpMenu.DropDownItems.Add(shortcutsItem);

            ToolStripMenuItem aboutItem = new ToolStripMenuItem("О программе", null, (s, e) =>
            {
                MessageBox.Show(
                    "LevelLife — система мотивации с игровыми элементами.\n" +
                    "Версия 3.0\n" +
                    "Разработано в рамках производственной практики.",
                    "О программе",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            });
            helpMenu.DropDownItems.Add(aboutItem);

            ToolStripMenuItem historyMenu = new ToolStripMenuItem("История");
            ToolStripMenuItem historyItem = new ToolStripMenuItem("Показать удалённые задачи", null, (s, e) =>
            {
                ShowHistoryDialog();
            });
            historyMenu.DropDownItems.Add(historyItem);

            ToolStripMenuItem statsMenu = new ToolStripMenuItem("Статистика");
            ToolStripMenuItem statsItem = new ToolStripMenuItem("Показать статистику", null, (s, e) =>
            {
                ShowStatistics();
            });
            statsMenu.DropDownItems.Add(statsItem);

            ToolStripMenuItem settingsMenu = new ToolStripMenuItem("Настройки");
            ToolStripMenuItem changeNameItem = new ToolStripMenuItem("Сменить имя и цель", null, (s, e) =>
            {
                ShowGreetingDialog(true);
                UpdatePlayerUI();
                AutoSave();
            });
            settingsMenu.DropDownItems.Add(changeNameItem);

            menuStrip.Items.Add(helpMenu);
            menuStrip.Items.Add(historyMenu);
            menuStrip.Items.Add(statsMenu);
            menuStrip.Items.Add(settingsMenu);

            // ---- Панель персонажа ----
            Panel playerPanel = new Panel
            {
                Location = new Point(760, 35),
                Size = new Size(260, 400),
                BackColor = panelColor,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10)
            };
            playerPanel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, playerPanel.Width, playerPanel.Height, 25, 25));

            panelAvatar = new Panel
            {
                Location = new Point(80, 15),
                Size = new Size(100, 100),
                BackColor = Color.Transparent
            };
            panelAvatar.Paint += DrawLevelMedal;

            lblGreeting = new Label
            {
                Location = new Point(20, 125),
                Size = new Size(220, 25),
                Text = "Привет, Игрок!",
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblName = new Label
            {
                Location = new Point(20, 150),
                Size = new Size(220, 25),
                Text = "Цель: Стать лучше",
                ForeColor = textMuted,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblLevel = new Label
            {
                Location = new Point(20, 185),
                Size = new Size(220, 35),
                Text = "Уровень 1",
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblXP = new Label
            {
                Location = new Point(20, 225),
                Size = new Size(220, 25),
                Text = "Опыт: 0",
                ForeColor = textMuted,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblXpAnimation = new Label
            {
                Location = new Point(20, 225),
                Size = new Size(220, 25),
                Text = "",
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 260),
                Size = new Size(220, 22),
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous,
                ForeColor = accentColor,
                BackColor = Color.FromArgb(50, 50, 65)
            };
            progressBar.Paint += DrawGradientProgressBar;

            lblToNext = new Label
            {
                Location = new Point(20, 290),
                Size = new Size(220, 25),
                Text = "До след. уровня: 100 опыта",
                ForeColor = textMuted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };

            playerPanel.Controls.AddRange(new Control[] {
                panelAvatar, lblGreeting, lblName, lblLevel, lblXP, lblXpAnimation, progressBar, lblToNext
            });

            // ---- Контейнер для карточек задач ----
            flowTasks = new FlowLayoutPanel
            {
                Location = new Point(12, 35),
                Size = new Size(730, 430),
                BackColor = Color.Transparent,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0)
            };

            // ---- График прогресса ----
            chartProgress = new Chart
            {
                Location = new Point(12, 480),
                Size = new Size(730, 200),
                BackColor = Color.FromArgb(35, 37, 46),
                BorderlineColor = Color.FromArgb(60, 60, 80),
                BorderlineDashStyle = ChartDashStyle.Solid,
                BorderlineWidth = 1
            };

            ChartArea chartArea = new ChartArea
            {
                BackColor = Color.FromArgb(35, 37, 46),
                AxisX = {
                    Title = "День",
                    TitleForeColor = textMuted,
                    LabelStyle = { ForeColor = textMuted },
                    LineColor = Color.FromArgb(60, 60, 80)
                },
                AxisY = {
                    Title = "Задач",
                    TitleForeColor = textMuted,
                    LabelStyle = { ForeColor = textMuted },
                    LineColor = Color.FromArgb(60, 60, 80),
                    Minimum = 0
                }
            };
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(40, 40, 50);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(40, 40, 50);
            chartProgress.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                Name = "Выполнено задач",
                ChartType = SeriesChartType.Column,
                Color = accentColor,
                BorderWidth = 2,
                IsValueShownAsLabel = true,
                LabelForeColor = textMuted,
                LabelFont = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            chartProgress.Series.Add(series);

            chartProgress.Titles.Add(new Title
            {
                Text = "Прогресс за неделю",
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            });

            // ---- Кнопки ----
            btnAdd = CreateStyledButton("Добавить", 12, 695, 120, 42);
            btnEdit = CreateStyledButton("Редактировать", 138, 695, 120, 42);
            btnDelete = CreateStyledButton("Удалить", 264, 695, 120, 42);
            btnComplete = CreateStyledButton("Выполнить", 390, 695, 120, 42);

            btnAdd.Click += btnAdd_Click;
            btnEdit.Click += btnEdit_Click;
            btnDelete.Click += btnDelete_Click;
            btnComplete.Click += btnComplete_Click;

            // ---- Кнопка "Начать заново" под персонажем ----
            btnReset = CreateStyledButton("Начать заново", 800, 460, 180, 40);
            btnReset.BackColor = Color.FromArgb(60, 50, 50);
            btnReset.ForeColor = Color.FromArgb(220, 180, 180);
            btnReset.FlatAppearance.BorderColor = Color.FromArgb(120, 80, 80);
            btnReset.Click += btnReset_Click;

            // ---- Статусная строка ----
            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(35, 37, 46),
                ForeColor = textMuted,
                SizingGrip = false,
                Padding = new Padding(12, 0, 12, 0)
            };
            statusLabel = new ToolStripStatusLabel("Готов")
            {
                ForeColor = textMuted,
                Spring = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusStrip.Items.Add(statusLabel);

            this.Controls.AddRange(new Control[] { flowTasks, chartProgress, playerPanel, btnAdd, btnEdit, btnDelete, btnComplete, btnReset, statusStrip, menuStrip });
            this.MainMenuStrip = menuStrip;
        }

        // ---- Кастомный рендерер для меню ----
        private class CustomMenuRenderer : ToolStripProfessionalRenderer
        {
            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(60, 60, 80)), e.Item.ContentRectangle);
                }
                else
                {
                    base.OnRenderMenuItemBackground(e);
                }
            }
        }

        // ---- Стилизованная кнопка ----
        private Button CreateStyledButton(string text, int x, int y, int width, int height)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                UseVisualStyleBackColor = false
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 110);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(75, 80, 105);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 48, 60);
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, width, height, 8, 8));
            return btn;
        }

        // ---- Отрисовка медали уровня ----
        private void DrawLevelMedal(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            int size = 90;
            int x = 5, y = 5;
            Rectangle rect = new Rectangle(x, y, size, size);

            // Свечение
            using (RadialGradientBrush glow = new RadialGradientBrush(rect, Color.FromArgb(80, accentColor), Color.Transparent))
            {
                glow.Center = new PointF(rect.Width / 2, rect.Height / 2);
                glow.FocusScales = new PointF(0.5f, 0.5f);
                g.FillEllipse(glow, rect);
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(rect,
                Color.FromArgb(180, 160, 80),
                Color.FromArgb(220, 190, 130), 45f))
            {
                g.FillEllipse(brush, rect);
            }

            using (Pen pen = new Pen(Color.FromArgb(140, 120, 70), 3))
            {
                g.DrawEllipse(pen, rect);
            }

            string levelText = player?.Level.ToString() ?? "1";
            using (Font font = new Font("Segoe UI", 28, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(levelText, font, new SolidBrush(Color.FromArgb(40, 35, 20)), rect, sf);
            }
        }

        // ---- RadialGradientBrush для свечения ----
        private class RadialGradientBrush : Brush
        {
            private readonly Color _centerColor;
            private readonly Color _surroundColor;
            private readonly Rectangle _rect;

            public PointF Center { get; set; }
            public PointF FocusScales { get; set; }

            public RadialGradientBrush(Rectangle rect, Color centerColor, Color surroundColor)
            {
                _rect = rect;
                _centerColor = centerColor;
                _surroundColor = surroundColor;
                Center = new PointF(rect.Width / 2f, rect.Height / 2f);
                FocusScales = new PointF(1f, 1f);
            }

            public override object Clone() => new RadialGradientBrush(_rect, _centerColor, _surroundColor);
        }

        // ---- Прогресс-бар ----
        private void DrawGradientProgressBar(object sender, PaintEventArgs e)
        {
            ProgressBar pb = sender as ProgressBar;
            if (pb == null) return;

            Rectangle rect = pb.ClientRectangle;
            rect.Inflate(-2, -2);

            GraphicsPath path = GetRoundRectangle(rect, 10);
            e.Graphics.SetClip(path);

            using (LinearGradientBrush brush = new LinearGradientBrush(rect, progressStart, progressEnd, LinearGradientMode.Horizontal))
            {
                float percent = (float)pb.Value / pb.Maximum;
                Rectangle fillRect = new Rectangle(rect.X, rect.Y, (int)(rect.Width * percent), rect.Height);
                if (fillRect.Width > 0)
                {
                    e.Graphics.FillRectangle(brush, fillRect);
                }
            }

            e.Graphics.ResetClip();

            using (Pen pen = new Pen(Color.FromArgb(80, 85, 110), 1))
            {
                e.Graphics.DrawPath(pen, path);
            }

            string text = $"{pb.Value}%";
            using (Font font = new Font("Segoe UI", 8, FontStyle.Bold))
            using (StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(220, 220, 230)), rect, sf);
            }
        }

        private GraphicsPath GetRoundRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        // ---- Карточки задач ----
        private void RenderTaskCards()
        {
            flowTasks.Controls.Clear();
            if (tasks == null) return;
            foreach (var task in tasks)
            {
                var card = CreateTaskCard(task);
                flowTasks.Controls.Add(card);
            }
        }

        private Panel CreateTaskCard(Task task)
        {
            Panel card = new Panel
            {
                Width = 700,
                Height = 80,
                BackColor = task.IsCompleted ? completedColor : cardBgColor,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 0, 8),
                Tag = task
            };
            card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 12, 12));

            Panel statusLine = new Panel
            {
                Width = 6,
                Height = card.Height - 20,
                Location = new Point(10, 10),
                BackColor = task.IsCompleted ? Color.FromArgb(0, 230, 118) : accentColor
            };
            statusLine.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, statusLine.Width, statusLine.Height, 6, 6));

            Label lblTitle = new Label
            {
                Text = task.Title,
                Location = new Point(25, 10),
                Size = new Size(300, 25),
                ForeColor = textColor,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            Label lblDesc = new Label
            {
                Text = task.Description ?? "Нет описания",
                Location = new Point(25, 35),
                Size = new Size(300, 20),
                ForeColor = textMuted,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
            };

            Label lblXp = new Label
            {
                Text = $"Опыт: {task.XpReward}",
                Location = new Point(350, 10),
                Size = new Size(100, 25),
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            Label lblStatus = new Label
            {
                Text = task.IsCompleted ? "Выполнена" : "Не выполнена",
                Location = new Point(470, 10),
                Size = new Size(150, 25),
                ForeColor = task.IsCompleted ? Color.FromArgb(0, 230, 118) : Color.Orange,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular)
            };

            card.Controls.AddRange(new Control[] { statusLine, lblTitle, lblDesc, lblXp, lblStatus });

            card.Click += (s, e) => SelectTask(task);
            foreach (Control ctrl in card.Controls)
            {
                ctrl.Click += (s, e) => SelectTask(task);
            }

            return card;
        }

        private void SelectTask(Task task)
        {
            foreach (Panel card in flowTasks.Controls)
            {
                if (card.Tag == task)
                {
                    card.BackColor = Color.FromArgb(80, 85, 110);
                }
                else
                {
                    var t = card.Tag as Task;
                    card.BackColor = t != null && t.IsCompleted ? completedColor : cardBgColor;
                }
            }
        }

        private Task GetSelectedTask()
        {
            foreach (Panel card in flowTasks.Controls)
            {
                if (card.BackColor == Color.FromArgb(80, 85, 110))
                    return card.Tag as Task;
            }
            return null;
        }

        // ---- График прогресса ----
        private void UpdateChart()
        {
            chartProgress.Series["Выполнено задач"].Points.Clear();

            DateTime today = DateTime.Now.Date;
            for (int i = 6; i >= 0; i--)
            {
                DateTime day = today.AddDays(-i);
                int completedCount = tasks.Count(t => t.IsCompleted && t.CompletionDate.HasValue && t.CompletionDate.Value.Date == day);
                string label = i == 0 ? "Сегодня" : day.ToString("ddd");
                chartProgress.Series["Выполнено задач"].Points.AddXY(label, completedCount);
            }
        }

        // ---- История ----
        private void ShowHistoryDialog()
        {
            if (deletedTasks == null || deletedTasks.Count == 0)
            {
                MessageBox.Show("История пуста. Удалённые задачи будут появляться здесь.", "История", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Form historyForm = new Form
            {
                Text = "История удалённых задач",
                Size = new Size(750, 500),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = bgColor,
                FormBorderStyle = FormBorderStyle.Sizable
            };

            FlowLayoutPanel historyFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };

            foreach (var task in deletedTasks)
            {
                Panel card = new Panel
                {
                    Width = 700,
                    Height = 70,
                    BackColor = Color.FromArgb(45, 47, 60),
                    BorderStyle = BorderStyle.None,
                    Padding = new Padding(10),
                    Margin = new Padding(0, 0, 0, 8)
                };
                card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, card.Width, card.Height, 12, 12));

                Label lblTitle = new Label
                {
                    Text = task.Title,
                    Location = new Point(15, 10),
                    Size = new Size(300, 25),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold)
                };

                Label lblInfo = new Label
                {
                    Text = $"Опыт: {task.XpReward}  |  Удалена: {task.CreationDate:dd.MM.yyyy}",
                    Location = new Point(15, 35),
                    Size = new Size(300, 20),
                    ForeColor = Color.FromArgb(160, 160, 175),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
                };

                Label lblStatus = new Label
                {
                    Text = "Удалена",
                    Location = new Point(500, 15),
                    Size = new Size(150, 25),
                    ForeColor = Color.Salmon,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleRight
                };

                card.Controls.AddRange(new Control[] { lblTitle, lblInfo, lblStatus });
                historyFlow.Controls.Add(card);
            }

            historyForm.Controls.Add(historyFlow);
            historyForm.ShowDialog();
        }

        // ---- Статистика ----
        private void ShowStatistics()
        {
            if (player == null || tasks == null)
            {
                MessageBox.Show("Данные ещё не загружены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int total = tasks.Count;
            int completed = tasks.Count(t => t.IsCompleted);
            int notCompleted = total - completed;
            int totalXP = player.TotalXP;
            int deletedCount = deletedTasks?.Count ?? 0;
            MessageBox.Show(
                $"Всего задач: {total}\n" +
                $"Выполнено: {completed}\n" +
                $"Не выполнено: {notCompleted}\n" +
                $"Удалено: {deletedCount}\n" +
                $"Всего опыта: {totalXP}\n" +
                $"Уровень: {player.Level}",
                "Статистика",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        // ---- Сброс ----
        private void btnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите начать заново?\nВесь прогресс будет сброшен.", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                tasks.Clear();
                deletedTasks.Clear();
                player.Reset();
                tasks.Add(new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50));
                RenderTaskCards();
                UpdatePlayerUI();
                UpdateChart();
                AutoSave();
                ShowStatusMessage("Прогресс сброшен. Начни свой путь заново!", false);
            }
        }

        // ---- Горячие клавиши ----
        private void InitializeKeyboardShortcuts()
        {
            this.KeyPreview = true;
            this.KeyDown += (sender, e) =>
            {
                if (e.Control && e.KeyCode == Keys.N)
                {
                    btnAdd.PerformClick();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Delete && GetSelectedTask() != null)
                {
                    btnDelete.PerformClick();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Enter && GetSelectedTask() != null)
                {
                    btnComplete.PerformClick();
                    e.Handled = true;
                }
                else if (e.Control && e.KeyCode == Keys.S)
                {
                    AutoSave();
                    ShowStatusMessage("Данные сохранены вручную (Ctrl+S)", false);
                    e.Handled = true;
                }
            };
        }

        // ---- Приветствие ----
        private void ShowGreetingDialog(bool isEditing = false)
        {
            try
            {
                using (var dialog = new GreetingDialog(player, isEditing))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        if (!string.IsNullOrWhiteSpace(dialog.PlayerName))
                        {
                            if (player == null) player = new Player(dialog.PlayerName);
                            else player.Name = dialog.PlayerName;
                            player.Goal = dialog.PlayerGoal;

                            lblGreeting.Text = $"Привет, {player.Name}!";
                            lblName.Text = $"Цель: {player.Goal}";

                            if (!isEditing && tasks.Count == 0)
                            {
                                tasks.Add(new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50));
                                RenderTaskCards();
                                UpdateChart();
                                AutoSave();
                            }
                        }
                    }
                    else
                    {
                        if (!isEditing && player != null && tasks.Count == 0)
                        {
                            tasks.Add(new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50));
                            RenderTaskCards();
                            UpdateChart();
                            AutoSave();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии приветствия: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (player == null)
                {
                    player = new Player("Игрок");
                    player.Goal = "Стать лучше";
                }
                if (tasks.Count == 0)
                {
                    tasks.Add(new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50));
                }
            }
        }

        // ---- Загрузка данных ----
        private void LoadData()
        {
            try
            {
                if (player == null) player = new Player("Игрок");

                var loaded = DataService.LoadWithHistory();
                if (loaded.player != null)
                {
                    player.Name = loaded.player.Name;
                    player.Goal = loaded.player.Goal;
                    typeof(Player).GetProperty("Level")?.SetValue(player, loaded.player.Level);
                    typeof(Player).GetField("CurrentXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, loaded.player.CurrentXP);
                    typeof(Player).GetField("TotalXP", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(player, loaded.player.TotalXP);

                    tasks = loaded.tasks ?? new List<Task>();
                    deletedTasks = loaded.deletedTasks ?? new List<Task>();
                }
                else
                {
                    tasks = new List<Task>();
                    deletedTasks = new List<Task>();
                }

                if (tasks.Count == 0)
                {
                    tasks.Add(new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50));
                    DataService.SaveWithHistory(player, tasks, deletedTasks);
                }
                else
                {
                    int maxId = tasks.Max(t => t.Id);
                    Task.SetNextId(maxId);
                }

                ShowStatusMessage("Данные загружены успешно", false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\nБудут созданы новые данные.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (player == null) player = new Player("Игрок");
                tasks = new List<Task>();
                deletedTasks = new List<Task>();
                tasks.Add(new Task("Добро пожаловать в игру!", "У вас всё получится! Выполни эту задачу, чтобы начать свой путь.", 50));
            }
        }

        // ---- Обновление интерфейса ----
        private void UpdatePlayerUI()
        {
            if (player == null) return;
            lblLevel.Text = $"Уровень {player.Level}";
            lblXP.Text = $"Опыт: {player.CurrentXP}";
            lblToNext.Text = $"До след. уровня: {player.GetRemainingXP()} опыта";
            progressBar.Value = player.GetProgressPercent();
            panelAvatar.Invalidate();
            UpdateChart();
        }

        // ---- Таймеры ----
        private void InitializeStatusTimer()
        {
            statusTimer = new Timer { Interval = 3500, Enabled = false };
            statusTimer.Tick += (s, e) =>
            {
                statusLabel.Text = "Готов";
                statusLabel.ForeColor = textMuted;
                statusTimer.Stop();
            };
        }

        private void ShowStatusMessage(string message, bool isError = false)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isError ? Color.Salmon : accentColor;
            statusTimer.Stop();
            statusTimer.Start();
        }

        private void InitializeXpAnimation()
        {
            xpAnimationTimer = new Timer { Interval = 30, Enabled = false };
            xpAnimationTimer.Tick += (s, e) =>
            {
                if (xpAnimationValue < xpAnimationTarget)
                {
                    xpAnimationValue += Math.Max(1, (xpAnimationTarget - xpAnimationValue) / 5);
                    lblXpAnimation.Text = $"+{xpAnimationValue} опыта!";
                    lblXpAnimation.Visible = true;
                }
                else
                {
                    xpAnimationTimer.Stop();
                    lblXpAnimation.Visible = false;
                    lblXP.Text = $"Опыт: {player.CurrentXP}";
                }
            };
        }

        private void ShowXpAnimation(int xpGained)
        {
            xpAnimationValue = 0;
            xpAnimationTarget = xpGained;
            lblXpAnimation.Visible = true;
            xpAnimationTimer.Start();
        }

        // ---- Эффект конфетти ----
        private class ConfettiParticle
        {
            public float X, Y, SpeedX, SpeedY, Size, Rotation;
            public Color Color;
            public float Lifetime;
        }

        private void InitializeConfettiTimer()
        {
            confettiTimer = new Timer { Interval = 30, Enabled = false };
            confettiTimer.Tick += (s, e) =>
            {
                // Обновляем частицы
                for (int i = confettiParticles.Count - 1; i >= 0; i--)
                {
                    var p = confettiParticles[i];
                    p.X += p.SpeedX;
                    p.Y += p.SpeedY;
                    p.SpeedY += 0.2f;
                    p.Rotation += 0.1f;
                    p.Lifetime -= 0.02f;

                    if (p.Lifetime <= 0 || p.Y > this.Height)
                        confettiParticles.RemoveAt(i);
                }

                this.Invalidate();
            };
        }

        private void StartConfettiEffect()
        {
            confettiParticles.Clear();
            for (int i = 0; i < 150; i++)
            {
                confettiParticles.Add(new ConfettiParticle
                {
                    X = rand.Next(0, this.Width),
                    Y = rand.Next(0, 50),
                    SpeedX = (float)(rand.NextDouble() * 8 - 4),
                    SpeedY = (float)(rand.NextDouble() * 4 + 2),
                    Size = rand.Next(5, 12),
                    Color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)),
                    Lifetime = (float)(rand.NextDouble() * 2 + 1),
                    Rotation = 0
                });
            }
            confettiTimer.Start();
        }

        // Рисование конфетти
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (confettiParticles.Count > 0)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                foreach (var p in confettiParticles)
                {
                    g.TranslateTransform(p.X, p.Y);
                    g.RotateTransform(p.Rotation * 57.3f);
                    using (var brush = new SolidBrush(p.Color))
                    {
                        g.FillRectangle(brush, -p.Size / 2, -p.Size / 4, p.Size, p.Size / 2);
                    }
                    g.ResetTransform();
                }
                if (confettiParticles.Count == 0)
                {
                    confettiTimer.Stop();
                }
            }
        }

        // ---- Сохранение ----
        private void AutoSave()
        {
            try
            {
                if (player != null && tasks != null && deletedTasks != null)
                {
                    DataService.SaveWithHistory(player, tasks, deletedTasks);
                    ShowStatusMessage($"Сохранено в {DateTime.Now.ToShortTimeString()}", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"Ошибка сохранения: {ex.Message}", true);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            AutoSave();
        }

        // ---- Обработчики кнопок ----
        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new TaskDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    tasks.Add(new Task(dialog.Title, dialog.Description, dialog.XpReward));
                    RenderTaskCards();
                    UpdatePlayerUI();
                    AutoSave();
                    ShowStatusMessage($"Задача \"{dialog.Title}\" добавлена", false);
                    // Звук добавления
                    try { soundAdd.Play(); } catch { }
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null)
            {
                MessageBox.Show("Выберите задачу.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (task.IsCompleted)
            {
                MessageBox.Show("Нельзя редактировать выполненную задачу.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var dialog = new TaskDialog(task))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    task.Title = dialog.Title;
                    task.Description = dialog.Description;
                    task.XpReward = dialog.XpReward;
                    RenderTaskCards();
                    AutoSave();
                    ShowStatusMessage($"Задача \"{task.Title}\" обновлена", false);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null)
            {
                MessageBox.Show("Выберите задачу.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            using (var confirmDialog = new ConfirmDialog($"Удалить задачу \"{task.Title}\"?"))
            {
                if (confirmDialog.ShowDialog() == DialogResult.Yes)
                {
                    deletedTasks.Add(task);
                    tasks.Remove(task);
                    RenderTaskCards();
                    AutoSave();
                    ShowStatusMessage($"Задача \"{task.Title}\" удалена", false);
                    try { soundDelete.Play(); } catch { }
                }
            }
        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            var task = GetSelectedTask();
            if (task == null)
            {
                MessageBox.Show("Выберите задачу.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (task.IsCompleted)
            {
                MessageBox.Show("Задача уже выполнена.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int oldLevel = player.Level;
            int xpGained = task.Complete();
            task.CompletionDate = DateTime.Now;
            player.AddXP(xpGained);

            ShowXpAnimation(xpGained);
            RenderTaskCards();
            UpdatePlayerUI();
            AutoSave();

            ShowStatusMessage($"Задача \"{task.Title}\" выполнена! +{xpGained} опыта", false);

            // Звук выполнения
            try { soundComplete.Play(); } catch { }

            if (player.Level > oldLevel)
            {
                ShowLevelUpAnimation();
                try { soundLevelUp.Play(); } catch { }
                StartConfettiEffect();
                MessageBox.Show($"Поздравляем! Вы достигли {player.Level} уровня!", "Уровень повышен", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private async void ShowLevelUpAnimation()
        {
            for (int i = 0; i < 5; i++)
            {
                panelAvatar.BackColor = i % 2 == 0 ? Color.FromArgb(255, 215, 0) : Color.Transparent;
                await System.Threading.Tasks.Task.Delay(150);
            }
            panelAvatar.BackColor = Color.Transparent;
            panelAvatar.Invalidate();
        }
    }
}