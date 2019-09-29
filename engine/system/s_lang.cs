#region

using Quiver.system;
using System.Collections.Generic;
using System.IO;

#endregion

namespace Quiver
{
    public class lang
    {
        public static string[] langs = {"english", "español", "français", "deutsch"};

        internal static string[] langfiles =
            {"lang/english.txt", "lang/spanish.txt", "lang/french.txt", "lang/german.txt"};

        private static Dictionary<string, string> _dict;

        internal static void LoadLang(string file)
        {
            if (!filesystem.Exists(file))
            {
                log.WriteLine("failed to load language file!", log.LogMessageType.Error);
                return;
            }
            
            _dict = new Dictionary<string, string>();
            using (var read = new StreamReader(filesystem.Open(file)))
            {
                while (!read.EndOfStream)
                {
                    var p = read.ReadLine().Split('=');
                    if (p[0] == "" || p[0][0] == '\'') continue;
                    _dict.Add(p[0], p[1]);
                }
            }
        }

        /// <summary>
        /// Fetches localized string identified by key. If no translation is found,
        /// the key is returned.
        /// </summary>
        /// <param name="key">String identifier</param>
        /// <returns>Localized string</returns>
        public static string Get(string key)
        {
            if (_dict == null || !_dict.ContainsKey(key)) return key;
            return _dict[key];
        }
    }
}