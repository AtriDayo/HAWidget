using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;

namespace HAWidget
{
    public partial class WidgetWindow : Window
    {
        private readonly AppConfig _config;
        private readonly WindowGroupConfig _group;
        private readonly HomeAssistantService _haService;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly List<EntityViewItem> _entityItems = new List<EntityViewItem>();
        private bool _isRefreshing = false;
        private bool _isEditMode = false;

        public WidgetWindow(AppConfig config, WindowGroupConfig group)
        {
            InitializeComponent();

            _config = config;
            _group = group;

            _haService = new HomeAssistantService(_config.HaBaseUrl, _config.Token);

            Left = _group.Left;
            Top = _group.Top;
            Width = _group.Width;
            Height = _group.Height;

            SetEditMode(false);

            Topmost = _group.WindowLevel == "TopMost";

            GroupTitleText.Text = _group.Title;

            foreach (EntityConfig entity in _group.Entities)
            {
                _entityItems.Add(new EntityViewItem
                {
                    Title = entity.Title,
                    Value = "--",
                    Status = "等待刷新",
                    Config = entity
                });
            }

            EntityItemsControl.ItemsSource = _entityItems;

            MouseLeftButtonDown += WidgetWindow_MouseLeftButtonDown;

            LocationChanged += WidgetWindow_LocationChanged;
            SizeChanged += WidgetWindow_SizeChanged;
            Closing += WidgetWindow_Closing;

            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            _ = RefreshAllEntities();
        }

        private async void Timer_Tick(object? sender, EventArgs e)
        {
            await RefreshAllEntities();
        }

        private async Task RefreshAllEntities()
        {
            if (_isRefreshing)
            {
                return;
            }

            _isRefreshing = true;

            try
            {
                if (string.IsNullOrWhiteSpace(_config.HaBaseUrl) || string.IsNullOrWhiteSpace(_config.Token))
                {
                    foreach (EntityViewItem item in _entityItems)
                    {
                        item.Value = "--";
                        item.Status = "请先在设置中填写 HA 地址和 Token";
                    }

                    RefreshItems();
                    return;
                }

                foreach (EntityViewItem item in _entityItems)
                {
                    item.Status = "读取中...";
                }

                RefreshItems();

                foreach (EntityViewItem item in _entityItems)
                {
                    try
                    {
                        string state = await GetEntityState(item.Config.EntityId);

                        item.RawState = state;
                        item.IsOn = state == "on";

                        item.Value = FormatValue(state, item.Config);
                        item.Status = "更新于 " + DateTime.Now.ToString("HH:mm:ss");
                    }
                    catch (Exception ex)
                    {
                        item.Value = "--";
                        item.Status = GetFriendlyErrorMessage(ex);
                    }
                }

                RefreshItems();
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void RefreshItems()
        {
            EntityItemsControl.ItemsSource = null;
            EntityItemsControl.ItemsSource = _entityItems;
        }

        private async Task<string> GetEntityState(string entityId)
        {
            return await _haService.GetEntityStateAsync(entityId);
        }

        private string FormatValue(string state, EntityConfig entity)
        {
            if (entity.EntityType == "Light")
            {
                return state == "on" ? "已开启" : "已关闭";
            }

            if (entity.EntityType == "Fan")
            {
                return state == "on" ? "运行中" : "已关闭";
            }

            if (entity.EntityType == "Climate")
            {
                return state;
            }

            if (string.IsNullOrWhiteSpace(entity.CustomUnit))
            {
                return state;
            }

            return state + " " + entity.CustomUnit;
        }

        private void WidgetWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isEditMode && e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private string GetFriendlyErrorMessage(Exception ex)
        {
            string message = ex.Message;

            if (string.IsNullOrWhiteSpace(_config.HaBaseUrl) || string.IsNullOrWhiteSpace(_config.Token))
            {
                return "未配置 HA 地址或 Token";
            }

            if (message.Contains("401"))
            {
                return "Token 无效或未授权";
            }

            if (message.Contains("404"))
            {
                return "实体不存在";
            }

            if (message.Contains("No such host") || message.Contains("Name or service not known"))
            {
                return "服务器地址无效";
            }

            if (message.Contains("actively refused") || message.Contains("拒绝"))
            {
                return "连接被拒绝";
            }

            if (message.Contains("timed out") || message.Contains("超时"))
            {
                return "连接超时";
            }

            return "读取失败";
        }

        private void WidgetWindow_LocationChanged(object? sender, EventArgs e)
        {
            SaveWindowBoundsToGroup();
        }

        private void WidgetWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveWindowBoundsToGroup();
        }

        private void WidgetWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowBoundsToGroup();
        }

        private void SetEditMode(bool enabled)
        {
            _isEditMode = enabled;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;

            if (_isEditMode)
            {
                Topmost = false;
                GroupTitleText.Text = _group.Title + "（编辑中）";
                ResizeThumb.Visibility = Visibility.Visible;
                RootBorder.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
            }
            else
            {
                Topmost = _group.WindowLevel == "TopMost";
                GroupTitleText.Text = _group.Title;
                ResizeThumb.Visibility = Visibility.Collapsed;
                RootBorder.BorderBrush = System.Windows.Media.Brushes.Transparent;
            }
        }

        private void EnterEditModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
        }

        private void ExitEditModeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(false);
            SaveWindowBoundsToGroup();
            ConfigService.SaveConfig(_config);
        }

        private void CloseWindowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            double newHeight = Height + e.VerticalChange;

            if (newWidth >= 220)
            {
                Width = newWidth;
            }

            if (newHeight >= 140)
            {
                Height = newHeight;
            }

            SaveWindowBoundsToGroup();
        }

        private void SaveWindowBoundsToGroup()
        {
            _group.Left = Left;
            _group.Top = Top;
            _group.Width = Width;
            _group.Height = Height;
        }

        private async void LightOnButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not EntityViewItem item)
            {
                return;
            }

            try
            {
                item.Status = "正在打开...";
                RefreshItems();

                await _haService.TurnLightOnAsync(item.Config.EntityId);

                string state = await GetEntityState(item.Config.EntityId);
                item.RawState = state;
                item.IsOn = state == "on";
                item.Value = FormatValue(state, item.Config);
                item.Status = "更新于 " + DateTime.Now.ToString("HH:mm:ss");

                RefreshItems();
            }
            catch (Exception ex)
            {
                item.Status = GetFriendlyErrorMessage(ex);
                RefreshItems();
            }
        }

        private async void LightOffButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not EntityViewItem item)
            {
                return;
            }

            try
            {
                item.Status = "正在关闭...";
                RefreshItems();

                await _haService.TurnLightOffAsync(item.Config.EntityId);

                string state = await GetEntityState(item.Config.EntityId);
                item.RawState = state;
                item.IsOn = state == "on";
                item.Value = FormatValue(state, item.Config);
                item.Status = "更新于 " + DateTime.Now.ToString("HH:mm:ss");

                RefreshItems();
            }
            catch (Exception ex)
            {
                item.Status = GetFriendlyErrorMessage(ex);
                RefreshItems();
            }
        }
    }
}