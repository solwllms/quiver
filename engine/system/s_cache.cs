#region

using System.Collections.Generic;
using Quiver.Audio;
using Quiver.display;

#endregion

namespace Quiver.system
{
    public class cache
    {
        /*
         * TEXTURE CACHING SYSTEM
         * we use the same system but simplified as otherwise
         * it would limit the use case of this system.
         * (it uses more memory but who cares?)
         */
        private static readonly Dictionary<string, texture> Texturecache = new Dictionary<string, texture>();

        /*
           SOUND CACHING SYSTEM
           we dont cache music, as this would be hella lot of memory,
           and defeat the point of streaming, but we do do it for sounds,
           as these can be really common!

            (we also unload any that are level-only)
         */
        private static readonly Dictionary<string, SoundFile> Soundcache = new Dictionary<string, SoundFile>();

        public static texture GetTexture(string name, bool cache = true)
        {
            if (Texturecache.ContainsKey(name) && cache) return Texturecache[name];

            var tex = new texture(name);
            if (cache) Texturecache.Add(name, tex);
            return tex;
        }

        public static void ClearAll()
        {
            Texturecache.Clear();

            //ClearSounds();
        }

        
        public static SoundFile GetSound(string name, bool cache = true)
        {
            if (Soundcache.ContainsKey(name)) return Soundcache[name];

            //try
            //{
                var buffer = new SoundFile(name);
                if (cache)
                    Soundcache.Add(name, buffer);
                return buffer;
                /*
            }
            catch 
            {
                log.WriteLine("failed to load sound file: \"" + name + "\"", log.messageType.Error);
                return null;
            }*/
        }


        // un comment: fix usage above and in game.cs
        public static void ClearSounds()
        {
            // to stop cache from getting super long
            Soundcache.Clear();
        }
    }
}