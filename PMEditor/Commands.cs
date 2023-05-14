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
        public static readonly RoutedUICommand PlayOrPause = new RoutedUICommand(
            "开始/暂停", "Play", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.Space)
            });

        public static readonly RoutedUICommand ChangeNoteType = new RoutedUICommand(
            "Drag/Note", "Change Note", typeof(Commands), new InputGestureCollection()
            {
                new KeyGesture(Key.D,ModifierKeys.Control)
            });
    }
}
