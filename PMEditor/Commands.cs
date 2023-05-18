using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PMEditor
{
    public static class Commands
    {
        public static readonly RoutedUICommand PlayOrPause = new (
            "开始/暂停", "Play", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Space)
            });

        public static readonly RoutedUICommand ChangeNoteType = new (
            "Drag/Note", "Change Note", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.D,ModifierKeys.Control)
            });

        public static readonly RoutedUICommand Undo = new (
            "撤销", "Undo", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Z,ModifierKeys.Control)
            });


        public static readonly RoutedUICommand Redo = new (
            "重做", "Redo", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Y,ModifierKeys.Control)
            });
    }
}
