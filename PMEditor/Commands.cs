using System;
using PMEditor.Operation;
using System.IO;
using System.Windows;
using System.Windows.Input;
using PMEditor.EditorTool;

namespace PMEditor
{
    public static class Commands
    {
        public static readonly RoutedUICommand PlayOrPause = new(
            "开始/暂停", "Play", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.Space)
            });

        public static readonly RoutedUICommand ChangeNoteType = new(
            "Catch/Tap", "Change Note", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.F,ModifierKeys.Control)
            });

        public static readonly RoutedUICommand Undo = new(
            "撤销", "Undo", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.Z,ModifierKeys.Control)
            });

        public static readonly RoutedUICommand Redo = new(
            "重做", "Redo", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.Z,ModifierKeys.Control | ModifierKeys.Shift)
            });

        public static readonly RoutedUICommand Save = new(
            "保存", "Save", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture (Key.S,ModifierKeys.Control)
            });

        public static readonly RoutedUICommand Copy = new(
            "复制", "Copy", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.C, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Paste = new(
            "粘贴", "Paste", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.V, ModifierKeys.Control)
            }
        );
        
        public static readonly RoutedUICommand Cut = new(
            "剪切", "Cut", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.X, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Delete = new(
            "删除", "Delete", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.Delete)
            }
        );

        public static readonly RoutedUICommand ArrowTool = new(
            "箭头工具", "ArrowTool", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.A, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand EraserTool = new(
            "橡皮擦", "EraserTool", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.D, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand MoveTool = new(
            "移动工具", "MoveTool", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.W, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand ResizeTool = new(
            "缩放工具", "ResizeTool", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.R, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand PutTool = new(
            "放置工具", "PutTool", typeof(Commands), new InputGestureCollection
            {
                new KeyGesture(Key.E, ModifierKeys.Control)
            }
        );
    }

    public partial class EditorWindow
    {
        //SPACE - 播放/暂停
        private void PlayOrPauseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = player.HasAudio;
        }

        private void PlayOrPauseExecuted(object sender, ExecutedRoutedEventArgs e)
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
        private void ChangeNoteTypeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currPageIndex == 0;
        }

        private void ChangeNoteTypeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            puttingTap = !puttingTap;
            TrackEditorPage.Instance!.FlushObjPreviewFill();
        }

        //撤销
        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OperationManager.CanUndo;
        }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs e)
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
            this.OperationInfo.Text = "撤销了 " + op!.GetInfo();
        }

        //重做
        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OperationManager.CanRedo;
        }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs e)
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
            this.OperationInfo.Text = "重做了 " + op!.GetInfo();
        }

        //保存
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currPageIndex == 0;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OperationManager.savedOperationIndex = OperationManager.Index;
            this.Title = "Re:PMEditor - " + info.TrackName;
            string text = track.ToJsonString();
            try
            {
                File.WriteAllText("./tracks/" + track.TrackName + "/track.json", text);
            }
            catch (Exception e1)
            {
                MessageBox.Show("保存谱面的时候遇到了问题QAQ\n" + e1, "错误", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            this.OperationInfo.Text = "谱面文件已保存";
        }
        
        private void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.CopyCanExecute(sender, e);
                    break;
            }
        }

        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.CopyExecuted(sender, e);
                    break;
            }
        }

        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.PasteCanExecute(sender, e);
                    break;
            }
        }

        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.PasteExecuted(sender, e);
                    break;
            }
        }

        private void CutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.CutCanExecute(sender, e);
                    break;
            }
        }

        private void CutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.CutExecuted(sender, e);
                    break;
            }
        }

        private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.DeleteCanExecute(sender, e);
                    break;
            }
        }

        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            switch (currPageIndex)
            {
                case 0:
                    TrackEditorPage.Instance!.CurrPanel.DeleteExecuted(sender, e);
                    break;
            }
        }

        private void ToolCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = currPageIndex == 0;
        }

        private void ToolExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == Commands.ArrowTool)
            {
                TrackEditorPage.Instance!.UpdateToolBarStatus(EditorToolType.Arrow);
            }
            else if(e.Command == Commands.EraserTool)
            {
                TrackEditorPage.Instance!.UpdateToolBarStatus(EditorToolType.Eraser);
            }
            else if (e.Command == Commands.MoveTool)
            {
                TrackEditorPage.Instance!.UpdateToolBarStatus(EditorToolType.Move);
            }
            else if (e.Command == Commands.ResizeTool)
            {
                TrackEditorPage.Instance!.UpdateToolBarStatus(EditorToolType.Resize);
            }
            else if (e.Command == Commands.PutTool)
            {
                TrackEditorPage.Instance!.UpdateToolBarStatus(EditorToolType.Put);
            }
        }
    }

    partial class TrackEditorPage
    {
    }
}
