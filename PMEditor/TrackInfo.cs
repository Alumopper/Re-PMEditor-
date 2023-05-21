namespace PMEditor
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
    }
}
