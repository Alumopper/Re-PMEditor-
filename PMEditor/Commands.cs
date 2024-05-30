using PMEditor.Operation;
using System.IO;
using System.Windows.Input;

namespace PMEditor
{
    public static class Commands
    {
        public static readonly RoutedUICommand PlayOrPause = new(
            "开始/暂停", "Play", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Space)
            });

        public static readonly RoutedUICommand ChangeNoteType = new(
            "Catch/Tap", "Change Note", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.D,ModifierKeys.Control)
            });

        public static readonly RoutedUICommand Undo = new(
            "撤销", "Undo", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Z,ModifierKeys.Control)
            });


        public static readonly RoutedUICommand Redo = new(
            "重做", "Redo", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Z,ModifierKeys.Control | ModifierKeys.Shift)
            });

        public static readonly RoutedUICommand Save = new(
            "保存", "Save", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture (Key.S,ModifierKeys.Control)
            });
    }

    public partial class EditorWindow
    {
        //SPACE - 播放/暂停
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = player.HasAudio;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (isPlaying)
            {
                player.Pause();
                isPlaying = false;
            }
            else
            {
                player.Play();
                isPlaying = true;
            }
        }

        //Shift - 改变note种类
        private void CommandBinding_CanExecute_1(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currPageIndex == 0;
        }

        private void CommandBinding_Executed_1(object sender, ExecutedRoutedEventArgs e)
        {
            puttingTap = !puttingTap;
            (pages[0] as TrackEditorPage)!.FlushNotePreview();
        }

        //撤销
        private void CommandBinding_CanExecute_2(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OperationManager.CanUndo;
        }

        private void CommandBinding_Executed_2(object sender, ExecutedRoutedEventArgs e)
        {
            var op = OperationManager.Undo();
            //更改标题
            if (OperationManager.HasSaved)
            {
                this.Title = "Re:PMEditor - " + info.TrackName;
            }
            else
            {
                this.Title = "Re:PMEditor - " + info.TrackName + " *";
            }
            this.operationInfo.Text = "撤销了 " + op!.GetInfo();
        }

        //重做
        private void CommandBinding_CanExecute_3(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OperationManager.CanRedo;
        }

        private void CommandBinding_Executed_3(object sender, ExecutedRoutedEventArgs e)
        {
            var op = OperationManager.Redo();
            //更改标题
            if (OperationManager.HasSaved)
            {
                this.Title = "Re:PMEditor - " + info.TrackName;
            }
            else
            {
                this.Title = "Re:PMEditor - " + info.TrackName + " *";
            }
            this.operationInfo.Text = "重做了 " + op!.GetInfo();
        }

        //保存
        private void CommandBinding_CanExecute_4(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currPageIndex == 0;
        }

        private void CommandBinding_Executed_4(object sender, ExecutedRoutedEventArgs e)
        {
            OperationManager.savedOperationIndex = OperationManager.Index;
            this.Title = "Re:PMEditor - " + info.TrackName;
            string text = track.ToJsonString();
            File.WriteAllText("./tracks/" + track.TrackName + "/track.json", text);
            this.operationInfo.Text = "谱面文件已保存";
        }
    }
}
