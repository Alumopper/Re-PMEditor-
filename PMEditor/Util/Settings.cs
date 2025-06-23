using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PMEditor.Util
{
    public class Settings
    {
        public static Settings currSetting;

        public List<string> canSelectedSpeedList { get; set; }

        public double Tick { get; set; } = 20;

        public double MapLength { get; set; } = 32;

        public bool WarnEventTypeChange { get; set; } = true;
        
        public string ExportPath { get; set; } = @".\out\";

        public Settings(List<string>? canSelectedSpeedList = null)
        {
            this.canSelectedSpeedList = canSelectedSpeedList ?? new List<string> { "1.0", "0.25", "0.5", "0.75", "1.0", "1.25", "1.5", "1.75", "2.0" };
        }

        [JsonConstructor]
        public Settings(List<string> canSelectedSpeedList, double Tick, double MapLength, bool WarnEventTypeChange)
        {
            this.canSelectedSpeedList = canSelectedSpeedList;
            this.Tick = Tick;
            this.MapLength = MapLength;
            this.WarnEventTypeChange = WarnEventTypeChange;
        }
    }
}
