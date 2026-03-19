# HAWidget

一个基于 WPF + .NET 10 的 Home Assistant 桌面小组件程序。

## 当前功能

- 多个 WidgetWindow
- 每个窗口组可显示多个传感器
- 配置面板
- 托盘常驻
- 主界面最小化到托盘
- 右下角缩放控制柄
- 保存窗口位置和大小

## 运行方式

1. 复制 Sample 文件夹下的 `config.example.json` ，复制到程序根目录下并重命名为 `config.json`
2. 启动程序
3. 在程序的系统托盘右键打开设置，填写 Home Assistant 地址和长期访问令牌，即可使用

## 技术栈

- WPF
- .NET 10
- C#
- Home Assistant REST API