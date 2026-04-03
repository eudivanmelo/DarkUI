using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Windows.Forms.Design;
using System;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    [ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip)]
    public class DarkToolStripComboBox : ToolStripControlHost
    {
        private const int DefaultHostHeight = 24;

        public DarkComboBox ComboBox
        {
            get { return (DarkComboBox)Control; }
        }

        public ComboBox.ObjectCollection Items
        {
            get { return ComboBox.Items; }
        }

        public object SelectedItem
        {
            get { return ComboBox.SelectedItem; }
            set { ComboBox.SelectedItem = value; }
        }

        public int SelectedIndex
        {
            get { return ComboBox.SelectedIndex; }
            set { ComboBox.SelectedIndex = value; }
        }

        public DarkToolStripComboBox() : base(new DarkComboBox())
        {
            AutoSize = false;
            Size = new Size(140, DefaultHostHeight);
            Margin = new Padding(1, 3, 1, 3);

            ComboBox.AutoSize = false;
            ComboBox.IntegralHeight = false;

            SyncHostedControlSize();
        }

        protected override void OnBoundsChanged()
        {
            base.OnBoundsChanged();
            SyncHostedControlSize();
        }

        protected override void OnOwnerChanged(EventArgs e)
        {
            base.OnOwnerChanged(e);
            SyncHostedControlSize();
        }

        private void SyncHostedControlSize()
        {
            var hostedHeight = Height - Padding.Vertical;
            if (hostedHeight < 1)
                hostedHeight = 1;

            if (ComboBox.Height != hostedHeight)
                ComboBox.Height = hostedHeight;

            var itemHeight = hostedHeight - 6;
            if (itemHeight < 1)
                itemHeight = 1;

            if (ComboBox.ItemHeight != itemHeight)
                ComboBox.ItemHeight = itemHeight;
        }
    }
}
