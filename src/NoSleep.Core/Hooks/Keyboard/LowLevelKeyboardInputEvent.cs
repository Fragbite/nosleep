using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.Core.Hooks.Keyboard
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelKeyboardInputEvent
    {
        public int VirtualCode;

        public int HardwareScanCode;

        public int Flags;

        public int TimeStamp;

        public IntPtr AdditionalInformation;
    }
}
