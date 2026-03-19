using System.Collections.Generic;

namespace HAWidget
{
    public class AppConfig
    {
        public string HaBaseUrl { get; set; } = "http://127.0.0.1:8123";
        public string Token { get; set; } = "";

        public List<WindowGroupConfig> WindowGroups { get; set; } = new List<WindowGroupConfig>();
    }

    public class WindowGroupConfig
    {
        public string Title { get; set; } = "新窗口";
        public string WindowLevel { get; set; } = "Normal";

        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 320;
        public double Height { get; set; } = 240;

        public List<EntityConfig> Entities { get; set; } = new List<EntityConfig>();
    }

    public class EntityConfig
    {
        public string Title { get; set; } = "新实体";
        public string EntityId { get; set; } = "sensor.example";
        public string CustomUnit { get; set; } = "";

        // 先保留一个通用类型字段，后面做专用控制界面会用到
        // 可选值先约定：Sensor / Light / Fan / Climate
        public string EntityType { get; set; } = "Sensor";
    }
}