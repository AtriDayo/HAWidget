using System.Windows;
using System.Windows.Controls;

namespace HAWidget
{
    public partial class SensorEditorWindow : Window
    {
        public EntityConfig Entity { get; private set; }

        public SensorEditorWindow(EntityConfig entity)
        {
            InitializeComponent();

            Entity = entity;

            TitleTextBox.Text = Entity.Title;
            EntityIdTextBox.Text = Entity.EntityId;
            UnitTextBox.Text = Entity.CustomUnit;

            SelectEntityType(Entity.EntityType);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Entity.Title = TitleTextBox.Text.Trim();
            Entity.EntityId = EntityIdTextBox.Text.Trim();
            Entity.CustomUnit = UnitTextBox.Text.Trim();

            if (EntityTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Entity.EntityType = selectedItem.Tag?.ToString() ?? "Sensor";
            }
            else
            {
                Entity.EntityType = "Sensor";
            }

            if (string.IsNullOrWhiteSpace(Entity.Title))
            {
                System.Windows.MessageBox.Show("标题不能为空。", "提示");
                return;
            }

            if (string.IsNullOrWhiteSpace(Entity.EntityId))
            {
                System.Windows.MessageBox.Show("实体 ID 不能为空。", "提示");
                return;
            }

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SelectEntityType(string type)
        {
            foreach (ComboBoxItem item in EntityTypeComboBox.Items)
            {
                if ((item.Tag?.ToString() ?? "") == type)
                {
                    EntityTypeComboBox.SelectedItem = item;
                    return;
                }
            }

            EntityTypeComboBox.SelectedIndex = 0;
        }
    }
}