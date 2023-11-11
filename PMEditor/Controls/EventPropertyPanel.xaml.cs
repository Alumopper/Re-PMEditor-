using PMEditor.Util;
using System.Windows;
using System.Windows.Controls;

namespace PMEditor.Controls
{
    /// <summary>
    /// PropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class EventPropertyPanel : UserControl
    {
        Event @event;

        public EventPropertyPanel(Event e)
        {
            InitializeComponent();
            this.@event = e;
            eventType.IsEnabled = e.parentLine.eventList.isMainEvent(e.rail) && e.parentLine.eventList.Count == 1;
            startTime.Text = e.StartTime.ToString();
            endTime.Text = e.EndTime.ToString();
            functions.ItemsSource = EaseFunctions.functions.Keys;
            functions.SelectedItem = e.easeFunctionID;
            eventType.SelectedIndex = e.TypeId;
        }

        private void eventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void startTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (startTime.IsReadOnly) return;
            var qwq = double.TryParse(startTime.Text, out double value);
            if (qwq)
            {
                @event.StartTime = value;
            }
            else
            {
                startTime.Text = @event.StartTime.ToString();
            }
        }

        private void endTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (endTime.IsReadOnly) return;
            var qwq = double.TryParse(endTime.Text, out double value);
            if (qwq)
            {
                @event.EndTime = value;
            }
            else
            {
                endTime.Text = @event.EndTime.ToString();
            }
        }

        private void functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            @event.easeFunctionID = functions.SelectedValue.ToString();
            @event.easeFunction = EaseFunctions.functions[@event.easeFunctionID];
        }
    }
}
