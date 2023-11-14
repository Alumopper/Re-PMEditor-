using PMEditor.Util;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            eventType.IsEnabled = e.parentList.IsMainEvent() && e.parentList.events.Count == 1;
            startTime.Text = e.StartTime.ToString();
            endTime.Text = e.EndTime.ToString();
            functions.ItemsSource = EaseFunctions.functions.Keys;
            functions.SelectedItem = e.easeFunctionID;
            eventType.SelectedIndex = e.TypeId;
            startValue.Text = e.StartValue.ToString();
            endValue.Text = e.EndValue.ToString();
        }

        private void eventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            @event.easeFunctionID = functions.SelectedValue.ToString();
            @event.easeFunction = EaseFunctions.functions[@event.easeFunctionID];
        }

        private void startTime_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key != Key.Enter) return;
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
            Keyboard.ClearFocus();
        }

        private void endTime_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
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
            Keyboard.ClearFocus();
        }

        private void startValue_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (startValue.IsReadOnly) return;
            var qwq = double.TryParse(startValue.Text, out double value);
            if (qwq)
            {
                @event.startValue = value;
            }
            else
            {
                startValue.Text = @event.startValue.ToString();
            }
            Keyboard.ClearFocus();
        }

        private void endValue_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (endValue.IsReadOnly) return;
            var qwq = double.TryParse(endValue.Text, out double value);
            if (qwq)
            {
                @event.endValue = value;
            }
            else
            {
                endValue.Text = @event.endValue.ToString();
            }
            Keyboard.ClearFocus();
        }
    }
}
