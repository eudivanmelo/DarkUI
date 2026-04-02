using System;

using System.Runtime.Versioning;

namespace DarkUI.Controls
{
    [SupportedOSPlatform("windows6.1")]
    public class ScrollValueEventArgs(int value) : EventArgs
    {
        public int Value { get; private set; } = value;
    }
}

