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
        private static readonly string HASH_FOLDER = "hash";

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

        public static async Task<string> ComputeFileHashAsync(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                byte[] hash = await md5.ComputeHashAsync(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public static HashSet<string> LoadUploadedFileHashes(int session_id)
        {
            string HashFile = GetHashFilePath(session_id);
            HashSet<string> hashs = new HashSet<string>();
            if (File.Exists(HashFile))
            {

                foreach (var line in File.ReadAllLines(HashFile))
                {
                    hashs.Add(line);
                }
            }
            return hashs;
        }

        public static void SaveHashToFile(HashSet<string> newFileHashSet, int session_id)
        {
            string HashFile = GetHashFilePath(session_id);
            if (newFileHashSet != null && newFileHashSet.Count > 0)
            {
                File.AppendAllLines(HashFile, newFileHashSet);
            }

        }

        public static string GetHashFilePath(int session_id)
        {
           
            if (!Directory.Exists(HASH_FOLDER))
            {
                Directory.CreateDirectory(HASH_FOLDER);
            }
            return HASH_FOLDER + "/" + session_id + ".txt";
        }

    }
}
