using System.Management;

namespace engine.system
{
    public class Hardware
    {
        // sourced from https://stackoverflow.com/questions/29667666/how-get-gpu-information-in-c
        public static string GetInfo(string data)
        {
            var objvide = new ManagementObjectSearcher("select * from Win32_VideoController");

            foreach (ManagementObject obj in objvide.Get()) return obj[data].ToString();

            return "?";
        }
    }
}