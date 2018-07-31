using System;
using System.Collections.Generic;
using System.IO;
using SFML.Audio;
using SFML.System;

namespace engine.system
{
    public class Audio
    {
        public static int volume = 100;

        // used for one-shot (or simply shorter) sounds
        private static List<Snddata> _sounds;

        // used for longer tracks as it streams in over time!
        private static int _mVolume;
        private static string _mTrack;
        private static bool _mLoop;
        private static Music _mPlayer;

        public static int Volume
        {
            get => _mVolume;
            set
            {
                _mVolume = value;
                UpdateVolumes();
            }
        }

        private static bool AudioLoaded { get; set; }

        public static void Init()
        {
            Log.WriteLine("initializing audio..");
            try
            {
                _sounds = new List<Snddata>();
                AudioLoaded = true;

                //m_player = new Music("@");
                if (_mTrack != null) PlayTrack(_mTrack, _mLoop, _mVolume);

                Volume = (int) Cmd.GetValuef("volume");
                Log.WriteLine("audio ok!");
            }
            catch (Exception e)
            {
                Log.WriteLine("failed to init audio. are you missing openal32.dll or libsndfile-1.dll?",
                    Log.MessageType.Error);
                Log.WriteLine(e.Message, Log.MessageType.Error);
            }
        }

        public static void Dispose()
        {
            AudioLoaded = _mPlayer != null;
            if (!AudioLoaded) return;

            Log.WriteLine("unloading audio..");

            _mPlayer.Stop();
            _mPlayer.Dispose();

            foreach (var snd in _sounds.ToArray())
            {
                snd.snd.Stop();
                snd.snd.Dispose();
            }

            _sounds = null;

            AudioLoaded = false;
        }

        public static void UpdateListener(float x, float y, float angle)
        {
            if (!AudioLoaded) return;

            Listener.Position = new Vector3f(x, 0, y);
            Listener.Direction = new Vector3f(0, 0, -angle);

            foreach (var snd in _sounds.ToArray())
                if (snd.snd.Status != SoundStatus.Playing)
                {
                    snd.snd.Dispose();
                    _sounds.Remove(snd);
                }
        }

        public static void PlaySound2D(string sound, int vol = 100, bool loop = false, bool cache = true)
        {
            if (!AudioLoaded) return;

            try
            {
                var soundplayer = new Sound();
                soundplayer.RelativeToListener = true;
                soundplayer.Position = new Vector3f(0, 0, 0);
                soundplayer.SoundBuffer = global::engine.system.Cache.GetSound(sound, cache);
                soundplayer.Attenuation = 0;
                soundplayer.Volume = (float) Volume / vol * 100;
                soundplayer.Loop = loop;
                soundplayer.Play();
                _sounds.Add(new Snddata { snd = soundplayer, file = sound, playvol = vol });
            }
            catch
            {
            } // sometimes SFML has issues with playing audio that I can't fix!
        }

        public static Sound PlaySound3D(string sound, Vector position, int vol = 100, bool loop = false,
            bool cache = true)
        {
            if (!AudioLoaded) return null;

            try
            {
                var soundplayer = new Sound();
                soundplayer.RelativeToListener = false;
                soundplayer.SoundBuffer = global::engine.system.Cache.GetSound(sound, cache);
                soundplayer.Position = new Vector3f(position.x, 0, position.y);
                soundplayer.Volume = (float) Volume / vol * 100;
                soundplayer.Attenuation = 5;
                soundplayer.Loop = loop;
                soundplayer.Play();
                _sounds.Add(new Snddata {snd = soundplayer, file = sound, playvol = vol});
                return soundplayer;
            }
            catch
            {
                return null;
            } // sometimes SFML has issues with playing audio that I can't fix!
        }

        public static void PlayTrack(string track, bool loop, int vol = 100)
        {
            if (!AudioLoaded) return;

            try
            {
                //FIX2: Using SFML's music class again becuase of memory usage worries but
                //      are using the file path and not stream (no music .zips then!)
                //FIX: Not using SFML's music class because of memory leak and DDL errors!

                var tw = Console.Out;
                Console.SetOut(TextWriter.Null);

                _mPlayer = new Music(Filesystem.GetPath(track + ".wav"));

                _mVolume = vol;
                _mLoop = loop;
                _mTrack = track;

                _mPlayer.Volume = (float) Volume / vol * Cmd.GetValuef("musicvol");
                _mPlayer.Loop = loop;
                _mLoop = loop;
                _mPlayer.Play();

                Console.SetOut(tw);
            }
            catch
            {
                Log.WriteLine("failed to load/play track: \"" + track + "\"", Log.MessageType.Error);
            }
        }

        public static void StopSound(string file)
        {
            foreach (Snddata s in _sounds)
            {
                if (s.file == file)
                    s.snd.Stop();
            }
        }

        public static void StopTrack()
        {
            if (!AudioLoaded || _mPlayer.Status != SoundStatus.Playing) return;

            _mPlayer.Stop();
        }
        public static void ClearSounds()
        {
            if (!AudioLoaded) return;

            foreach (var snd in _sounds.ToArray()) snd.snd.Stop();
        }

        public static void UpdateVolumes()
        {
            if (!AudioLoaded) return;

            if (_mPlayer != null)
                _mPlayer.Volume = (float) Volume / _mVolume * Cmd.GetValuef("musicvol");

            foreach (var snd in _sounds.ToArray()) snd.snd.Volume = (float) Volume / snd.playvol * 100;
        }

        private struct Snddata
        {
            public Sound snd;
            public string file;
            public int playvol;
        }
    }
}