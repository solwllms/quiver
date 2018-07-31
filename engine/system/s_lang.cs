using System.Collections.Generic;
using System.IO;

namespace engine.system
{
    public class Lang
    {
        public static string[] langs = new[] { "english", "español", "français", "deutsch" };
        public static string[] langfiles = new[] { "lang/english.txt", "lang/spanish.txt", "lang/french.txt", "lang/german.txt" };

        private static Dictionary<string, string> _dict;

        public static void LoadLang(string file)
        {
            Log.WriteLine("loading language file "+file+"..");
            _dict = new Dictionary<string, string>();
            using (var read = new StreamReader(Filesystem.Open(file)))
            {
                while (!read.EndOfStream)
                {
                    string[] p = read.ReadLine().Split('=');
                    if (p[0] == "" || p[0][0] == '\'') continue;
                    _dict.Add(p[0], p[1]);
                }
            }
        }

        public static string Get(string phrase)
        {
            if (!_dict.ContainsKey(phrase)) return phrase;
            return _dict[phrase];
        }
    }
}
