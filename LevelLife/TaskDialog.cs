using System;
using System.Drawing;
using System.Windows.Forms;

namespace LevelLife
{
    public partial class TaskDialog : Form
    {
        private TextBox txtTitle;
        private TextBox txtDescription;
        private NumericUpDown numXp;
        private Button btnOK, btnCancel;
        private string placeholderText = "день, чтобы стать лучше";

        public string Title => txtTitle.Text;
        public string Description => txtDescription.Text;
        public int XpReward => (int)numXp.Value;

        public TaskDialog(Task task = null)
        {
            InitializeComponent();
            if (task != null)
            {
                txtTitle.Text = task.Title;
                txtDescription.Text = task.Description;
                numXp.Value = task.XpReward;
            }
            else
            {
                // Placeholder для описания
                txtDescription.Text = placeholderText;
                txtDescription.ForeColor = Color.Gray;
                txtDescription.Enter += (s, e) =>
                {
                    if (txtDescription.Text == placeholderText)
                    {
                        txtDescription.Text = "";
                        txtDescription.ForeColor = Color.FromArgb(235, 235, 240);
                    }
                };
                txtDescription.Leave += (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(txtDescription.Text))
                    {
                        txtDescription.Text = placeholderText;
                        txtDescription.ForeColor = Color.Gray;
                    }
                };
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Задача";
            this.Size = new Size(450, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(38, 40, 50);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            Label lblTitle = new Label
            {
                Text = "Название:",
                Location = new Point(20, 20),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(235, 235, 240)
            };
            txtTitle = new TextBox
            {
                Location = new Point(130, 20),
                Size = new Size(280, 23),
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(235, 235, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblDesc = new Label
            {
                Text = "Описание:",
                Location = new Point(20, 60),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(235, 235, 240)
            };
            txtDescription = new TextBox
            {
                Location = new Point(130, 60),
                Size = new Size(280, 100),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(235, 235, 240),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblXp = new Label
            {
                Text = "Награда XP:",
                Location = new Point(20, 180),
                Size = new Size(100, 25),
                ForeColor = Color.FromArgb(235, 235, 240)
            };
            numXp = new NumericUpDown
            {
                Location = new Point(130, 180),
                Size = new Size(100, 23),
                Minimum = 1,
                Maximum = 10000,
                Value = 10,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(235, 235, 240)
            };

            btnOK = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 230),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            btnOK.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 110);
            btnOK.FlatAppearance.BorderSize = 1;

            btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(310, 230),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 75),
                ForeColor = Color.FromArgb(220, 220, 230),
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 110);
            btnCancel.FlatAppearance.BorderSize = 1;

            this.Controls.AddRange(new Control[] { lblTitle, txtTitle, lblDesc, txtDescription, lblXp, numXp, btnOK, btnCancel });
        }
    }
}