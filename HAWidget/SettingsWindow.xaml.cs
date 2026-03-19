using System.Collections.Generic;
using System.Windows;

namespace HAWidget
{
    public partial class SettingsWindow : Window
    {
        public AppConfig Config { get; private set; }

        public SettingsWindow(AppConfig config)
        {
            InitializeComponent();

            Config = new AppConfig
            {
                HaBaseUrl = config.HaBaseUrl,
                Token = config.Token,
                WindowGroups = new List<WindowGroupConfig>()
            };

            foreach (WindowGroupConfig group in config.WindowGroups)
            {
                WindowGroupConfig copiedGroup = new WindowGroupConfig
                {
                    Title = group.Title,
                    WindowLevel = group.WindowLevel,
                    Left = group.Left,
                    Top = group.Top,
                    Width = group.Width,
                    Height = group.Height,
                    Entities = new List<EntityConfig>()
                };

                foreach (EntityConfig sensor in group.Entities)
                {
                    copiedGroup.Entities.Add(new EntityConfig
                    {
                        Title = sensor.Title,
                        EntityId = sensor.EntityId,
                        CustomUnit = sensor.CustomUnit
                    });
                }

                Config.WindowGroups.Add(copiedGroup);
            }

            HaUrlTextBox.Text = Config.HaBaseUrl;
            TokenTextBox.Text = Config.Token;

            RefreshWindowGroupList();
        }

        private void RefreshWindowGroupList()
        {
            WindowGroupListBox.ItemsSource = null;
            WindowGroupListBox.ItemsSource = Config.WindowGroups;
        }

        private void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            WindowGroupConfig newGroup = new WindowGroupConfig
            {
                Title = "新窗口",
                WindowLevel = "Normal",
                Left = 100,
                Top = 100,
                Width = 320,
                Height = 240
            };

            WindowGroupEditorWindow editor = new WindowGroupEditorWindow(newGroup);
            editor.Owner = this;

            if (editor.ShowDialog() == true)
            {
                Config.WindowGroups.Add(editor.Group);
                RefreshWindowGroupList();
            }
        }

        private void EditGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowGroupListBox.SelectedItem is not WindowGroupConfig selectedGroup)
            {
                System.Windows.MessageBox.Show("请先选择一个窗口组。", "提示");
                return;
            }

            WindowGroupConfig copy = new WindowGroupConfig
            {
                Title = selectedGroup.Title,
                WindowLevel = selectedGroup.WindowLevel,
                Left = selectedGroup.Left,
                Top = selectedGroup.Top,
                Width = selectedGroup.Width,
                Height = selectedGroup.Height,
                Entities = new List<EntityConfig>()
            };

            foreach (EntityConfig sensor in selectedGroup.Entities)
            {
                copy.Entities.Add(new EntityConfig
                {
                    Title = sensor.Title,
                    EntityId = sensor.EntityId,
                    CustomUnit = sensor.CustomUnit
                });
            }

            WindowGroupEditorWindow editor = new WindowGroupEditorWindow(copy);
            editor.Owner = this;

            if (editor.ShowDialog() == true)
            {
                selectedGroup.Title = editor.Group.Title;
                selectedGroup.WindowLevel = editor.Group.WindowLevel;
                selectedGroup.Left = editor.Group.Left;
                selectedGroup.Top = editor.Group.Top;
                selectedGroup.Width = editor.Group.Width;
                selectedGroup.Height = editor.Group.Height;
                selectedGroup.Entities = editor.Group.Entities;

                RefreshWindowGroupList();
            }
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowGroupListBox.SelectedItem is not WindowGroupConfig selectedGroup)
            {
                System.Windows.MessageBox.Show("请先选择一个窗口组。", "提示");
                return;
            }

            if (System.Windows.MessageBox.Show($"确定要删除窗口组“{selectedGroup.Title}”吗？",
                "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Config.WindowGroups.Remove(selectedGroup);
                RefreshWindowGroupList();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Config.HaBaseUrl = HaUrlTextBox.Text.Trim();
            Config.Token = TokenTextBox.Text.Trim();

            ConfigService.SaveConfig(Config);

            if (System.Windows.Application.Current is App app)
            {
                app.SaveSettingsAndReload();
            }

            Close();
        }
    }
}