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

        bool init = true;

        public EventPropertyPanel(Event e)
        {
            InitializeComponent();
            this.@event = e;
            startTime.Text = e.StartTime.ToString();
            endTime.Text = e.EndTime.ToString();
            functions.ItemsSource = EaseFunctions.functions.Keys;
            functions.SelectedItem = e.easeFunctionID;
            eventType.SelectedIndex = e.TypeId;
            startValue.Text = e.StartValue.ToString();
            endValue.Text = e.EndValue.ToString();
        }

        //事件类型修改
        private void EventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(init)
            {
                init = false;
                return;
            }
            //修改整个轨道上此类事件的类型
            if(@event.parentList.events.Count > 1)
            {
                var result = MessageBox.Show(
                    "此轨道上有多个事件，无法修改类型\n可前往设置修改不再弹出提示",
                    "PMEditor",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                    );
                if (result != MessageBoxResult.OK) return;
            }
            @event.parentList.SetType((EventType)eventType.SelectedIndex);
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateEventTypeList();
        }

        private void Functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            @event.easeFunctionID = functions.SelectedValue.ToString();
            @event.easeFunction = EaseFunctions.functions[@event.easeFunctionID];
        }

        private void StartTime_KeyDown(object sender, KeyEventArgs e)
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
