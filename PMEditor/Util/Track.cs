using System.Collections.Generic;
using PMEditor.Util;
using System.IO;
using System.Text.Json;

namespace PMEditor
{
    public partial class Track
    {
        public DatapackGenerator datapack;
        public DirectoryInfo target;

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var line in AllLines)
                {
                    foreach (var note in line.Notes)
                    {
                        count += note.GetCount();
                    }
                }
                return count;
            }
        }
        
        public List<Line> AllLines => new (lines) { freeLine };

        public void Init()
        {
            datapack.Create(false);
        }

        public void Build()
        {
            if (!target.Exists)
            {
                target.Create();
            }
            var nbt = NBTTrack.FromTrack(this);
            nbt.ToFrameFunctions(target);
        }

        public static Track? GetTrack(FileInfo trackFile)
        {
            string all = trackFile.OpenText().ReadToEnd();
            Track track = JsonSerializer.Deserialize<Track>(all)!;
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
