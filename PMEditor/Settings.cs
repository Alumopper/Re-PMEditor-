using System.Collections.Generic;

namespace PMEditor
{
    public class Settings
    {
        public static Settings currSetting;

        public List<string> canSelectedSpeedList;

        public Settings(
            List<string> canSelectedSpeedList = null
            )
        {
            this.canSelectedSpeedList = canSelectedSpeedList ??= new List<string>() { "1.0", "0.25", "0.5", "0.75", "1.0", "1.25", "1.5", "1.75", "2.0" };
        }
    }
}
