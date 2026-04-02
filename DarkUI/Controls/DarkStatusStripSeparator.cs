using DarkUI.Config;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.StatusStrip)]
    public class DarkStatusStripSeparator : ToolStripItem
    {
        #region Constructor Region

        public DarkStatusStripSeparator()
            : base(string.Empty, null, null)
        {
            AutoSize = false;
            Size = new Size(4, 8);
            Margin = new Padding(0, 0, 0, 0);
            Enabled = true;
        }

        #endregion

        #region Paint Region

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var centerX = Width / 2;
            var top = 1;
            var bottom = Height - 2;

            using var dark = new Pen(Colors.DarkBorder);
            using var light = new Pen(Colors.LightBorder);

            e.Graphics.DrawLine(dark, centerX, top, centerX, bottom);
            e.Graphics.DrawLine(light, centerX + 1, top, centerX + 1, bottom);
        }

        #endregion
    }
}
