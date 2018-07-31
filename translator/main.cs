using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using YandexTranslateCSharpSdk;

namespace langtranslator
{
    class main
    {
        [STAThread]
        static void Main(string[] args)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Language Files|*.txt";
            openFileDialog1.Title = "Select a Language File";
            

            List<string> lines = new List<string>();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Enter language pair: (from-to, eg en-fr)");
                string lang = Console.ReadLine();
                using (var read = new StreamReader(openFileDialog1.OpenFile()))
                {
                    while (!read.EndOfStream)
                    {
                        string line = read.ReadLine();
                        string[] p = line.Split('=');
                        if (p[0] == "" || p[0][0] == '\'') lines.Add(line);
                        else
                        {
                            lines.Add(p[0]+"="+ Translate(p[0], p[1], lang));
                        }
                    }
                }
            }

            SaveFileDialog save = new SaveFileDialog();
            save.InitialDirectory = openFileDialog1.FileName;
            save.Filter = "Language Files|*.txt";
            save.Title = "Save a Language File";
            if (save.ShowDialog() == DialogResult.OK)
            {
                using (var read = new StreamWriter(save.OpenFile()))
                {
                    read.Flush();
                    foreach (var s in lines)
                    {
                        read.WriteLine(s);
                    }
                }
            }

            Console.ReadKey();
        }

        static string Translate(string c, string text, string lang)
        {
            Console.WriteLine("translating component "+ c);
            return TranslateTextAsync(text, lang).Result;
        }

        static async Task<string> TranslateTextAsync(string input, string languagePair)
        {
            YandexTranslateSdk wrapper = new YandexTranslateSdk();
            wrapper.ApiKey = "trnsl.1.1.20180719T123308Z.c79b1bb6c241198d.67e7e24df670a3b2cc04cfd53003ccd7cb8b72ca";
            string englishText = input;
            return await wrapper.TranslateText(englishText, languagePair);
        }
    }
}
