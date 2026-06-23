using System;
using System.Drawing;
using System.Windows.Forms;

namespace LevelLife
{
    public partial class GreetingDialog : Form
    {
        private TextBox txtName;
        private TextBox txtGoal;
        private Button btnOK, btnCancel;
        private Player existingPlayer;

        public string PlayerName => txtName.Text;
        public string PlayerGoal => txtGoal.Text;

        public GreetingDialog(Player player = null, bool isEditing = false)
        {
            existingPlayer = player;
            InitializeComponent(isEditing);
            if (player != null && isEditing)
            {
                txtName.Text = player.Name;
                txtGoal.Text = player.Goal;
                this.Text = "Сменить имя и цель";
                btnOK.Text = "Сохранить";
            }
        }

        private void InitializeComponent(bool isEditing)
        {
            this.Text = isEditing ? "Сменить имя и цель" : "Добро пожаловать в LevelLife!";
            this.Size = new Size(450, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(38, 40, 50);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            Label lblGreeting = new Label
            {
                Text = isEditing ? "Измени свои данные:" : "Привет! Представься, чтобы начать путь.",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                ForeColor = Color.FromArgb(235, 190, 120),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblName = new Label
            {
                Text = "Твоё имя:",
                Location = new Point(30, 65),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(235, 235, 240)
            };
            txtName = new TextBox
            {
                Location = new Point(140, 65),
                Size = new Size(260, 23),
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(235, 235, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Text = existingPlayer?.Name ?? "Игрок"
            };

            Label lblGoal = new Label
            {
                Text = "Твоя цель:",
                Location = new Point(30, 105),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(235, 235, 240)
            };
            txtGoal = new TextBox
            {
                Location = new Point(140, 105),
                Size = new Size(260, 23),
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(235, 235, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Text = existingPlayer?.Goal ?? "Стать лучше"
            };

            btnOK = new Button
            {
                Text = isEditing ? "Сохранить" : "Начать!",
                DialogResult = DialogResult.OK,
                Location = new Point(140, 160),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(235, 190, 120);
            btnOK.FlatAppearance.BorderSize = 1;

            btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(260, 160),
                Size = new Size(100, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 110);
            btnCancel.FlatAppearance.BorderSize = 1;

            this.Controls.AddRange(new Control[] { lblGreeting, lblName, txtName, lblGoal, txtGoal, btnOK, btnCancel });
        }
    }
}