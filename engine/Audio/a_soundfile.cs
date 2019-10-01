using System.IO;
using Quiver.system;
using OpenTK.Audio.OpenAL;
using System;

namespace Quiver.Audio
{
    public class SoundFile
    {
        public readonly string filename;
        public readonly byte[] data;

        public readonly int dataSize;
        public readonly int formatSize;
        public readonly int format;
        public readonly int sampleRate;
        public readonly int channels;
        public readonly int byteRate;
        public readonly int blockAlign;
        public readonly int bitDepth;

        public ALFormat SoundFormat => (channels == 1 ? (bitDepth == 8 ? ALFormat.Mono8 : ALFormat.Mono16) :
                 (bitDepth == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16));

        // based off of a number of OpenTK / OpenAL audio systems
        public SoundFile(string file)
        {
            if (!filesystem.Exists(file + ".wav")) {
                log.WriteLine("audio file '" + file + "' not found.", log.LogMessageType.Error);
                return;
            }
            
            filename = file;
            using (Stream s = filesystem.Open(file + ".wav"))
            {
                using (BinaryReader reader = new BinaryReader(s))
                {
                    try
                    {
                        string signature = new string(reader.ReadChars(4));
                        if (signature != "RIFF")
                        {
                            log.WriteLine("audio file '" + file + "' format unsupported. (RIFF mismatch)", log.LogMessageType.Error);
                            reader.Close();
                            return;
                        }

                        reader.ReadInt32(); // unused

                        string format = new string(reader.ReadChars(4));
                        if (format != "WAVE")
                        {
                            log.WriteLine("audio file '" + file + "' format unsupported. (WAVE mismatch)", log.LogMessageType.Error);
                            reader.Close();
                            return;
                        }

                        string fSig = new string(reader.ReadChars(4));
                        if (fSig != "fmt ")
                        {
                            log.WriteLine("audio file '" + file + "' format unsupported. (fmt mismatch)", log.LogMessageType.Error);
                            reader.Close();
                            return;
                        }

                        formatSize = reader.ReadInt32();
                        this.format = reader.ReadInt16();
                        channels = reader.ReadInt16();
                        sampleRate = reader.ReadInt32();
                        byteRate = reader.ReadInt32();
                        blockAlign = reader.ReadInt16();
                        bitDepth = reader.ReadInt16();

                        string dataSignature = new string(reader.ReadChars(4));
                        if (dataSignature != "data")
                        {
                            log.WriteLine("audio file '" + file + "' format unsupported. (signature mismatch)", log.LogMessageType.Error);
                            reader.Close();
                            return;
                        }

                        dataSize = reader.ReadInt32();
                        data = reader.ReadBytes(dataSize);
                        //log.WriteLine("loaded audio file ("+file+") @ "+data.Length+" bytes");

                    }
                    catch (Exception e){
                        log.WriteLine("audio file '" + file + "' failed to load. ("+e.Message+")", log.LogMessageType.Error);
                    }
                }
            }
        }

        public bool Ready()
        {
            return data != null;
        }
    }
}
