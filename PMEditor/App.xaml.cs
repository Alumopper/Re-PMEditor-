using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace PMEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Startup += App_Startup;
            this.Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if(EditorWindow.Instance == null)
            {
                MessageBox.Show("啊呀，制谱器遇到了不会解决的问题呢\n" + (e.ExceptionObject as Exception)?.StackTrace, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ExceptionHandler((Exception)e.ExceptionObject);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (EditorWindow.Instance == null)
            {
                MessageBox.Show("啊呀，制谱器遇到了不会解决的问题呢\n" + e.Exception.StackTrace, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ExceptionHandler(e.Exception);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

            if (EditorWindow.Instance == null)
            {
                MessageBox.Show("啊呀，制谱器遇到了不会解决的问题呢\n" + e.Exception.StackTrace, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                ExceptionHandler(e.Exception);
            }
        }

        private void ExceptionHandler(Exception e)
        {
            EditorWindow window = EditorWindow.Instance;
            switch (e)
            {
                default:
                    {
                        var re = MessageBox.Show("啊呀，制谱器坏掉了，不过我们仍然会尝试保存你的项目ヽ(*。>Д<)o゜\n" + e.ToString(), "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        if(re == MessageBoxResult.OK)
                        {
                            string text = window.track.ToJsonString();
                            File.WriteAllText("./tracks/" + window.track.TrackName + "/track.json", text);
                        }
                        this.Shutdown();
                        break;
                    }
            }
        }
    }
}
