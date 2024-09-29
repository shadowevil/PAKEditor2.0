using System;
using System.Drawing;
using System.Windows.Forms;

namespace PAKEditor
{
    public class InputDialog : Form
    {
        private TextBox inputTextBox;
        private Button okButton;
        private Button cancelButton;
        private Label contentLabel;
        private bool isNumericOnly;

        public string InputText { get; private set; }

        public InputDialog(string caption, string contentText, bool numericOnly = false)
        {
            this.Text = caption;
            this.isNumericOnly = numericOnly;
            InitializeComponent(contentText);
        }

        private void InitializeComponent(string contentText)
        {
            // Caption/content label
            this.contentLabel = new Label()
            {
                Text = contentText,
                Left = 20,
                Top = 10,
                Width = 280,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                MaximumSize = new Size(280, 0), // Allow wrapping
            };

            this.inputTextBox = new TextBox() { Left = 20, Top = 50, Width = 280 };

            // If numeric only, restrict input to digits
            if (isNumericOnly)
            {
                this.inputTextBox.KeyPress += InputTextBox_KeyPress;
            }

            this.okButton = new Button() { Text = "OK", Left = 50, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            this.cancelButton = new Button() { Text = "Cancel", Left = 160, Width = 100, Top = 90, DialogResult = DialogResult.Cancel };

            this.okButton.Click += OkButton_Click;
            this.cancelButton.Click += CancelButton_Click;

            this.Controls.Add(contentLabel);
            this.Controls.Add(inputTextBox);
            this.Controls.Add(okButton);
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(320, 150);
        }

        private void InputTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow only digits, control keys, and decimal separator if numeric only
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            InputText = inputTextBox.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public static string? ShowInputDialog(string caption, string contentText, bool numericOnly = false)
        {
            using (var form = new InputDialog(caption, contentText, numericOnly))
            {
                return form.ShowDialog() == DialogResult.OK ? form.InputText : null;
            }
        }
    }
}
