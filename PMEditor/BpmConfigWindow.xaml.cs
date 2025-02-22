using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PMEditor;

public partial class BpmConfigWindow : Window
{
    private ObservableCollection<BpmInfo> bpmInfos;
    
    public BpmConfigWindow()
    {
        InitializeComponent();
        bpmInfos = new ObservableCollection<BpmInfo>(EditorWindow.Instance.track.BpmInfo.Select(info => info.Clone()));
        DataGrid.ItemsSource = bpmInfos;
        DefaultBpm.Content = EditorWindow.Instance.track.BaseBpm;
    }

    private void InsertBefore(object sender, RoutedEventArgs e)
    {
        //在选中的位置后插入
        var index = bpmInfos.IndexOf((BpmInfo)DataGrid.SelectedItem);
        if (index == -1)
        {
            bpmInfos.Add(new BpmInfo(EditorWindow.Instance.track.BaseBpm, 0, 0));
        }
        else
        {
            bpmInfos.Insert(bpmInfos.IndexOf((BpmInfo)DataGrid.SelectedItem), new BpmInfo(EditorWindow.Instance.track.BaseBpm, 0, 0));
        }
    }

    private void Delete(object sender, RoutedEventArgs e)
    {
        bpmInfos.Remove((BpmInfo)DataGrid.SelectedItem);
    }

    private void InsertAfter(object sender, RoutedEventArgs e)
    {
        //在选中的位置后插入
        var index = bpmInfos.IndexOf((BpmInfo)DataGrid.SelectedItem);
        if (index == bpmInfos.Count - 1)
        {
            bpmInfos.Add(new BpmInfo(EditorWindow.Instance.track.BaseBpm, 0, 0));
        }
        else
        {
            bpmInfos.Insert(bpmInfos.IndexOf((BpmInfo)DataGrid.SelectedItem) + 1, new BpmInfo(EditorWindow.Instance.track.BaseBpm, 0, 0));
        }
    }

    private void Ok(object sender, RoutedEventArgs e)
    {
        EditorWindow.Instance.track.BpmInfo.Clear();
        foreach (var info in ArrangeBpm())
        {
            EditorWindow.Instance.track.BpmInfo.Add(info);
        }
        //更新判定线
        EditorWindow.Instance.track.UpdateLineTimes();
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private List<BpmInfo> ArrangeBpm()
    {
        var points = new SortedSet<int>();
        var last = 0;
        foreach (var info in bpmInfos)
        {
            if(info.EndMeasure - info.StartMeasure <= 0) continue;
            points.Add(info.StartMeasure);
            points.Add(info.EndMeasure);
            last = info.EndMeasure > last ? info.EndMeasure : last;
        }
        var sortedPoints = points.OrderBy(x => x).ToList();
        var result = new List<BpmInfo>();
        for (var i = 0; i < sortedPoints.Count - 1; i++)
        {
            var start = sortedPoints[i];
            var end = sortedPoints[i + 1];
            var value = (from interval in bpmInfos where interval.StartMeasure <= start && interval.EndMeasure >= end select interval.Value).FirstOrDefault();

            result.Add(new BpmInfo(value, start, end));
        }

        return result;
    }
    

}

public class IntValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return int.TryParse(value as string, out _) ? ValidationResult.ValidResult : new ValidationResult(false, "输入必须是整数");
    }
}
    
public class DoubleValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        return double.TryParse(value as string, out _) ? ValidationResult.ValidResult : new ValidationResult(false, "输入必须是整数");
    }
}