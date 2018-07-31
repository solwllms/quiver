using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace engine.system
{
    public class Filesystem
    {
        private static readonly List<string> Directories = new List<string>();

        public static void AddDirectory(string path)
        {
            Directories.Add(path);
        }

        public static string GetBaseDirectory()
        {
            return Directories[0];
        }

        public static string GetPath(string filename, bool create = false)
        {
            foreach (var dir in Directories)
            {
                var path = dir + "/" + filename;
                if (File.Exists(path)) return Path.GetFullPath(path);
            }

            if (create)
            {
                Log.WriteLine("trying to create..");
                try
                {
                    Directory.CreateDirectory(
                        Path.GetDirectoryName(Path.GetFullPath(Cmd.GetValue("dir") + "/" + filename)));
                }
                catch
                {
                }

                var s = File.Create(filename);
                s.Close();
                s.Dispose();
                Log.WriteLine("created file " + Path.GetFullPath(filename));
                return Path.GetFullPath(filename);
            }

            return null;
        }

        public static bool TryPath(string filename, out string path)
        {
            foreach (var dir in Directories)
            {
                var tpath = dir + "/" + filename;
                if (File.Exists(tpath))
                {
                    path = Path.GetFullPath(tpath);
                    return true;
                }
            }

            path = null;
            return false;
        }

        public static Stream Open(string filename, bool create = false, bool tex = false)
        {
            string path;
            if (TryPath(filename, out path) || create)
                return File.Open(GetPath(filename, create), create ? FileMode.OpenOrCreate : FileMode.Open,
                    FileAccess.ReadWrite, FileShare.ReadWrite);

            if (tex)
            {
                var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("engine.resources.error.png");
                Log.WriteLine("failed to find file: \"" + filename + "\"", Log.MessageType.Error);
                return s;
            }
            else return null;
        }
    }
}