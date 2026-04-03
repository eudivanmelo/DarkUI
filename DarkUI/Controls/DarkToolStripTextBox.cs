using DarkUI.Config;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Windows.Forms.Design;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class DarkToolStripTextBox : ToolStripControlHost
    {
        public DarkTextBox TextBox
        {
            get { return (DarkTextBox)Control; }
        }

        public override string Text
        {
            get { return TextBox.Text; }
            set { TextBox.Text = value; }
        }


        public DarkToolStripTextBox() : base(new DarkTextBox())
        {
            AutoSize = false;
            Size = new Size(140, 22);
            Margin = new Padding(1, 3, 1, 3);
            TextBox.Padding = new Padding(2, 4, 2, 2);
            TextBox.BorderStyle = BorderStyle.FixedSingle;
            TextBox.BackColor = Colors.LightBackground;
            TextBox.ForeColor = Colors.LightText;
        }
    }
}
