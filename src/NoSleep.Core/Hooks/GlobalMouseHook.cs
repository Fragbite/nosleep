using NoSleep.Core.EventArgs;
using NoSleep.Core.Hooks.External;
using NoSleep.Core.Hooks.Mouse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.Core.Hooks
{
    public class GlobalMouseHook : IDisposable
    {
        public event EventHandler<GlobalMouseHookEventArgs> MouseAction;
        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelMouseProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private const int WH_MOUSE_LL = 14;
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        public GlobalMouseHook()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        ~GlobalMouseHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
                
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                MouseExternal.UnhookWindowsHookEx(_hookID);
            }
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return MouseExternal.SetWindowsHookEx(WH_MOUSE_LL, proc,
                  MouseExternal.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                
                var eventArguments = new GlobalMouseHookEventArgs((MouseMessages)wParam);

                EventHandler<GlobalMouseHookEventArgs> handler = MouseAction;
                if (handler != null)
                {
                    handler.Invoke(this, eventArguments);
                }
            }
            return MouseExternal.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
