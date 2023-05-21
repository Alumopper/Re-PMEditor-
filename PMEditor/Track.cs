using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        public List<Line> lines;    //判定线

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

        public List<Line> Lines
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
            Track? track = JsonSerializer.Deserialize<Track>(all);
            return track;
        }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        }
    }

    public class Line
    {
        public double y;        //判定线的y坐标
        public List<Note> notes;//note

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
        #endregion

        public Line(double y)
        {
            this.y = y;
            this.notes = new();
        }

        public Line() : this(0) { }
    }

    public partial class Note
    {
        public int rail;                //轨道
        public int noteType;            //note类型
        public int fallType;            //掉落类型
        public bool isFake;             //是否是假键
        public int time;                //被判定的时间，单位tick
        public int generTime;           //生成此note的时间，单位tick
        public int holdTime;            //需要按住的时间，仅用于hold，单位tick
        public List<double> positions;  //所有可能的位置

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

        public int Time
        {
            get => time; set => time = value;
        }

        public double ActualTime
        {
            get => actualTime; set => actualTime = value;
        }

        public int GenerTime
        {
            get => generTime; set => generTime = value;
        }

        public int HoldTime
        {
            get => holdTime; set => holdTime = value;
        }

        public double ActualHoldTime
        {
            get => actualHoldTime; set => actualHoldTime = value;
        }

        public List<double> Positions
        {
            get => positions; set => positions = value;
        }
        #endregion

        public Note(int rail, int noteType, int fallType, bool isFake, double actualTime, int generTime, double actualHoldTime = 0)
        {
            this.rail = rail;
            this.noteType = noteType;
            this.fallType = fallType;
            this.isFake = isFake;
            this.actualTime = actualTime;
            this.time = (int)(actualTime * 20);
            this.generTime = generTime;
            this.actualHoldTime = actualHoldTime;
            this.holdTime = (int)(actualHoldTime * 20);
            this.positions = new();
            this.sound.MediaEnded += Sound_MediaEnded;

            this.type = (NoteType)Enum.Parse(typeof(NoteType), noteType.ToString());

            rectangle = new NoteRectangle(this);

            //注册点击事件
            rectangle.MouseRightButtonUp += Rectangle_MouseRightButtonUp;

            if (noteType == (int)PMEditor.NoteType.Tap)
            {
                sound.Open(new Uri("./assets/sounds/tap.wav", UriKind.Relative));
                rectangle.Fill = new SolidColorBrush(
                Color.FromArgb(255, 109, 209, 213));
            }
            else
            {
                sound.Open(new Uri("./assets/sounds/drag.wav", UriKind.Relative));
                rectangle.Fill = new SolidColorBrush(
                Color.FromArgb(255, 227, 214, 76));
            }
        }

    }
}
