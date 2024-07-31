using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CBakWeChatDesktop.helper
{
    public class UploadHelper
    {

        private static readonly string IGNORE_FILE = "ignore.txt";
        private static readonly string FORCE_FILE = "force.txt";
        private static readonly string HASH_FILE = "uploaded_files.txt";

        public static List<string> IgnorePath()
        {
            var list = new List<string>();
            foreach (string line in File.ReadLines(IGNORE_FILE))
            {
                list.Add(line);
            }
            return list;
        }

        public static Boolean IsIgnore(string path, List<string> ignore)
        {
            foreach (var i in ignore)
            {
                if (path.Contains(i))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<string> ForceTypes()
        {
            var list = new List<string>();
            foreach (string line in File.ReadLines(FORCE_FILE))
            {
                list.Add(line);
            }
            return list;
        }

        public static Boolean IsForce(string path, List<string> forceTypes)
        {
            foreach (var i in forceTypes)
            {
                if (path.EndsWith(i))
                {
                    return true;
                }
            }
            return false;
        }

        public static string ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static HashSet<string> LoadUploadedFileHashes()
        {
            HashSet<string> hashs = new HashSet<string>();
            if (File.Exists(HASH_FILE))
            {

                foreach (var line in File.ReadAllLines(HASH_FILE))
                {
                    hashs.Add(line);
                }
            }
            return hashs;
        }

        public static void SaveHashToFile(HashSet<string> newFileHashSet)
        {
            if (newFileHashSet != null && newFileHashSet.Count > 0)
            {
                File.AppendAllLines(HASH_FILE, newFileHashSet);
            }

        }

    }
}
