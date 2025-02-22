using System;
using System.Collections;
using System.Collections.Generic;
using PMEditor.Util;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace PMEditor
{
    public partial class Track
    {
        public DatapackGenerator Datapack;
        public DirectoryInfo Target;

        [JsonIgnore]
        public int Count
        {
            get
            {
                return AllLines.SelectMany(line => line.Notes).Sum(note => note.GetCount());
            }
        }
        
        [JsonIgnore]
        public List<Line> AllLines => new (lines) { freeLine };

        /// <summary>
        /// 每一拍开始的时间
        /// </summary>
        [JsonIgnore]
        public List<double> LineTimes = new();

        public (double startTime, double length) GetTimeRange(int measure)
        {
            if (measure < 0) throw new ArgumentException();
            if(measure >= LineTimes.Count - 1) return (
                LineTimes.Last() + (measure - LineTimes.Count + 1) * baseBpm / 60,
                baseBpm / 60
                );
            return (
                LineTimes[measure],
                LineTimes[measure + 1] - LineTimes[measure]
            );
        }

        public (int measure, double deltime) GetMeasureFromTime(double time)
        {
            if (time < 0) throw new ArgumentException();
            for (var i = 0;; i++)
            {
                var (startTime, l) = GetTimeRange(i);
                if (startTime <= time && time < startTime + l)
                {
                    return (i, time - startTime);
                }
            }
        }

        // ReSharper disable once InconsistentNaming
        public double GetBPM(int measure)
        {
            if (bpmInfo.Count == 0) return baseBpm;
            if (measure >= bpmInfo.Last().EndMeasure) return baseBpm;
            if (measure < bpmInfo.First().StartMeasure) return baseBpm;
            foreach (var info in bpmInfo)
            {
                if (info.StartMeasure <= measure && measure < info.EndMeasure)
                {
                    return info.Value;
                }
            }

            return baseBpm;
        }
        
        public void Init()
        {
            Datapack.Create(false);
        }

        public void Build()
        {
            if (!Target.Exists)
            {
                Target.Create();
            }
            var nbt = NBTTrack.FromTrack(this);
            nbt.ToFrameFunctions(Target);
        }

        public void UpdateLineTimes()
        {
            double time = 0;
            LineTimes.Clear();
            LineTimes.Add(0);
            for (var i = 0;; i++)
            {
                var bpm = GetBPM(i);
                time += 60 / bpm;
                LineTimes.Add(time);
                if(time > length) return;
            }
        }

        public static Track GetTrack(FileInfo trackFile)
        {
            var all = trackFile.OpenText().ReadToEnd();
            var track = JsonSerializer.Deserialize<Track>(all)!;
            foreach (var line in track.lines)
            {
                line.Init(track);
            }
            track.freeLine.Init(track);
            track.Init();
            return track;
        }

        public string ToJsonString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
