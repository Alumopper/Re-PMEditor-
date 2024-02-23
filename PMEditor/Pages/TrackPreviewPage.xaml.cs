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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PMEditor.Pages
{
    /// <summary>
    /// TrackPreviewPage.xaml 的交互逻辑
    /// </summary>
    public partial class TrackPreviewPage : Page
    {
        Track track;

        public TrackPreviewPage(Track track)
        {
            InitializeComponent();
            this.track = track;
        }
    }
}
