using System;
using System.Windows;

namespace PMEditor
{
    /// <summary>
    /// DebugWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DebugWindow : Window
    {
        public static DebugWindow Instance { get; private set; }

        public DebugWindow()
        {
            InitializeComponent();
            this.Title = "Debug";
            Instance = this;
        }

        // 添加一个方法，用于在调试窗口中输出带有时间信息的日志
        public static void Log(string message)
        {
#if DEBUG
            string logMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            Instance.logListBox.Items.Add(logMessage);
#endif
        }
    }
}
