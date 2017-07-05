using NoSleep.Core.Hooks;
using NoSleep.Core.Hooks.Mouse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.Core.EventArgs
{
    public class GlobalMouseHookEventArgs : HandledEventArgs
    {
        public MouseMessages MouseEvent { get; set; }

        public GlobalMouseHookEventArgs(MouseMessages mouseEvent)
        {
            MouseEvent = mouseEvent;
        }
    }
}
