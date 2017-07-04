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
        Timer _noSleepTimer { get; set; }
        GlobalKeyboardHook _globalKeyboardHook { get; set; }
        GlobalMouseHook _globalMouseHook { get; set; }
        int InactivityTimeout = 90;
        int SavedTimeCounter = 0;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            _inactivityTimer = new Timer();
            _inactivityTimer.Interval = TimeSpan.FromSeconds(InactivityTimeout).TotalMilliseconds;
            _inactivityTimer.AutoReset = false;
            _inactivityTimer.Elapsed += InactivityTimer_Elapsed;

            _noSleepTimer = new Timer();
            _noSleepTimer.Interval = TimeSpan.FromSeconds(InactivityTimeout).TotalMilliseconds;
            _noSleepTimer.AutoReset = true;
            _noSleepTimer.Elapsed += NoSleepTimer_Elapsed;
            
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += GlobalKeyboardHook_KeyboardPressed;

            _globalMouseHook = new GlobalMouseHook();
            _globalMouseHook.MouseAction += GlobalMouseHook_MouseAction;

            _inactivityTimer.Start();
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
            _inactivityTimer.Stop();
            PreventSleep();
            _noSleepTimer.Start();
        }

        private void NoSleepTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PreventSleep();
        }

        private void ResetTimers()
        {
            _noSleepTimer.Stop();
            _inactivityTimer.Stop();
            _inactivityTimer.Start();
        }

        private void PreventSleep()
        {
            SavedTimeCounter += InactivityTimeout;
            KeyboardSimulator.SimulateKeypress();
        }

        private void StopApplication()
        {
            _inactivityTimer.Stop();
            _inactivityTimer.Dispose();

            _noSleepTimer.Stop();
            _noSleepTimer.Dispose();

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

        private void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            var msg = "I've just saved you from {0} minutes o work!";
            MessageBox.Show(string.Format(msg, (int)TimeSpan.FromSeconds(SavedTimeCounter).TotalMinutes), "WOW!", MessageBoxButton.OK);
        }

        private void ExitApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
