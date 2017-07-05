using NoSleep.Core.EventArgs;
using NoSleep.Core.Hooks.External;
using NoSleep.Core.Hooks.Keyboard;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NoSleep.Core.Hooks
{
    public class GlobalKeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private HookProc _hookProc;

        private const int WH_KEYBOARD_LL = 13;

        public GlobalKeyboardHook()
        {
            _windowsHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;
            _hookProc = LowLevelKeyboardProc;

            _user32LibraryHandle = KeyboardExternal.LoadLibrary("User32");
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, string.Format("Failed to load library 'User32.dll'. Error {0}: {1}.", errorCode, new Win32Exception(Marshal.GetLastWin32Error()).Message));
            }

            _windowsHookHandle = KeyboardExternal.SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, _user32LibraryHandle, 0);
            if (_windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, string.Format("Failed to adjust keyboard hooks for '{0}'. Error {1}: {2}.", Process.GetCurrentProcess().ProcessName, errorCode, new Win32Exception(Marshal.GetLastWin32Error()).Message));
            }
        }

        ~GlobalKeyboardHook()
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
                if (_windowsHookHandle != IntPtr.Zero)
                {
                    if (!KeyboardExternal.UnhookWindowsHookEx(_windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, string.Format("Failed to remove keyboard hooks for '{0}'. Error {1}: {2}.", Process.GetCurrentProcess().ProcessName, errorCode, new Win32Exception(Marshal.GetLastWin32Error()).Message));
                    }
                    _windowsHookHandle = IntPtr.Zero;

                    _hookProc -= LowLevelKeyboardProc;
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!KeyboardExternal.FreeLibrary(_user32LibraryHandle))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, string.Format("Failed to unload library 'User32.dll'. Error {0}: {1}.", errorCode, new Win32Exception(Marshal.GetLastWin32Error()).Message));
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }
             
        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;

            var wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                var eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);

                EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                if (handler != null)
                {
                    handler.Invoke(this, eventArguments);
                }

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr)1 : KeyboardExternal.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
