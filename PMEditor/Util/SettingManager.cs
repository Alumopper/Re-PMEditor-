using System;
using System.Text.Json;

namespace PMEditor.Util
{
    public class SettingManager
    {
        public static readonly string SettingPath = "setting.json";

        public static Settings Read(string SettingPath)
        {
            Settings? settings = null;
            try
            {
                settings = JsonSerializer.Deserialize<Settings>(System.IO.File.ReadAllText(SettingPath));
            }
            catch(Exception e)
            {
                settings = new Settings();
                Write(settings);
#if DEBUG
                DebugWindow.Log("Error reading settings, using default settings");
                DebugWindow.Log(e.Message);
#endif
            }
            return settings ?? new ();
        }

        public static void Write(Settings settings)
        {
            System.IO.File.WriteAllText(SettingPath, JsonSerializer.Serialize(settings));
        }
    }
}
