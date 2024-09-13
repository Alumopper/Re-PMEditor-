using System.Windows.Controls;

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
