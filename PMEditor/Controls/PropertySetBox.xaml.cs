using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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

namespace PMEditor.Controls
{
    /// <summary>
    /// PropertySetBox.xaml 的交互逻辑
    /// </summary>
    public partial class PropertySetBox : UserControl
    {
        private PropertyType type;
        public PropertyType Type
        {
            get => type;
            set
            {
                type = value;
                switch (value)
                {
                    case PropertyType.Double:
                        box.PreviewTextInput += (sender, e) =>
                        {
                            if (e.Text == ".")
                            {
                                if (box.Text.Contains("."))
                                {
                                    e.Handled = true;
                                }
                                return;
                            }
                            if (!char.IsDigit(e.Text, 0))
                            {
                                e.Handled = true;
                            }
                        };
                        break;
                    case PropertyType.Int:
                        box.PreviewTextInput += (sender, e) =>
                        {
                            if (!char.IsDigit(e.Text, 0))
                            {
                                e.Handled = true;
                            }
                        };
                        break;
                    case PropertyType.String:
                        break;
                }
            }
        }

        private object value;
        public object Value
        {
            get => value;
            set
            {
                switch (type)
                {
                    case PropertyType.Double:
                        //保留两位小数
                        box.Text = ((double)value).ToString("0.00");
                        break;
                    case PropertyType.Int:
                        box.Text = ((int)value).ToString();
                        break;
                    case PropertyType.String:
                        box.Text = value.ToString();
                        break;
                }
            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get => isReadOnly;
            set
            {
                isReadOnly = value;
                box.IsReadOnly = value;
            }
        }

        public static readonly RoutedEvent PropertyChange = EventManager.RegisterRoutedEvent(
            "PropertyChange", 
            RoutingStrategy.Bubble, 
            typeof(RoutedEventHandler), 
            typeof(PropertySetBox)
            );

        public event RoutedEventHandler PropertyChangeEvent
        {
            add { AddHandler(PropertyChange, value); }
            remove { RemoveHandler(PropertyChange, value); }
        }

        public PropertySetBox()
        {
            InitializeComponent();
            type = PropertyType.Double;
        }

        private void TextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            box.Focusable = true;
            box.Focus();
            box.SelectAll();
        }

        protected virtual void OnPropertyChange()
        {
            PropertyChangeEventArgs args = new(PropertyChange, value);
            RaiseEvent(args);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Escape) return;
            if (isReadOnly) return;
            if (e.Key == Key.Escape)
            {
                box.Text = value.ToString();
                box.Focusable = false;
                return;
            }
            box.Focusable = false;
            switch (type)
            {
                case PropertyType.Double:
                    value = double.TryParse(box.Text, out double d) ? d : value;
                    break;
                case PropertyType.Int:
                    value = int.TryParse(box.Text, out int i) ? i : value;
                    break;
                case PropertyType.String:
                    value = box.Text;
                    break;
            }
            OnPropertyChange();
        }

        public enum PropertyType
        {
            Double,
            Int,
            String
        }
    }

    public class PropertyChangeEventArgs : RoutedEventArgs
    {
        public object PropertyValue { get; set; }

        public PropertyChangeEventArgs(RoutedEvent routedEvent, object propertyValue) : base(routedEvent)
        {
            PropertyValue = propertyValue;
        }
    }
}
