using DarkUI.Renderers;
using System.Windows.Forms;

using System.Runtime.Versioning;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    public class DarkMenuStrip : MenuStrip
    {
        #region Constructor Region

        public DarkMenuStrip()
        {
            Renderer = new DarkMenuRenderer();
            Padding = new Padding(3, 2, 0, 2);
        }

        #endregion
    }
}

