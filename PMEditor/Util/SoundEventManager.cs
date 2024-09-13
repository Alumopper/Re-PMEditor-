namespace PMEditor.Util
{
    public class SoundEventManager
    {
        public SoundPool soundPool;

        int time = 0;

        public SoundEventManager(SoundPool soundPool)
        {
            this.soundPool = soundPool;
        }

        public void AppendSoundRequest()
        {
            time++;
        }

        public void PlaySound()
        {
            if (time == 0) return;
            soundPool.Play(time);
            time = 0;
        }
    }
}
