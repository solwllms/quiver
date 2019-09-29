﻿using System;
using OpenTK.Audio.OpenAL;
using System.Collections.Generic;
using Quiver.system;
using OpenTK.Audio;

namespace Quiver.Audio
{
    public class audio
    {
        public static cvar cvarAudio = new cvar("audio", "1", true, true, callback: delegate
        {
            if (cvarAudio.Valueb()) Init();
            else Unload();
        });
        public static cvar cvarVolume = new cvar("volume", "100", true, callback: delegate
        {
            SetVolume(cvarVolume.Valuef());
        });
        public static cvar cvarMusVolume = new cvar("musicvol", "20", true, callback: delegate
        {
            musicPlayer.SetVolume(cvarMusVolume.Valuef());
        });

        private static AudioContext context;
        private static AudioPlayer musicPlayer;
        private static List<AudioPlayer> nowPlaying;

        internal static void Init()
        {
            log.WriteLine("initializing audio...");
            context = new AudioContext();
            musicPlayer = new AudioPlayer();
            nowPlaying = new List<AudioPlayer>();


            var version = AL.Get(ALGetString.Version);
            var vendor = AL.Get(ALGetString.Vendor);
            var renderer = AL.Get(ALGetString.Renderer);
            log.WriteLine("loaded audio successfully. (" + GetDevice() + ": "+ version + ", " + vendor + ", " + renderer +")", log.LogMessageType.Good);
        }

        internal static void Tick()
        {
            PollPlayer(musicPlayer);
            foreach (AudioPlayer player in nowPlaying) PollPlayer(player);
        }

        private static void PollPlayer(AudioPlayer player)
        {
            int state;
            AL.GetSource(player.source, ALGetSourcei.SourceState, out state);

            if ((ALSourceState)state != ALSourceState.Playing)
                player.Stop();
        }

        internal static void Unload()
        {
            log.WriteLine("unloading audio...");
            ClearSounds();
            context.Dispose();
        }

        internal static void UpdateListener(vector position, float angle)
        {
            SetListenerPosition(position);
            SetListenerOrientation(angle);
        }

        /*

         MUSIC PLAYER

         used for music tracks and soundtrack exclusively. allows for one hefty file at a time.

         */

        public static void PlayTrack(string file, float volume = 100, bool looping = false)
        {
            SoundFile sound = cache.GetSound(file, false);
            if (sound == null || !sound.Ready()) return;

            musicPlayer.Load(sound);

            musicPlayer.SetVolume(volume);
            musicPlayer.SetLooping(looping);

            musicPlayer.Play();
        }


        public static void StopTrack()
        {
            musicPlayer.Stop();
        }

        public static string GetTrack()
        {
            return musicPlayer.filename;
        }

        /*

         SFX PLAYER(S)

         used for unlimited sound effects to be played at once, and in 3d if needed.

         */

        public static void PlaySound(string file, float volume = 100, bool looping = false)
        {
            SoundFile sound = cache.GetSound(file);
            if (sound == null || !sound.Ready()) return;

            AudioPlayer player = new AudioPlayer();
            player.Load(sound);

            player.SetVolume(volume);
            player.SetLooping(looping);

            player.Play();
            nowPlaying.Add(player);
        }
        public static void PlaySound3D(string file, vector pos, float volume = 100, bool looping = false)
        {
            SoundFile sound = cache.GetSound(file);
            if (sound == null || !sound.Ready()) return;

            AudioPlayer player = new AudioPlayer();
            player.Load(sound);

            player.SetVolume(volume);
            player.SetPosition(pos);
            player.SetLooping(looping);

            player.Play();
            nowPlaying.Add(player);
        }

        public static void ClearSounds()
        {
            foreach (AudioPlayer player in nowPlaying)
                player.Stop();

            musicPlayer.Stop();
            nowPlaying.Clear();
        }

        public static void SetVolume(float volume)
        {
            AL.Listener(ALListenerf.Gain, volume / 100);
        }
        public static void SetListenerPosition(vector position)
        {
            AL.Listener(ALListener3f.Position, position.x, 0, position.y);
        }
        public static void SetListenerOrientation(float angle)
        {
            AL.Listener(ALListener3f.Position, 0, 0, -angle);
        }

        public static string GetDevice()
        {
            return Alc.GetString(Alc.OpenDevice(null), AlcGetString.DeviceSpecifier);
        }
    }

    public class AudioPlayer
    {
        public SoundFile file;
        public string filename => file?.filename;

        public int buffer;
        public int source;

        public AudioPlayer()
        {
            buffer = AL.GenBuffer();
            source = AL.GenSource();
        }

        public void Load(SoundFile sound)
        {
            file = sound;
            AL.Source(source, ALSourceb.SourceRelative, true);
            AL.BufferData(buffer, file.SoundFormat, file.data, file.data.Length, file.sampleRate);
            AL.Source(source, ALSourcei.Buffer, buffer);
        }

        public void SetLooping(bool loop)
        {
            AL.Source(source, ALSourceb.Looping, loop);
        }
        public void SetVolume(float volume)
        {
            AL.Source(source, ALSourcef.Gain, volume / 100);
        }
        public void SetPosition(vector position)
        {
            AL.Source(source, ALSourceb.SourceRelative, false);
            AL.Source(source, ALSource3f.Position, position.x, 0, position.y);
        }
        public void SetVelocity(vector velocity)
        {
            AL.Source(source, ALSource3f.Velocity, velocity.x, 0, velocity.y);
        }

        public void Play()
        {
            AL.SourcePlay(source);
        }
        public void Pause()
        {
            AL.SourcePause(source);
        }
        public void Stop()
        {
            AL.SourceStop(source);
            AL.DeleteSource(source);
            AL.DeleteBuffer(buffer);
        }
    }
}