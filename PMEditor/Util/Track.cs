using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Transactions;
using System.Windows.Media;

namespace PMEditor
{
    /// <summary>
    /// 一个类，用于存储谱面信息
    /// </summary>
    public class Track
    {
        public string trackName;    //谱面名字
        public string musicAuthor;  //曲师
        public string trackAuthor;  //谱师
        public double bpm;          //bpm
        public double length;       //曲目长度
        public string difficulty;   //谱面难度
        public ObservableCollection<Line> lines;    //判定线

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var line in lines)
                {
                    foreach (var note in line.notes)
                    {
                        count += note.GetCount();
                    }
                }
                return count;
            }
        }

        #region getter and setter
        public string TrackName
        {
            get { return trackName; }
            set { trackName = value; }
        }

        public string MusicAuthor
        {
            get { return musicAuthor; }
            set { musicAuthor = value; }
        }

        public string TrackAuthor
        {
            get { return trackAuthor; }
            set { trackAuthor = value; }
        }

        public double Bpm
        {
            get { return bpm; }
            set { bpm = value; }
        }

        public double Length
        {
            get { return length; }
            set { length = value; }
        }

        public string Difficulty
        {
            get { return difficulty; }
            set { difficulty = value; }
        }

        public ObservableCollection<Line> Lines
        {
            get { return lines; }
            set { lines = value; }
        }
        #endregion

        public Track(string trackName, string musicAuthor, string trackAuthor, double bpm, double length, string difficulty)
        {
            this.trackName = trackName;
            this.musicAuthor = musicAuthor;
            this.trackAuthor = trackAuthor;
            this.bpm = bpm;
            this.length = length;
            this.difficulty = difficulty;
            this.lines = new();
            lines.Add(new Line());
        }

        public static Track? GetTrack(FileInfo trackFile)
        {
            string all = trackFile.OpenText().ReadToEnd();
            Track track = JsonSerializer.Deserialize<Track>(all)!;
            foreach (var line in track.lines)
            {
                foreach (var note in line.notes)
                {
                    note.parentLine = line;
                }
                foreach (var item in line.fakeCatch)
                {
                    item.parentLine = line;
                }
                for (int i = 0; i < line.eventLists.Count; i++)
                {
                    line.eventLists[i].parentLine = line;
                    line.eventLists[i].events.ForEach(e =>
                    {
                        e.parentList = line.eventLists[i];
                        line.SetType(e.type, i);
                    });
                    line.eventLists[i].GroupEvent();
                }
            }
            return track;
        }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        }
    }

    public partial class Line
    {
        public string id;           //判定线的名字
        public double y;            //判定线的y坐标
        public List<Note> notes;    //note
        public List<FakeCatch> fakeCatch;
        public List<EventList> eventLists;  //事件
        public List<Function> functions;

        #region getter and setter
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public List<Note> Notes
        {
            get { return notes; }
            set { notes = value; }
        }

        public List<EventList> EventLists
        {
            get { return eventLists; }
            set { eventLists = value; }
        }

        public List<FakeCatch> FakeCatch
        {
            get { return fakeCatch; }
            set { fakeCatch = value; }
        }

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        public List<Function> Functions
        {
            get { return functions; }
            set { functions = value; }
        }
        
        #endregion

        public Line(double y, string id = "newLine")
        {
            this.y = y;
            this.id = id;
            this.notes = new();
            this.eventLists = new();
            this.fakeCatch = new();
            this.functions = new();
        }

        public Line() : this(0) { }
    }

    public partial class Note
    {
        public int rail;                //轨道
        public int noteType;            //note类型
        public int fallType;            //掉落类型
        public bool isFake;             //是否是假键

        public double actualTime;       //准确的判定时间
        public double actualHoldTime;   //准确的按住时间

        #region getter and setter
        public int Rail
        {
            get => rail; set => rail = value;
        }

        public int NoteType
        {
            get => noteType; set => noteType = value;
        }

        public int FallType
        {
            get => fallType; set => fallType = value;
        }

        public bool IsFake
        {
            get => isFake; set => isFake = value;
        }

        public double ActualTime
        {
            get => actualTime; set => actualTime = value;
        }

        public double ActualHoldTime
        {
            get => actualHoldTime; set => actualHoldTime = value;
        }

        #endregion

        public Color Color
        {
            set
            {
                rectangle.Fill = new SolidColorBrush(value);
            }
        }

        [JsonConstructor]
        public Note(int rail, int noteType, int fallType, bool isFake, double actualTime, double actualHoldTime = 0)
            : this(rail, noteType, fallType, isFake, actualTime, false, actualHoldTime) { }


        public Note(int rail, int noteType, int fallType, bool isFake, double actualTime, bool isCurrentLineNote, double actualHoldTime = 0)
        {
            this.rail = rail;
            this.noteType = noteType;
            this.fallType = fallType;
            this.isFake = isFake;
            this.actualTime = actualTime;
            this.actualHoldTime = actualHoldTime;

            this.type = (NoteType)Enum.Parse(typeof(NoteType), noteType.ToString());

            rectangle = new NoteRectangle(this);

            if (noteType == (int)PMEditor.NoteType.Tap)
            {
                rectangle.Fill = isCurrentLineNote ? new SolidColorBrush(EditorColors.tapColor) : new SolidColorBrush(EditorColors.tapColorButNotOnThisLine);
                rectangle.HighLightBorderBrush = new SolidColorBrush(EditorColors.tapHighlightColor);
            }
            else if (noteType == (int)PMEditor.NoteType.Hold)
            {
                rectangle.Fill = isCurrentLineNote ? new SolidColorBrush(EditorColors.holdColor) : new SolidColorBrush(EditorColors.holdHighlightColor);
                rectangle.HighLightBorderBrush = new SolidColorBrush(EditorColors.holdHighlightColor);
            }
            else if (noteType == (int)PMEditor.NoteType.Catch)
            {
                rectangle.Fill = isCurrentLineNote ? new SolidColorBrush(EditorColors.catchColor) : new SolidColorBrush(EditorColors.catchColorButNotOnThisLine);
                rectangle.HighLightBorderBrush = new SolidColorBrush (EditorColors.catchHighlightColor);
            }
        }
    }

    public partial class EventList
    {
        public int typeId;
        public List<Event> events;

        #region getter and setter
        public int TypeId
        {
            get => typeId;
            set => typeId = value;
        }

        public List<Event> Events
        {
            get => events;
            set => events = value;
        }
        #endregion

        [JsonConstructor]
        public EventList(int typeId, List<Event> events)
        {
            this.typeId = typeId;
            this.events = events;
            this.type = (EventType)Enum.Parse(typeof(EventType), typeId.ToString());
        }
    }

    public partial class Event
    {
        public double startTime;    //开始时间
        public double endTime;      //终止时间
        public int typeId;       //事件类型
        public double startValue;   //开始值
        public double endValue;     //结束值
        public string easeFunctionID; //缓动函数
        public Dictionary<string, object> properties;   //属性

        #region getter and setter
        public double StartTime
        {
            get => startTime;
            set => startTime = value;
        }

        public double EndTime
        {
            get => endTime;
            set => endTime = value;
        }

        public int TypeId
        {
            get => typeId;
            set => typeId = value;
        }

        public string EaseFunctionID
        {
            get => easeFunctionID;
            set => easeFunctionID = value;
        }

        public Dictionary<string, object> Properties
        {
            get => properties;
            set => properties = value;
        }

        public double StartValue
        {
            get => startValue;
            set => startValue = value;
        }

        public double EndValue
        {
            get => endValue;
            set => endValue = value;
        }

        #endregion

        [JsonConstructor]
        public Event(double startTime, double endTime, int typeId, string easeFunctionID, Dictionary<string, object> properties, double startValue, double endValue)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.typeId = typeId;
            this.properties = properties;
            this.easeFunctionID = easeFunctionID;
            this.startValue = startValue;
            this.endValue = endValue;

            this.easeFunction = EaseFunctions.functions[easeFunctionID];
            this.type = (EventType)Enum.Parse(typeof(EventType), typeId.ToString());

            rectangle = new(this)
            {
                Fill = new SolidColorBrush(EditorColors.GetEventColor(type)),
                HighLightBorderBrush = new SolidColorBrush(EditorColors.GetEventHighlightColor(type))
            };
            this.properties = properties;
        }
    }

    public partial class Function
    {
        public double time;    //开始时间
        public double rail;     //轨道
        public string functionName; //函数名

        #region getter and setter
        public double Time { get => time; set => time = value; }
        public double Rail { get => rail; set => rail = value; }
        public string FunctionName { get => functionName; set => functionName = value; }
        #endregion

        [JsonConstructor]
        public Function(double time, double rail, string functionName)
        {
            this.time = time;
            this.rail = rail;
            this.functionName = functionName;

            rectangle = new(this)
            {
                Fill = new SolidColorBrush(EditorColors.functionColor),
                HighLightBorderBrush = new SolidColorBrush(EditorColors.functionHighlightColor)
            };
        }
    }
}
