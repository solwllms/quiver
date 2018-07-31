using System.Collections.Generic;
using engine.display;
using SFML.Audio;

namespace engine.system
{
    public class Cache
    {
        /*
         * TEXTURE CACHING SYSTEM
         * we use the same system but simplified as otherwise
         * it would limit the use case of this system.
         * (it uses more memory but who cares?)
         */
        private static readonly Dictionary<string, Texture> Texturecache = new Dictionary<string, Texture>();

        /*
           SOUND CACHING SYSTEM
           we dont cache music, as this would be hella lot of memory,
           and defeat the point of streaming, but we do do it for sounds,
           as these can be really common!

            (we also unload any that are level-only)
         */
        private static readonly Dictionary<string, SoundBuffer> Soundcache = new Dictionary<string, SoundBuffer>();

        public static Texture GetTexture(string name, bool cache = true)
        {
            if (Texturecache.ContainsKey(name) && cache) return Texturecache[name];

            var tex = new Texture(name);
            if (cache) Texturecache.Add(name, tex);
            return tex;
        }

        public static void ClearAll()
        {
            Texturecache.Clear();
            ClearSounds();
        }

        public static SoundBuffer GetSound(string name, bool cache = true)
        {
            if (Soundcache.ContainsKey(name)) return Soundcache[name];

            var s = Filesystem.Open(name + ".wav");
            try
            {
                var buffer = new SoundBuffer(s);
                if (cache)
                    Soundcache.Add(name, buffer);
                return buffer;
            }
            catch
            {
                Log.WriteLine("failed to load sound file: \"" + name + "\"", Log.MessageType.Error);
                return null;
            }
        }

        public static void ClearSounds()
        {
            // to stop cache from getting super long
            Soundcache.Clear();
        }
    }
}