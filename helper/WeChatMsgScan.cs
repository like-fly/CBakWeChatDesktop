using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using log4net;

namespace CBakWeChatDesktop.Helpers
{


    public class WeChatMsgScan
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WeChatMsgScan));

        
        public static string GetName(Process p, IntPtr Base, IntPtr lpBaseAddress, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, Base + lpBaseAddress, array, nSize, 0) == 0)
            {
                return "";
            }
            string text = "";
            foreach (char c in Encoding.UTF8.GetString(array))
            {
                if (c == '\0')
                {
                    break;
                }
                text += c.ToString();
            }
            return text;
        }
        public static string GetAccount(Process p, IntPtr Base, IntPtr Account, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, Base + Account, array, nSize, 0) == 0)
            {
                return "";
            }
            string text = "";
            foreach (char c in Encoding.UTF8.GetString(array))
            {
                if (c == '\0')
                {
                    break;
                }
                text += c.ToString();
            }
            return text;
        }
        public static string GetMobile(Process p, IntPtr Base, IntPtr Mobile, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, Base + Mobile, array, nSize, 0) == 0)
            {
                return "";
            }
            string text = "";
            foreach (char c in Encoding.UTF8.GetString(array))
            {
                if (c == '\0')
                {
                    break;
                }
                text += c.ToString();
            }
            return text;
        }
        public static string GetMail(Process p, IntPtr Base, IntPtr Email, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, Base + Email, array, nSize, 0) == 0)
            {
                return "";
            }
            string text = "";
            foreach (char c in Encoding.UTF8.GetString(array))
            {
                if (c == '\0')
                {
                    break;
                }
                text += c.ToString();
            }
            return text;
        }

        public static string GetKey(Process p, IntPtr Base, IntPtr KeyAddr)
        {
            Int64 WeChatKey = Base + KeyAddr;
            return GetHex(p.Handle, (IntPtr)WeChatKey);
        }


        public static string GetHex(IntPtr hProcess, IntPtr lpBaseAddress)
        {
            byte[] array = new byte[8];
            if (WeChatMsgScan.ReadProcessMemory(hProcess, lpBaseAddress, array, 8, 0) == 0)
            {
                return "";
            }
            int num = 32;
            byte[] array2 = new byte[num];
            IntPtr lpBaseAddress2 = (IntPtr)(((long)array[7] << 56) + ((long)array[6] << 48) + ((long)array[5] << 40) + ((long)array[4] << 32) + ((long)array[3] << 24) + ((long)array[2] << 16) + ((long)array[1] << 8) + (long)array[0]);
            if (WeChatMsgScan.ReadProcessMemory(hProcess, lpBaseAddress2, array2, num, 0) == 0)
            {
                return "";
            }
            return WeChatMsgScan.bytes2hex(array2);
        }

        private static string bytes2hex(byte[] bytes)
        {
            return BitConverter.ToString(bytes, 0).Replace("-", string.Empty).ToLower().ToUpper();
        }
        [DllImport("kernel32.dll")]
        public static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern int GetModuleHandleA(string moduleName);
        [DllImport("kernel32.dll")]
        public static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, int lpNumberOfBytesRead);


    }
}
