using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PMEditor
{
    /// <summary>
    /// EditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditorWindow : Window
    {
        public TrackInfo info;
        public Track track;
        public List<Page> pages;

        public EditorWindow(TrackInfo info, Track track)
        {
            InitializeComponent();
            this.info = info;
            this.track = track;
            this.Title = "PMEditor - " + info.TrackName;
            pages = new List<Page>() { new TrackEditorPage(this), new CodeViewer(this) };
            SetCurrPage(0);
        }

        private void editorButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrPage(0);
        }
        
        private void codeButton_Click(object sender, RoutedEventArgs e)
        {
            SetCurrPage(1);
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void settingButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public void SetCurrPage(int index)
        {
            //设置页面
            page.Content = pages[index];
            //设置按钮状态
            UIElementCollection uIElements = buttonPanel.Children;
            for(int i = 0;i < uIElements.Count;i++)
            {
                if(i == index*2+1 || (i != index*2 && i % 2 == 0))
                {
                    uIElements[i].Visibility = Visibility.Collapsed;
                }
                else
                {
                    uIElements[i].Visibility = Visibility.Visible;
                }
            }
        }
    }
}
