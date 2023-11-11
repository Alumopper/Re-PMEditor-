using PMEditor.Controls;
using PMEditor.Operation;
using System;
using System.Windows.Input;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// 一个note
    /// </summary>
    public partial class Note
    {
        public NoteRectangle rectangle;

        public NoteType type;

        public bool hasJudged = false;

        public MediaPlayer sound = new MediaPlayer();

        public Line parentLine;


        public void SetNoteType(NoteType noteType)
        {
            this.type = noteType;
            this.noteType = (int)noteType;
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Note note)
            {
                return this.actualTime == note.actualTime && this.rail == note.rail;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return actualTime.GetHashCode() + rail.GetHashCode() + noteType.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(type == PMEditor.NoteType.Tap ? "Tap" : "Drag")}[line={parentLine.notes.IndexOf(this)},rail={rail},time={actualTime}]";
        }

        public bool IsOverlap(Note note)
        {
            if(!(note.rail == rail && note.parentLine == parentLine)) return false;
            if(type == PMEditor.NoteType.Hold)
            {
                if(note.type == PMEditor.NoteType.Hold)
                {
                    return actualTime <= note.actualTime && note.actualTime <= actualTime + actualHoldTime || actualTime <= note.actualTime + note.actualHoldTime && note.actualTime + note.actualHoldTime <= actualTime + actualHoldTime;
                }
                else
                {
                    return actualTime <= note.actualTime && note.actualTime <= actualTime + actualHoldTime;
                }
            }
            else
            {
                if(note.type == PMEditor.NoteType.Hold)
                {
                    return note.IsOverlap(this);
                }
                else
                {
                    return note.actualTime == actualTime;
                }
            }
        }

        //右键删除此note
        private void Rectangle_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            parentLine.notes.Remove(this);
            TrackEditorPage.Instance.notePanel.Children.Remove(rectangle);
            OperationManager.AddOperation(new RemoveNoteOperation(this, parentLine));
        }

        //左键选中此note
        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TrackEditorPage.Instance.infoFrame.Content = new NotePropertyPanel(this);
            TrackEditorPage.Instance.UpdateSelectedNote(this);
        }

        private void Sound_MediaEnded(object? sender, EventArgs e)
        {
            (sender as MediaPlayer).Stop();
            (sender as MediaPlayer).Position = new TimeSpan(0);
        }
    }

    public enum NoteType
    {
        Tap, Drag, Hold
    }
}
