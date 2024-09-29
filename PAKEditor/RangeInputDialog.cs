using System;
using System.Drawing;
using System.Windows.Forms;

namespace PAKEditor
{
    public class RangeInputDialog : Form
    {
        private TextBox minRangeTextBox;
        private TextBox maxRangeTextBox;
        private Button okButton;
        private Button cancelButton;
        private Label contentLabel;
        private Label minRangeLabel;
        private Label maxRangeLabel;

        public Range? range { get; private set; }

        public RangeInputDialog(string caption, string contentText)
        {
            this.Text = caption;
            InitializeComponent(contentText);
        }

        private void InitializeComponent(string contentText)
        {
            // Caption label with word-wrap
            this.contentLabel = new Label()
            {
                Text = contentText,
                Left = 20,
                Top = 10,
                Width = 280, // Adjust width accordingly to fit within form size
                Height = 40, // Adjust height for potential wrapping
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                MaximumSize = new Size(280, 0), // Allows dynamic height for wrapping
            };

            this.minRangeLabel = new Label() { Text = "Min Range:", Left = 20, Top = 60, AutoSize = true };
            this.minRangeTextBox = new TextBox() { Left = 120, Top = 60, Width = 150 };

            this.maxRangeLabel = new Label() { Text = "Max Range:", Left = 20, Top = 90, AutoSize = true };
            this.maxRangeTextBox = new TextBox() { Left = 120, Top = 90, Width = 150 };

            this.okButton = new Button() { Text = "OK", Left = 50, Width = 100, Top = 130, DialogResult = DialogResult.OK };
            this.cancelButton = new Button() { Text = "Cancel", Left = 160, Width = 100, Top = 130, DialogResult = DialogResult.Cancel };

            this.okButton.Click += new EventHandler(OkButton_Click);
            this.cancelButton.Click += new EventHandler(CancelButton_Click);

            this.Controls.Add(contentLabel);
            this.Controls.Add(minRangeLabel);
            this.Controls.Add(minRangeTextBox);
            this.Controls.Add(maxRangeLabel);
            this.Controls.Add(maxRangeTextBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new System.Drawing.Size(320, 180);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            if (int.TryParse(minRangeTextBox.Text, out int minRange) &&
                int.TryParse(maxRangeTextBox.Text, out int maxRange))
            {
                if (minRange <= maxRange)
                {
                    range = new System.Range(minRange, maxRange);
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Min range must be less than or equal to Max range.", "Invalid Range", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please enter valid numbers.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public static Range? ShowInputDialog(string caption, string contentText)
        {
            using (var form = new RangeInputDialog(caption, contentText))
            {
                return form.ShowDialog() == DialogResult.OK ? form.range : null;
            }
        }
    }
}
