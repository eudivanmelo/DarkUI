using System;

namespace DarkUI.Docking
{
    public class DockContentEventArgs(DarkDockContent content) : EventArgs
    {
        public DarkDockContent Content { get; private set; } = content;
    }
}
