using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PMEditor
{
    /// <summary>
    /// 一个类，用于存储谱面信息
    /// </summary>
    public class Track
    {
        public string trackName;
        public string musicAuthor;
        public string trackAuthor;
        public double bpm;
        public double length;
        public string difficulty;
        public List<Line> lines;

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
        public double y;
        public List<Note> notes;

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

    public class Note
    {
        public int rail;
        public int noteType;
        public int fallType;
        public bool isFake;
        public int time;
        public int generTime;
        public List<double> positions;

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

        public int GenerTime
        {
            get => generTime; set => generTime = value;
        }

        public List<double> Positions
        {
            get => positions; set => positions = value;
        }
        #endregion

        public Note(int rail, int noteType, int fallType, bool isFake, int time, int generTime)
        {
            this.rail = rail;
            this.noteType = noteType;
            this.fallType = fallType;
            this.isFake = isFake;
            this.time = time;
            this.generTime = generTime;
            this.positions = new();
        }
    }
}
