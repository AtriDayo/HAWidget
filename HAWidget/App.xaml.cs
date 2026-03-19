using System;
using System.Collections.Generic;
using System.Windows;
using Forms = System.Windows.Forms;

namespace HAWidget
{
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;
        private SettingsWindow? _settingsWindow;

        private AppConfig _config = new AppConfig();
        private readonly List<WidgetWindow> _widgetWindows = new List<WidgetWindow>();

        private bool _isExiting;
        public bool IsExiting => _isExiting;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            CreateTrayIcon();
            LoadAndOpenWidgetWindows();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            base.OnExit(e);
        }

        private void CreateTrayIcon()
        {
            _notifyIcon = new Forms.NotifyIcon();
            _notifyIcon.Text = "HAWidget";
            _notifyIcon.Icon = new System.Drawing.Icon("Assets\\logo.ico");
            _notifyIcon.Visible = true;

            _notifyIcon.MouseClick += NotifyIcon_MouseClick;

            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            menu.Items.Add("打开主界面", null, (_, __) => ShowMainWindow());
            menu.Items.Add("打开设置", null, (_, __) => ShowSettingsWindow());
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add("退出", null, (_, __) => ExitApplication());

            _notifyIcon.ContextMenuStrip = menu;
        }

        private void NotifyIcon_MouseClick(object? sender, Forms.MouseEventArgs e)
        {
            if (e.Button == Forms.MouseButtons.Left)
            {
                ShowMainWindow();
            }
        }

        public void LoadAndOpenWidgetWindows()
        {
            CloseAllWidgetWindows();

            _config = ConfigService.LoadConfig();

            foreach (WindowGroupConfig group in _config.WindowGroups)
            {
                WidgetWindow widgetWindow = new WidgetWindow(_config, group);
                _widgetWindows.Add(widgetWindow);
                widgetWindow.Show();
            }

            if (_mainWindow != null)
            {
                _mainWindow.UpdateStatus($"已加载 {_config.WindowGroups.Count} 个窗口组");
            }
        }

        private void CloseAllWidgetWindows()
        {
            foreach (WidgetWindow window in _widgetWindows)
            {
                window.Close();
            }

            _widgetWindows.Clear();
        }

        public void ShowMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow();
                _mainWindow.Closed += MainWindow_Closed;
            }

            if (!_mainWindow.IsVisible)
            {
                _mainWindow.Show();
            }

            if (_mainWindow.WindowState == WindowState.Minimized)
            {
                _mainWindow.WindowState = WindowState.Normal;
            }

            _mainWindow.Activate();
            //_mainWindow.BringToFront();
            _mainWindow.Topmost = true;
            _mainWindow.Topmost = false; // 把窗口移到最上层来
            _mainWindow.Focus(); // 将窗口设为焦点
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _mainWindow = null;
        }

        public void ShowSettingsWindow()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(_config);
                _settingsWindow.Closed += SettingsWindow_Closed;
            }

            if (!_settingsWindow.IsVisible)
            {
                _settingsWindow.Show();
            }

            if (_settingsWindow.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }

            _settingsWindow.Activate();
        }

        private void SettingsWindow_Closed(object? sender, EventArgs e)
        {
            _settingsWindow = null;
        }

        public void SaveSettingsAndReload()
        {
            LoadAndOpenWidgetWindows();
        }

        public void ExitApplication()
        {
            if(_isExiting)
                return;

            _isExiting = true;

            try
            {
                CloseAllWidgetWindows();

                if (_settingsWindow != null)
                {
                    _settingsWindow.Close();
                    _settingsWindow = null;
                }

                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
                _mainWindow = null;
            }
            finally
            {
                Shutdown();
            }
        }
    }
}