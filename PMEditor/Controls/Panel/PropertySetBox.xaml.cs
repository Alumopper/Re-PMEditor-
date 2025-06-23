using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PMEditor.Controls;

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
                    box.PreviewTextInput += (_, e) =>
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
                    box.PreviewTextInput += (_, e) =>
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
            box.Text = type switch
            {
                PropertyType.Double =>
                    //保留两位小数
                    ((double)value).ToString("0.00"),
                PropertyType.Int => ((int)value).ToString(),
                PropertyType.String => value.ToString(),
                _ => box.Text
            } ?? string.Empty;
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
            box.Text = value.ToString() ?? string.Empty;
            box.Focusable = false;
            return;
        }
        box.Focusable = false;
        value = type switch
        {
            PropertyType.Double => double.TryParse(box.Text, out double d) ? d : value,
            PropertyType.Int => int.TryParse(box.Text, out int i) ? i : value,
            PropertyType.String => box.Text,
            _ => value
        };
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