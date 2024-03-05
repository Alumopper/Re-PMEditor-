using System.IO;

namespace PMEditor.Util
{
    public class TrackInfo
    {
        public string trackName;
        public string TrackName
        {
            get { return trackName; }
            set { trackName = value; }
        }

        public string musicAuthor;
        public string MusicAuthor
        {
            get { return musicAuthor; }
            set { musicAuthor = value; }
        }

        public string trackAuthor;
        public string TrackAuthor
        {
            get { return trackAuthor; }
            set { trackAuthor = value; }
        }

        public TrackInfo(string trackName, string musicAuthor, string trackAuthor)
        {
            this.trackName = trackName;
            this.musicAuthor = musicAuthor;
            this.trackAuthor = trackAuthor;
        }

        public override string ToString()
        {
            return trackName + "\n" + musicAuthor + "\n" + trackAuthor;
        }

        public static TrackInfo? FromFile(string path)
        {
            using StreamReader reader = new StreamReader(path);
            string? trackName = reader.ReadLine();
            string? musicAuthor = reader.ReadLine();
            string? trackAuthor = reader.ReadLine();
            if (trackName != null && musicAuthor != null && trackAuthor != null)
            {
                return new TrackInfo(trackName, musicAuthor, trackAuthor);
            }
            else return null;
        }
    }
}
