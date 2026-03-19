using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace HAWidget
{
    public partial class WindowGroupEditorWindow : Window
    {
        public WindowGroupConfig Group { get; private set; }

        public WindowGroupEditorWindow(WindowGroupConfig group)
        {
            InitializeComponent();

            Group = group;

            TitleTextBox.Text = Group.Title;
            LeftTextBox.Text = Group.Left.ToString(CultureInfo.InvariantCulture);
            TopTextBox.Text = Group.Top.ToString(CultureInfo.InvariantCulture);
            WidthTextBox.Text = Group.Width.ToString(CultureInfo.InvariantCulture);
            HeightTextBox.Text = Group.Height.ToString(CultureInfo.InvariantCulture);

            SetWindowLevelSelection(Group.WindowLevel);

            RefreshSensorList();
        }

        private void SetWindowLevelSelection(string level) // 窗口层级选择
        {
            foreach (ComboBoxItem item in WindowLevelComboBox.Items)
            {
                //if (item.Content?.ToString() == level)
                //{
                //    WindowLevelComboBox.SelectedItem = item;
                //    return;
                //}
                if ((String)item.Tag == Group.WindowLevel)
                {
                    WindowLevelComboBox.SelectedItem = item;
                    break;
                }
            }

            WindowLevelComboBox.SelectedIndex = 1;
        }

        private void RefreshSensorList()
        {
            SensorListBox.ItemsSource = null;
            SensorListBox.ItemsSource = Group.Entities;
        }

        private void AddSensorButton_Click(object sender, RoutedEventArgs e)
        {
            EntityConfig newEntity = new EntityConfig
            {
                Title = "新传感器",
                EntityId = "sensor.example",
                CustomUnit = ""
            };

            SensorEditorWindow editor = new SensorEditorWindow(newEntity);
            editor.Owner = this;

            if (editor.ShowDialog() == true)
            {
                Group.Entities.Add(editor.Entity);
                RefreshSensorList();
            }
        }

        private void EditSensorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SensorListBox.SelectedItem is not EntityConfig selectedEntity)
            {
                System.Windows.MessageBox.Show("请先选择一个实体。", "提示");
                return;
            }

            SensorEditorWindow editor = new SensorEditorWindow(selectedEntity);
            editor.Owner = this;
            editor.ShowDialog();

            RefreshSensorList();
        }

        private void DeleteSensorButton_Click(object sender, RoutedEventArgs e)
        {
            if (SensorListBox.SelectedItem is not EntityConfig selectedSensor)
            {
                System.Windows.MessageBox.Show("请先选择一个实体。", "提示");
                return;
            }

            if (System.Windows.MessageBox.Show($"确定要删除“{selectedSensor.Title}”吗？",
                "确认", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes)
            {
                Group.Entities.Remove(selectedSensor);
                RefreshSensorList();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                System.Windows.MessageBox.Show("窗口标题不能为空。", "提示");
                return;
            }

            if (!double.TryParse(LeftTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double left))
            {
                System.Windows.MessageBox.Show("Left 不是有效数字。", "提示");
                return;
            }

            if (!double.TryParse(TopTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double top))
            {
                System.Windows.MessageBox.Show("Top 不是有效数字。", "提示");
                return;
            }

            if (!double.TryParse(WidthTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double width))
            {
                System.Windows.MessageBox.Show("Width 不是有效数字。", "提示");
                return;
            }

            if (!double.TryParse(HeightTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double height))
            {
                System.Windows.MessageBox.Show("Height 不是有效数字。", "提示");
                return;
            }

            if (width < 150 || height < 100)
            {
                System.Windows.MessageBox.Show("窗口尺寸太小了。", "提示");
                return;
            }

            Group.Title = TitleTextBox.Text.Trim();
            Group.Left = left;
            Group.Top = top;
            Group.Width = width;
            Group.Height = height;

            if (WindowLevelComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Group.WindowLevel = selectedItem.Tag?.ToString() ?? "Normal";
            }
            else
            {
                Group.WindowLevel = "Normal";
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}