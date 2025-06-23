using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace PMEditor.Util;

public class WatchDog
{
    private readonly Thread watchdogThread;
    private readonly TimeSpan checkInterval;
    private DateTime lastActivityTime;
    private readonly TimeSpan maxInactiveTime;
    private volatile bool isRunning;

    public WatchDog(TimeSpan checkInterval, TimeSpan maxInactiveTime)
    {
        this.checkInterval = checkInterval;
        this.maxInactiveTime = maxInactiveTime;
        lastActivityTime = DateTime.Now;
        isRunning = true;

        watchdogThread = new Thread(WatchdogLoop)
        {
            IsBackground = true
        };
        watchdogThread.Start();

    }

    private void WatchdogLoop()
    {
        while (isRunning)
        {
            Thread.Sleep(checkInterval);
            
            if (DateTime.Now - lastActivityTime <= maxInactiveTime) continue;
            var result = MessageBox.Show(
                $"程序已超过{(DateTime.Now - lastActivityTime).TotalSeconds:F}未操作，是否保存当前谱面并退出?", 
                "提示",
                MessageBoxButton.YesNo
            );
            if (result == MessageBoxResult.Yes)
            {
                // 执行保存操作
                var window = EditorWindow.Instance;
                string text = window.track.ToJsonString();
                try
                {
                    File.WriteAllText("./tracks/" + window.track.TrackName + "/track.json", text);
                }
                catch (Exception e1)
                {
                    MessageBox.Show("保存谱面的时候遇到了问题QAQ\n" + e1, "错误", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                //继续等待
                lastActivityTime = DateTime.Now;
            }
        }
    }

    public void ReportActivity()
    {
        lastActivityTime = DateTime.Now;
    }

    public void Dispose()
    {
        isRunning = false;
        watchdogThread.Join(checkInterval);
    }
}