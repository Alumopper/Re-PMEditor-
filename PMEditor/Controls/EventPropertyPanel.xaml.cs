﻿using Newtonsoft.Json.Linq;
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
            startTime.Value = e.StartTime;
            endTime.Value = e.EndTime;
            functions.ItemsSource = EaseFunctions.functions.Keys;
            functions.SelectedItem = e.easeFunctionID;
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

        private void startTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.StartTime = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateEvent();
        }

        private void endTime_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.EndTime = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateEvent();
        }

        private void startValue_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.startValue = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateEvent();
        }

        private void endValue_PropertyChangeEvent(object sender, RoutedEventArgs e)
        {
            var value = (double)((PropertyChangeEventArgs)e).PropertyValue;
            @event.endValue = value;
            (EditorWindow.Instance.page.Content as TrackEditorPage)?.UpdateEvent();
        }
    }
}
