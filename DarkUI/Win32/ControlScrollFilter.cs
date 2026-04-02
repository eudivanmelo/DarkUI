using System.Drawing;
using System.Windows.Forms;
using System.Runtime.Versioning;

namespace DarkUI.Win32
{
    [SupportedOSPlatform("windows6.1")]
    public class ControlScrollFilter : IMessageFilter
    {
        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WM.MOUSEWHEEL:
                case (int)WM.MOUSEHWHEEL:
                    var hControlUnderMouse = Native.WindowFromPoint(new Point((int)m.LParam));

                    if (hControlUnderMouse == m.HWnd)
                        return false;

                    Native.SendMessage(hControlUnderMouse, (uint)m.Msg, m.WParam, m.LParam);
                    return true;
            }

            return false;
        }
    }
}
