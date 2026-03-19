using System;
using System.ComponentModel;
using System.Windows;

namespace HAWidget
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateStatus("主界面已打开");
        }

        public void UpdateStatus(string text)
        {
            StatusText.Text = text;
        }

        //public void BringToFront()
        //{
        //    Topmost = true;
        //    Topmost = false;
        //    Focus();
        //}

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current is App app)
            {
                app.LoadAndOpenWidgetWindows();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current is App app)
            {
                app.ShowSettingsWindow();
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (System.Windows.Application.Current is App app)
            {
                app.ExitApplication();
            }
        }
    }
}