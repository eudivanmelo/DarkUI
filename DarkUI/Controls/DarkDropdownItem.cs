using System.Drawing;
using System.Windows.Forms;

using System.Runtime.Versioning;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    public class DarkDropdownItem
    {
        #region Property Region

        public string Text { get; set; }

        public Bitmap Icon { get; set; }

        #endregion

        #region Constructor Region

        public DarkDropdownItem()
        { }

        public DarkDropdownItem(string text)
        {
            Text = text;
        }

        public DarkDropdownItem(string text, Bitmap icon)
            : this(text)
        {
            Icon = icon;
        }

        #endregion
    }
}

