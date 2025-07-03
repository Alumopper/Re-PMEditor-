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
            StartTime.Value = e.StartTime;
            EndTime.Value = e.EndTime;
            Functions.ItemsSource = EaseFunctions.functions.Keys;
            Functions.SelectedItem = e.EaseFunctionID;
            EventType.SelectedIndex = e.TypeId;
            StartValue.Value = e.StartValue;
            EndValue.Value = e.EndValue;
        }

        private void Functions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            @event.EaseFunctionID = Functions.SelectedValue.ToString();
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
