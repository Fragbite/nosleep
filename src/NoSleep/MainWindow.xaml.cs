using Microsoft.Win32;
using NoSleep.Core.EventArgs;
using NoSleep.Core.Hooks;
using NoSleep.Core.Simulators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoSleep
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer _inactivityTimer { get; set; }
        GlobalKeyboardHook _globalKeyboardHook { get; set; }
        GlobalMouseHook _globalMouseHook { get; set; }
        int _inactivityTimeout { get; set; }
        int _savedTimeCounter { get; set; }
        bool _isSuspended { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Init();
            Configure();
            SubscribeToEvents();
            StartTimers();
        }

        private void Init()
        {
            _inactivityTimer = new Timer();
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalMouseHook = new GlobalMouseHook();
        }

        private void Configure()
        {
            _inactivityTimeout = 90;
            _savedTimeCounter = 0;
            _isSuspended = false;

            _inactivityTimer.Interval = TimeSpan.FromSeconds(_inactivityTimeout).TotalMilliseconds;
            _inactivityTimer.AutoReset = true;
        }

        private void SubscribeToEvents()
        {
            _inactivityTimer.Elapsed += InactivityTimer_Elapsed;
            _globalKeyboardHook.KeyboardPressed += GlobalKeyboardHook_KeyboardPressed;
            _globalMouseHook.MouseAction += GlobalMouseHook_MouseAction;

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        private void StartTimers()
        {
            _inactivityTimer.Start();
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    HaltExecution_BySystem();
                    break;
                case PowerModes.Resume:
                    ResumeExecution_BySystem();
                    break;
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    HaltExecution_BySystem();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    ResumeExecution_BySystem();
                    break;
                case SessionSwitchReason.SessionLogoff:
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void HaltExecution_BySystem()
        {
            if (!_isSuspended)
            {
                _inactivityTimer.Stop();
            }
        }

        private void ResumeExecution_BySystem()
        {
            if (!_isSuspended)
            {
                _inactivityTimer.Start();
            }
        }

        private void GlobalMouseHook_MouseAction(object sender, GlobalMouseHookEventArgs e)
        {
            ResetTimers();
        }

        private void GlobalKeyboardHook_KeyboardPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
            ResetTimers();
        }

        private void InactivityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PreventSleep();
        }
        
        private void ResetTimers()
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        private void PreventSleep()
        {
            _savedTimeCounter += _inactivityTimeout;
            KeyboardSimulator.SimulateKeypress();
        }

        private void StopApplication()
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Dispose();

            if (_globalKeyboardHook != null)
            {
                _globalKeyboardHook.Dispose();
            }
            if (_globalMouseHook != null)
            {
                _globalMouseHook.Dispose();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopApplication();
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (_isSuspended)
            {
                _inactivityTimer.Start();
                UpdateToggleMenuItem("Suspend");
            }
            else
            {
                _inactivityTimer.Stop();
                UpdateToggleMenuItem("Resume");
            }
            _isSuspended = !_isSuspended;
        }

        private void UpdateToggleMenuItem(string header)
        {
            var firstItem = myNotifyIcon.ContextMenu.Items[0];
            var menuItem = firstItem as MenuItem;
            menuItem.Header = header;
        }

        private void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            Window window = new Window()
            {
                WindowStyle = WindowStyle.None,
                WindowState = System.Windows.WindowState.Maximized,
                Background = System.Windows.Media.Brushes.Transparent,
                AllowsTransparency = true,
                ShowInTaskbar = false,
                ShowActivated = true,
                Topmost = true
            };

            window.Show();

            var msg = "I've just saved you from {0} minutes of work!";
            MessageBox.Show(window, string.Format(msg, (int)TimeSpan.FromSeconds(_savedTimeCounter).TotalMinutes), "WOW!", MessageBoxButton.OK);
            
            window.Close();
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
