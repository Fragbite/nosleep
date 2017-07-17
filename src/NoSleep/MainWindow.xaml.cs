using Microsoft.Win32;
using NoSleep.Core.Hooks;
using NoSleep.Core.Hooks.LastUserInput;
using NoSleep.Core.Simulators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        string _logHistory { get; set; }
        TextBox _logWindow { get; set; }
        Timer _loopTimer { get; set; }
        int _inactivityTimeout { get; set; }
        int _loopInterval { get; set; }
        int _savedTimeCounter { get; set; }
        bool _isSuspended { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            SetupAndStartApplication();
            SubscribeToSystemEvents();
        }
        
        private void SetupAndStartApplication()
        {
            Log("Setting up & starting application");

            Init();
            Configure();
            SubscribeToApplicationEvents();
            StartTimers();
        }

        private void Init()
        {
            _loopTimer = new Timer();
        }

        private void Configure()
        {
            _loopInterval = 30;
            _inactivityTimeout = 85;
            _isSuspended = false;

            _loopTimer.Interval = TimeSpan.FromSeconds(_loopInterval).TotalMilliseconds;
            _loopTimer.AutoReset = true;
        }

        private void SubscribeToApplicationEvents()
        {
            _loopTimer.Elapsed += InactivityTimer_Elapsed;
        }

        private void StartTimers()
        {
            _loopTimer.Start();
        }

        private void SubscribeToSystemEvents()
        {
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
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
            Log("System is going into suspended mode");

            if (!_isSuspended)
            {
                HaltExecution();
            }
        }

        private void HaltExecution()
        {
            Log("Halting application execution");

            StopApplication();
        }

        private void ResumeExecution_BySystem()
        {
            Log("System is resuming execution from suspended mode");

            if (!_isSuspended)
            {
                ResumeExecution();
            }
        }

        private void ResumeExecution()
        {
            Log("Resuming application execution");

            SetupAndStartApplication();
        }

        private void InactivityTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var idleTickTimeout = GetLastUserInput.GetIdleTickCount();
            var inactivityTimeout = TimeSpan.FromMilliseconds(idleTickTimeout).TotalSeconds;   
            if (inactivityTimeout > _inactivityTimeout)
            {
                PreventSleep();
            }
        }
        
        private void ResetTimers()
        {
            _loopTimer.Stop();
            _loopTimer.Start();
        }

        private void PreventSleep()
        {
            Log("No activity, simulating keypress");

            _savedTimeCounter += _inactivityTimeout;
            KeyboardSimulator.SimulateKeypress();
        }

        private void StopApplication()
        {
            Log("Stopping timers and releasing hooks");

            _loopTimer.Stop();
            _loopTimer.Elapsed -= InactivityTimer_Elapsed;
            _loopTimer.Dispose();

            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopApplication();
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (_isSuspended)
            {
                ResumeExecution();
                UpdateToggleMenuItem("Suspend");
            }
            else
            {
                HaltExecution();
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
        
        private void ShowLog_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window()
            {
                WindowStyle = WindowStyle.ToolWindow,
                WindowState = System.Windows.WindowState.Normal,
                Title = "Log view",
                Width = 500,
                Height = 500,
                ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip,
                Background = System.Windows.Media.Brushes.WhiteSmoke,
                AllowsTransparency = false,
                ShowInTaskbar = false,
                ShowActivated = true,
                Topmost = true
            };

            _logWindow = new TextBox()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Text = _logHistory
            };

            window.Content = _logWindow;
            window.Closing += window_Closing;

            window.Show();
        }

        void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logWindow = null;
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Log(string msg)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine(msg);
            }

            _logHistory += msg + Environment.NewLine;
            if (_logWindow != null)
            {
                _logWindow.AppendText(msg + Environment.NewLine);
                _logWindow.ScrollToEnd();
            }
        }

        private void Log(string format, params string[] values)
        {
            Log(string.Format(format, values));
        }
    }
}
