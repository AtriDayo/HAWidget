namespace HAWidget
{
    public class EntityViewItem
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "--";
        public string Status { get; set; } = "";
        public EntityConfig Config { get; set; } = new EntityConfig();

        // 后面控制灯/风扇/空调时会逐步用到这些字段
        public bool IsOn { get; set; } = false;
        public string RawState { get; set; } = "";
    }
}