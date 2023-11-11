using System.Windows.Controls;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// NoteRectangle.xaml 的交互逻辑
    /// </summary>
    public partial class EventRectangle : UserControl
    {
        /// <summary>
        /// 这个矩形对应的note
        /// </summary>
        public Event @event;

        public EventRectangle(Event @event)
        {
            InitializeComponent();
            this.@event = @event;
        }

        public Brush Fill
        {
            get => rect.Fill; set => rect.Fill = value;
        }

        public Brush HighLightBorderBrush
        {
            get => highLightBorder.BorderBrush; set => highLightBorder.BorderBrush = value;
        }

        private bool highLight;
        public bool HighLight
        {
            get => highLight;
            set
            {
                highLight = value;
                //高亮
                if (highLight)
                {
                    highLightBorder.BorderThickness = new(2);
                }
                else
                {
                    highLightBorder.BorderThickness = new(0);
                }
            }
        }
    }
}
