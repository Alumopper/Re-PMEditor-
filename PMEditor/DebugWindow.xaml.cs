using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// DebugWindow.xaml 的交互逻辑
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class DebugWindow : Window
    {
        public static DebugWindow? Instance { get; private set; }
        
        public DebugWindow()
        {
            InitializeComponent();
            this.Title = "Debug";
            Instance = this;
        }

        // 添加一个方法，用于在调试窗口中输出带有时间信息的日志
        public static void Log(string message)
        {
            var logMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            Instance?.logListBox.Items.Add(logMessage);
        }

        public static void SetDebugContext(string value, int index)
        {
            if(Instance == null) return;
            while (index >= Instance.Panel.Children.Count)
            {
                Instance.Panel.Children.Add(new TextBlock
                {
                    FontSize = 12,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = Instance.Panel.ActualWidth,
                });
            }
            (Instance.Panel.Children[index] as TextBlock)!.Text = value;
        }
    }
}
