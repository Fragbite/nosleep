﻿using NoSleep.Core.Hooks.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.Core.Simulators
{
    public static class KeyboardSimulator
    {
        public static void SimulateKeypress()
        {
            KeyboardExternal.keybd_event(KEY, 0, KEYEVENTF_KEYDOWN, 0);
            KeyboardExternal.keybd_event(KEY, 0, KEYEVENTF_KEYUP, 0);
        }

        private const int KEYEVENTF_KEYDOWN = 0x0001; // Key down flag
        private const int KEYEVENTF_KEYUP = 0x0002; // Key up flag
        private const int KEY = 126; // F15 keycode.
    }
}
