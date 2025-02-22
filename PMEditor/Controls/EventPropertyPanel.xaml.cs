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

        bool init = true;

        public EventPropertyPanel(Event e)
        {
            InitializeComponent();
            this.@event = e;
            startTime.Value = e.StartTime;
            endTime.Value = e.EndTime;
            functions.ItemsSource = EaseFunctions.functions.Keys;
            functions.SelectedItem = e.EaseFunctionID;
            eventType.SelectedIndex = e.TypeId;
            startValue.Value = e.StartValue;
            endValue.Value = e.EndValue;
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
            if(@event.ParentList.Events.Count > 1)
            {
                var result = MessageBox.Show(
                    "此轨道上有多个事件，无法修改类型\n可前往设置修改不再弹出提示",
                    "PMEditor",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                    );
                if (result != MessageBoxResult.OK) return;
            }
            @event.ParentList.SetType((EventType)eventType.SelectedIndex);
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateEventTypeList();
        }

        private void Functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            @event.EaseFunctionID = functions.SelectedValue.ToString();
            @event.EaseFunction = EaseFunctions.functions[@event.EaseFunctionID];
        }

        private void startTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.StartTime = value;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateEvent();
        }

        private void endTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.EndTime = value;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateEvent();
        }

        private void startValue_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.StartValue = value;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateEvent();
        }

        private void endValue_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.EndValue = value;
            (EditorWindow.Instance.Page.Content as TrackEditorPage)?.UpdateEvent();
        }
    }
}
