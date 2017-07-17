using NoSleep.Core.Hooks.LastUserInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.Core.Hooks.External
{
    internal static class LastUserInputExternal
    {
        [DllImport("User32.dll")]
        public static extern bool GetLastInputInfo(ref GetLastUserInput.LASTINPUTINFO plii);
    }
}
