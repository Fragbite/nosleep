using NoSleep.Hooks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.EventArgs
{
    internal class GlobalMouseHookEventArgs : HandledEventArgs
    {
        public GlobalMouseHook.MouseMessages MouseEvent { get; set; }

        public GlobalMouseHookEventArgs(GlobalMouseHook.MouseMessages mouseEvent)
        {
            MouseEvent = mouseEvent;
        }
    }
}
