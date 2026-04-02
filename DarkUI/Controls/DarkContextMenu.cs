using DarkUI.Renderers;
using System.Windows.Forms;

using System.Runtime.Versioning;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    public class DarkContextMenu : ContextMenuStrip
    {
        #region Constructor Region

        public DarkContextMenu()
        {
            Renderer = new DarkMenuRenderer();
        }

        #endregion
    }
}

