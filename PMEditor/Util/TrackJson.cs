using PMEditor.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;
using NCalc;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global

// ReSharper disable once CheckNamespace        
#pragma warning disable CS8618
namespace PMEditor
{
    /// <summary>
    /// 一个类，用于存储谱面信息
    /// </summary>
    public partial class Track
    {
        protected string trackName;    //谱面名字
        protected string musicAuthor;  //曲师
        protected string trackAuthor;  //谱师
        protected double baseBpm;          //bpm
        protected double length;       //曲目长度
        protected string difficulty;   //谱面难度
        protected ObservableCollection<Line> lines;    //判定线
        protected Line freeLine = new();   //自由判定线
        protected ObservableCollection<BpmInfo> bpmInfo;
        protected string exportPath;

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

        public double BaseBpm
        {
            get { return baseBpm; }
            set { baseBpm = value; }
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

        public Line FreeLine
        {
            get { return freeLine; }
            set { freeLine = value; }
        }
        
        public ObservableCollection<BpmInfo> BpmInfo
        {
            get { return bpmInfo; }
            set { bpmInfo = value; }
        }
        
        public string ExportPath
        {
            get { return exportPath; }
            set { exportPath = value; }
        }
        #endregion

        public Track(string trackName, string musicAuthor, string trackAuthor, double baseBpm, double length, string difficulty)
        {
            this.trackName = trackName;
            this.musicAuthor = musicAuthor;
            this.trackAuthor = trackAuthor;
            this.baseBpm = baseBpm;
            this.length = length;
            this.difficulty = difficulty;
            this.lines = new ObservableCollection<Line>();
            lines.Add(new Line());
            this.bpmInfo = new ObservableCollection<BpmInfo>();
            this.exportPath = "./tracks/" + trackName + "/out/" + trackName;

            Target = new DirectoryInfo(ExportPath);
            Datapack = new DatapackGenerator(Target, trackName);
        }

    }

    public class BpmInfo
    {
        protected double value;
        protected int startMeasure;
        protected int endMeasure;
        
        #region getter and setter
        public double Value
        {
            get { return value; }
            set { this.value = value; }
        }
        
        public int StartMeasure
        {
            get { return startMeasure; }
            set { startMeasure = value; }
        }
        
        public int EndMeasure
        {
            get { return endMeasure; }
            set { endMeasure = value; }
        }
        
        #endregion

        public BpmInfo(double value, int startMeasure, int endMeasure)
        {
            this.value = value;
            this.startMeasure = startMeasure;
            this.endMeasure = endMeasure;
        }

        public BpmInfo Clone()
        {
            return new BpmInfo(value, startMeasure, endMeasure);
        }
        
    }

    public partial class Line
    {
        protected string id;           //判定线的名字
        protected double y;            //判定线的y坐标
        protected List<Note> notes;    //note
        protected List<FakeCatch> fakeCatch;
        protected List<EventList> eventLists;  //事件
        protected List<Function> functions;

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

        [JsonConstructor]
        public Line(double y, string id, List<Note> notes, List<FakeCatch> fakeCatch, List<EventList> eventLists, List<Function> functions)
        {
            this.y = y;
            this.id = id;
            this.notes = notes;
            this.fakeCatch = fakeCatch;
            this.eventLists = eventLists;
            this.functions = functions;
        }
        
        public Line(double y, string id = "newLine")
        {
            this.y = y;
            this.id = id;
            this.notes = new List<Note>();
            InitEventList();
            this.fakeCatch = new List<FakeCatch>();
            this.functions = new List<Function>();
        }

        public Line() : this(0) { }
    }

    public partial class Note
    {
        protected int rail;                //轨道
        protected int noteType;            //note类型
        protected int fallType;            //掉落类型
        protected bool isFake;             //是否是假键

        protected double actualTime;       //准确的判定时间
        protected double actualHoldTime;   //准确的按住时间

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
        
        public string? ExpressionString
        {
            get => Expression?.ExpressionString; set => Expression = value is null or "null" ? null : new Expression(value);
        }

        #endregion

        [JsonConstructor]
        public Note(int rail, int noteType, int fallType, bool isFake, double actualTime, double actualHoldTime = 0,
            string? expressionString = null)
        {
            this.rail = rail;
            this.noteType = noteType;
            this.fallType = fallType;
            this.isFake = isFake;
            this.actualTime = actualTime;
            this.actualHoldTime = actualHoldTime;

            this.type = (NoteType)Enum.Parse(typeof(NoteType), noteType.ToString());
            
            Expression = expressionString == null ? null : new Expression(expressionString);
        }
    }

    public partial class EventList
    {
        protected int typeId;
        protected List<Event> events;

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
            this.Type = (EventType)Enum.Parse(typeof(EventType), typeId.ToString());
        }
    }

    public partial class Event
    {
        protected double startTime;    //开始时间
        protected double endTime;      //终止时间
        protected int typeId;       //事件类型
        protected double startValue;   //开始值
        protected double endValue;     //结束值
        protected string easeFunctionID; //缓动函数
        protected Dictionary<string, object> properties;   //属性

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

            this.EaseFunction = EaseFunctions.Functions[easeFunctionID];
            this.Type = (EventType)Enum.Parse(typeof(EventType), typeId.ToString());

            this.properties = properties;
        }
    }

    public partial class Function
    {
        protected double time;    //开始时间
        protected int rail;     //轨道
        protected string functionName; //函数名

        #region getter and setter
        public double Time { get => time; set => time = value; }
        public int Rail { get => rail; set => rail = value; }
        public string FunctionName { get => functionName; set => functionName = value; }
        #endregion

        [JsonConstructor]
        public Function(double time, int rail, string functionName)
        {
            this.time = time;
            this.rail = rail;
            this.functionName = functionName;
        }
    }
}
