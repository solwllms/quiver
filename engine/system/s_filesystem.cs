#region

using Quiver.system;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;

#endregion

namespace Quiver
{
    public class filesystem
    {
        private static readonly List<string> Directories = new List<string>();
        private static readonly List<ZipArchive> Archives = new List<ZipArchive>();
        private static readonly List<string> ArchivePaths = new List<string>();

        /// <summary>
        /// Adds an archive to the cache by path.
        /// </summary>
        public static void AddDirectory(string path)
        {
            Directories.Add(path);
        }

        /// <summary>
        /// Adds a ZIP format archive to the cache.
        /// </summary>
        /// <param name="path">Archive directory.</param>
        public static void AddArchive(string path)
        {
            ArchivePaths.Add(path);
            Archives.Add(new ZipArchive(File.Open(path, FileMode.Open)));
        }

        /// <summary>
        /// Lists all directories and zips (and zip entries) loaded
        /// into the game virtual file system into the console. Search parameter
        /// term is used.
        /// </summary>
        /// <param name="param">[0] Search term</param>
        /// <returns></returns>
        internal static bool PrintPaks(string[] param)
        {
            string search = param.Length > 0 ? param[0] : "";

            log.WriteLine("indexed directories:");
            foreach (var dir in Directories)
            {
                if (dir.StartsWith(search)) log.WriteLine("- " + dir);
            }

            log.WriteLine("indexed paks:");
            for (int i = 0; i < ArchivePaths.Count; i++)
            {
                if (ArchivePaths[i].StartsWith(search)) log.WriteLine("- " + ArchivePaths[i]);

                foreach (var entry in Archives[i].Entries)
                    log.WriteLine("--- " + entry.FullName);
            }

            return false;
        }

        /// <summary>
        /// Gets the system path of the default game file system directory, typically the executable directory. (eg C:/evoke Software/)
        /// </summary>
        /// <returns></returns>
        public static string GetBaseDirectory()
        {
            return Directories[0];
        }

        /// <summary>
        /// Get paths of all files matching a search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern to use. Wildcard asterisks such as '*.PAK' are both permitted and recommended.</param>
        /// <returns>A list of paths to files matching search pattern.</returns>
        public static string[] GetAllFiles(string searchPattern)
        {
            var s = new List<string>();
            foreach (var dir in Directories)
            {
                if (!Directory.Exists(dir)) continue;

                var fileEntries = Directory.GetFiles(dir, searchPattern);
                foreach (var fileName in fileEntries)
                    s.Add(fileName);
            }

            return s.ToArray();
        }

        /// <summary>
        /// Gets the path of a file within the game's file system. This doesn't include archives.
        /// </summary>
        /// <param name="filename">File (including path) to search for.</param>
        /// <param name="create">Should the file be created, if it doesn't exist?</param>
        /// <param name="overwrite">Deletes the file if present.</param>
        /// <returns>The complete operating system file path.</returns>
        public static string GetPath(string filename, bool create = false, bool overwrite = false)
        {
            if (!overwrite)
            {
                foreach (var dir in Directories)
                {
                    var path = dir + "/" + filename;
                    if (File.Exists(path)) return Path.GetFullPath(path);
                }
            }
            else
            {
                if (File.Exists(filename)) File.Delete(filename);
            }

            if (create)
            {
                log.WriteLine("trying to create..");
                filename = Directories[0] + filename;

                var p = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(p) && !Directory.Exists(p))
                    Directory.CreateDirectory(p);

                var s = File.Create(filename);
                s.Close();
                log.WriteLine("created file " + Path.GetFullPath(filename));
                return Path.GetFullPath(filename);
            }

            return null;
        }

        /// <summary>
        /// Does the file exist within the game file system.
        /// </summary>
        /// <param name="filename">File (including path) to search for.</param>
        /// <param name="checkZips">Should include archives in search?</param>
        /// <returns>Does the file exist?</returns>
        public static bool Exists(string filename, bool checkZips = true)
        {
            foreach (var dir in Directories)
            {
                var path = dir + "/" + filename;
                if (File.Exists(path)) return true;
            }

            if (!checkZips) return false;

            foreach (var zip in Archives)
            foreach (var entry in zip.Entries)
                if (entry.FullName.ToUpper() == filename.ToUpper())
                    return true;

            return false;
        }

        /// <summary>
        /// Gets the path of a file within the game's file system. This doesn't include archives.
        /// </summary>
        /// <param name="filename">File (including path) to search for.</param>
        /// <param name="path">The complete operating system file path.</param>
        /// <returns>Does the file exist?</returns>
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

        /// <summary>
        /// Opens file by name for read/write.
        /// </summary>
        /// <param name="filename">File name (including path)</param>
        /// <param name="create">Should create if not present?</param>
        /// <param name="tex">Should return a error texture if not present?</param>
        /// <param name="overwrite">Should delete if present?</param>
        /// <returns>An open stream.</returns>
        public static Stream Open(string filename, bool create = false, bool tex = false, bool overwrite = false)
        {
            if (Exists(filename))
            {
                foreach (var dir in Directories)
                {
                    var path = dir + "/" + filename;
                    if (File.Exists(path))
                        return File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                }

                foreach (var zip in Archives)
                foreach (var entry in zip.Entries)
                    if (entry.FullName.ToUpper() == filename.ToUpper())
                        return entry.Open();
            }
            else if (create)
            {
                return File.Open(GetPath(filename, true, overwrite), FileMode.OpenOrCreate, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
            }

            // return defaults
            if (tex)
            {
                var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("engine.Resources.error.png");
                log.WriteLine("failed to find file: \"" + filename + "\"", log.LogMessageType.Error);
                return s;
            }

            return null;
        }
    }
}