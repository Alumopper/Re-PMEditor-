using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PMEditor.Controls
{
    /// <summary>
    /// 按照步骤 1a 或 1b 操作，然后执行步骤 2 以在 XAML 文件中使用此自定义控件。
    ///
    /// 步骤 1a) 在当前项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PMEditor.Controls"
    ///
    ///
    /// 步骤 1b) 在其他项目中存在的 XAML 文件中使用该自定义控件。
    /// 将此 XmlNamespace 特性添加到要使用该特性的标记文件的根
    /// 元素中:
    ///
    ///     xmlns:MyNamespace="clr-namespace:PMEditor.Controls;assembly=PMEditor.Controls"
    ///
    /// 您还需要添加一个从 XAML 文件所在的项目到此项目的项目引用，
    /// 并重新生成以避免编译错误:
    ///
    ///     在解决方案资源管理器中右击目标项目，然后依次单击
    ///     “添加引用”->“项目”->[浏览查找并选择此项目]
    ///
    ///
    /// 步骤 2)
    /// 继续操作并在 XAML 文件中使用控件。
    ///
    ///     <MyNamespace:RotatingButton/>
    ///
    /// </summary>
    public class RotatingButton : ItemsControl
    {
        private Grid back
        {
            get => Template.FindName("back",this) as Grid;
        }

        private ContentPresenter content
        {
            get => Template.FindName("content", this) as ContentPresenter;
        }

        static RotatingButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RotatingButton), new FrameworkPropertyMetadata(typeof(RotatingButton)));
        }
        private int currentIndex = 0;
        private SolidColorBrush originBackground;

        public static readonly DependencyProperty ContextBackgroundProperty =
            DependencyProperty.RegisterAttached("ContextBackground", typeof(SolidColorBrush), typeof(RotatingButton), new FrameworkPropertyMetadata(Brushes.Transparent));

        public event EventHandler ContentChanged;

        public RotatingButton()
        {
            Loaded += RotatingButton_Loaded;
            MouseDown += RotatingButton_MouseDown;
            MouseEnter += RotatingButton_MouseEnter;
            MouseLeave += RotatingButton_MouseLeave;
        }

        private void RotatingButton_Loaded(object sender, RoutedEventArgs e)
        {
            currentIndex = 0;
            if (Items.Count > 0)
            {
                content.Content = Items[currentIndex];
                back.Background = GetContextBackgroundProperty(Items[currentIndex] as DependencyObject);
            }
        }

        private void RotatingButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Items.Count > 0)
            {
                currentIndex = (currentIndex + 1) % Items.Count;
                content.Content = Items[currentIndex];
                back.Background = GetContextBackgroundProperty(Items[currentIndex] as DependencyObject);
                ContentChanged?.Invoke(this, EventArgs.Empty);
                originBackground = back.Background as SolidColorBrush;
            }
        }

        private void RotatingButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (back.Background is SolidColorBrush brush)
            {
                originBackground = brush.Clone();
                brush = new SolidColorBrush(Color.FromArgb(brush.Color.A, (byte)Math.Min(255, brush.Color.R + 30), (byte)Math.Min(255, brush.Color.G + 30), (byte)Math.Min(255, brush.Color.B + 30)));
                var anim = new ColorAnimation()
                {
                    To = brush.Color,
                    Duration = TimeSpan.FromSeconds(0.1)
                };
                back.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        private void RotatingButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (back.Background is SolidColorBrush)
            {
                var anim = new ColorAnimation()
                {
                    To = originBackground.Color,
                    Duration = TimeSpan.FromSeconds(0.1)
                };
                back.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        public static Brush GetContextBackgroundProperty(DependencyObject obj)
        {
            return (Brush)obj.GetValue(ContextBackgroundProperty);
        }

        public static void SetContextBackgroundProperty(DependencyObject obj, Brush value)
        {
            obj.SetValue(ContextBackgroundProperty, value);
        }
    }
}
