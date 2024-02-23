using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PMEditor.Controls
{
    /// <summary>
    /// CycleButton.xaml 的交互逻辑
    /// </summary>
    public partial class CycleButton : UserControl
    {
        private bool hasInit = false;
        private SolidColorBrush originBackground;

        public static readonly DependencyProperty ContentsProperty =
            DependencyProperty.Register("Contents", typeof(ObservableCollection<UIElement>), typeof(CycleButton), new PropertyMetadata(new ObservableCollection<UIElement>()));
        
        public static readonly DependencyProperty ContextBackgroundProperty =
            DependencyProperty.RegisterAttached("ContextBackground", typeof(Brush), typeof(CycleButton), new FrameworkPropertyMetadata(Brushes.Transparent));

        public ObservableCollection<UIElement> Contents
        {
            get { return (ObservableCollection<UIElement>)GetValue(ContentsProperty); }
            set { 
                SetValue(ContentsProperty, value);
                if (!hasInit)
                {
                    hasInit = true;
                    if(Contents.Count >  0)
                    {
                        currentIndex = 0;
                        back.Background = GetContextBackgroundProperty(Contents[0]);
                    }
                }
            }
        }

        private int currentIndex = 0;

        public UIElement? CurrentContent
        {
            get { return Contents.Count > 0 ? Contents[currentIndex] : null; }
        }

        public int CurrentIndex
        {
            get { return currentIndex; }
        }

        public event EventHandler ContentChanged;

        public CycleButton()
        {
            InitializeComponent();
            Loaded += CycleButton_Loaded;
            MouseDown += CycleButton_MouseDown;
            MouseEnter += CycleButton_MouseEnter;
            MouseLeave += CycleButton_MouseLeave;
        }

        private void CycleButton_Loaded(object sender, RoutedEventArgs e)
        {
            currentIndex = 0;
            //设置第一个元素为内容
            if (Contents.Count > 0)
            {
                contentPresenter.Content = Contents[currentIndex];
                back.Background = GetContextBackgroundProperty(Contents[currentIndex]);
            }
        }

        private void CycleButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Contents.Count > 0)
            {
                currentIndex = (currentIndex + 1) % Contents.Count;
                contentPresenter.Content = Contents[currentIndex];
                back.Background = GetContextBackgroundProperty(Contents[currentIndex]) ?? Brushes.White;
                ContentChanged?.Invoke(this, EventArgs.Empty);
                CycleButton_MouseEnter(sender, e);
            }
        }

        private void CycleButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (back.Background is SolidColorBrush brush)
            {
                DebugWindow.Log("enter/brush:" + brush.ToString());
                originBackground = brush.Clone();
                DebugWindow.Log("enter/originBackground:" + brush.ToString());
                brush = new SolidColorBrush(Color.FromArgb(brush.Color.A, (byte)Math.Min(255, brush.Color.R + 30), (byte)Math.Min(255, brush.Color.G + 30), (byte)Math.Min(255, brush.Color.B + 30)));
                DebugWindow.Log("enter/ChangedBrush:" + brush.ToString());
                DebugWindow.Log("enter/ChangedOriginBackground:" + originBackground.ToString());
                var anim = new ColorAnimation()
                {
                    To = brush.Color,
                    Duration = TimeSpan.FromSeconds(0.1)
                };
                back.Background.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        private void CycleButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (back.Background is SolidColorBrush)
            {
                DebugWindow.Log("exit/originBackground:" + originBackground.ToString());
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
