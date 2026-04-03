using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Windows.Forms.Design;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class DarkToolStripNumericUpDown : ToolStripControlHost
    {
        public DarkNumericUpDown NumericUpDown
        {
            get { return (DarkNumericUpDown)Control; }
        }

        public decimal Value
        {
            get { return NumericUpDown.Value; }
            set { NumericUpDown.Value = value; }
        }

        public decimal Minimum
        {
            get { return NumericUpDown.Minimum; }
            set { NumericUpDown.Minimum = value; }
        }

        public decimal Maximum
        {
            get { return NumericUpDown.Maximum; }
            set { NumericUpDown.Maximum = value; }
        }

        public int DecimalPlaces
        {
            get { return NumericUpDown.DecimalPlaces; }
            set { NumericUpDown.DecimalPlaces = value; }
        }

        public DarkToolStripNumericUpDown() : base(new DarkNumericUpDown())
        {
            AutoSize = false;
            Size = new Size(90, 22);
            Margin = new Padding(1, 3, 1, 3);
        }
    }
}
