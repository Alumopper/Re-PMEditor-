using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace PMEditor.Util
{
    public class SoundPool
    {
        readonly List<Sound> sounds = new ();

        readonly string path;

        public SoundPool(string path)
        {
            this.path = path;
            for (int i = 0; i < 8; i++)
            {
                sounds.Add(new Sound(path));
            }
        }

        public void Play(int times = 1)
        {
            foreach(var sound in sounds)
            {
                if (!sound.isPlaying)
                {
                    sound.Play(times);
                    return;
                }
            }
            //没有空闲的声音，新建一个
            sounds.Add(new Sound(path));
            sounds[^1].Play();
        }
    }

    class Sound
    {
        public MediaPlayer player;
        public bool isPlaying = false;

        public Sound(string path)
        {
            player = new MediaPlayer();
            player.Open(new Uri(path, UriKind.Relative));
            player.MediaEnded += (s, e) => {
                player.Pause();
                (s as MediaPlayer)!.Position = TimeSpan.Zero;
                isPlaying = false;
            };
            player.Volume = 0;
            player.Play();
            player.Pause();
            player.Volume = 0.5;
        }

        public Sound(MediaPlayer player)
        {
            this.player = player;
            player.MediaEnded += (s, e) => isPlaying = false;
        }

        public void Play(int times = 1)
        {
            if (times == 0) return;
            if (!isPlaying)
            {
                player.Volume = 0.4 + times * 0.1;
                player.Position = TimeSpan.Zero;
                player.Play();
                isPlaying = true;
            }
        }

        public void Stop()
        {
            player.Stop();
            isPlaying = false;
        }
    }

}
