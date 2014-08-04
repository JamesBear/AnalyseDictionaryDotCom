using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictManiac.Sources
{
    class FileUtil
    {
        public static bool ReadFile(string path, out string content)
        {
            try
            {
                StreamReader sr = new StreamReader(path);
                content = sr.ReadToEnd();
                sr.Close();
                return true;
            }
            catch
            {
                content = null;
                return false;
            }
        }

        public static bool WriteFile(string path, byte[] data)
        {
            try
            {
                File.WriteAllBytes(path, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                MyLogger.Attach("Creating directory: " + path);
                Directory.CreateDirectory(path);
            }
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
