using System;
using System.Drawing;
using System.Windows.Forms;

namespace LevelLife
{
    public partial class ConfirmDialog : Form
    {
        private Button btnYes, btnNo;

        public ConfirmDialog(string message)
        {
            InitializeComponent(message);
        }

        private void InitializeComponent(string message)
        {
            this.Text = "Подтверждение";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(38, 40, 50);

            Label lblMessage = new Label
            {
                Text = message,
                Location = new Point(20, 20),
                Size = new Size(340, 40),
                ForeColor = Color.FromArgb(235, 235, 240),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnYes = new Button
            {
                Text = "Да",
                DialogResult = DialogResult.Yes,
                Location = new Point(120, 70),
                Size = new Size(70, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230)
            };
            btnYes.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 110);
            btnYes.FlatAppearance.BorderSize = 1;

            btnNo = new Button
            {
                Text = "Нет",
                DialogResult = DialogResult.No,
                Location = new Point(200, 70),
                Size = new Size(70, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230)
            };
            btnNo.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 110);
            btnNo.FlatAppearance.BorderSize = 1;

            this.Controls.AddRange(new Control[] { lblMessage, btnYes, btnNo });
        }
    }
}