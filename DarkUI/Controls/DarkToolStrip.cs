using DarkUI.Renderers;
using DarkUI.Config;
using System.Drawing;
using System.Windows.Forms;

using System.Runtime.Versioning;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    public class DarkToolStrip : ToolStrip
    {
        #region Constructor Region

        public DarkToolStrip()
        {
            Renderer = new DarkToolStripRenderer();
            Padding = new Padding(5, 0, 1, 0);
            AutoSize = false;
            Size = new Size(1, 28);
        }

        protected override void OnItemAdded(ToolStripItemEventArgs e)
        {
            base.OnItemAdded(e);

            var textBoxItem = e.Item as ToolStripTextBox;
            if (textBoxItem != null)
            {
                textBoxItem.BackColor = Colors.LightBackground;
                textBoxItem.ForeColor = Colors.LightText;
                textBoxItem.BorderStyle = BorderStyle.FixedSingle;
                textBoxItem.AutoSize = false;
                textBoxItem.Size = new Size(textBoxItem.Width > 0 ? textBoxItem.Width : 140, 22);
            }

            var comboItem = e.Item as ToolStripComboBox;
            if (comboItem != null)
            {
                comboItem.BackColor = Colors.LightBackground;
                comboItem.ForeColor = Colors.LightText;
                comboItem.FlatStyle = FlatStyle.Flat;
                comboItem.AutoSize = false;
                comboItem.Size = new Size(comboItem.Width > 0 ? comboItem.Width : 140, 22);
            }

            var hostItem = e.Item as ToolStripControlHost;
            if (hostItem != null && hostItem.AutoSize)
                hostItem.AutoSize = false;
        }

        #endregion
    }
}

