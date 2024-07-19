using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using log4net;

namespace CBakWeChatDesktop.Helpers
{

    public class WeChatMsg 
    {
        public string name;
        public string accountname;
        public string mobile;
        public string mail;
        public string key;
        public string version;
        public string dir;
        public string wxid;
        public string pid;
        public string sessionid;
    }


    public class WeChatMsgScan
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(WeChatMsgScan));

        public static WeChatMsg ReadProcess()
        {
            log.Info("get weixin memory infomations");
            WeChatMsg WeChatMsg = null;
            List<int> SupportList = null;
            
            Process[] processList = Process.GetProcessesByName("WeChat");
            int count = processList.Length;
            log.Info($"weixin process list size is: {count}");
            if (processList.Length == 0)
            {
                return null;
            }
            Process WeChatProcess = processList[0];
            

            WeChatMsg = new WeChatMsg();
            int pid = WeChatProcess.Id;
            WeChatMsg.pid = pid.ToString();
            log.Info($"weixin process id is {pid}");

            foreach (object obj in WeChatProcess.Modules)
            {
                ProcessModule processModule = (ProcessModule)obj;
                string processModuleName = processModule.ModuleName;
                if (processModule.ModuleName == "WeChatWin.dll")
                {
                    // 基址
                    WeChatMsgScan.WeChatWinBaseAddress = processModule.BaseAddress;
                    string FileVersion = processModule.FileVersionInfo.FileVersion;
                    log.Info($"version is: {FileVersion}");
                    WeChatMsg.version = FileVersion;
                    // 版本号
                    if (!WeChatMsgScan.VersionList.TryGetValue(FileVersion, out SupportList))
                    {
                        MessageBox.Show("Current WeiXin version is: " + FileVersion + " Not Support.");
                        return null;
                    }
                    break;
                }
            }
            if (SupportList == null)
            {
                MessageBox.Show("WeChat Base Address Get Faild.");
                return null;
            }
            // 获取信息
            Int64 WeChatKey = (Int64)WeChatMsgScan.WeChatWinBaseAddress + SupportList[4];
            // string HexKey = WeChatMsgScan.GetHex(WeChatProcess.Handle, (IntPtr)WeChatKey);
            string HexKey = GetKey(WeChatProcess, WeChatMsgScan.WeChatWinBaseAddress, SupportList[4]);
            log.Info($"weixin hex key is {HexKey}");
            if (!string.IsNullOrWhiteSpace(HexKey))
            {
                WeChatMsg.key = HexKey;
            }

            Int64 WeChatName = (Int64)WeChatMsgScan.WeChatWinBaseAddress + SupportList[0];
            string name = WeChatMsgScan.GetName(WeChatProcess, (IntPtr)WeChatName, 100);
            WeChatMsg.name = name;
            log.Info($"weixin user name is {name}");
            Int64 WeChatAccount = (Int64)WeChatMsgScan.WeChatWinBaseAddress + SupportList[1];
            string Account = WeChatMsgScan.GetMobile(WeChatProcess, (IntPtr)WeChatAccount);
            log.Info($"weixin account is {Account}");
            if (!string.IsNullOrWhiteSpace(Account))
            {
                WeChatMsg.accountname = Account;
            }
            Int64 WeChatMobile = (Int64)WeChatMsgScan.WeChatWinBaseAddress + SupportList[2];
            string Mobile = WeChatMsgScan.GetMobile(WeChatProcess, (IntPtr)WeChatMobile);
            log.Info($"weixin mobile is {Mobile}");
            if (!string.IsNullOrWhiteSpace(Mobile))
            {
                WeChatMsg.mobile = Mobile;
            }

            Int64 WeChatMail = (Int64)WeChatMsgScan.WeChatWinBaseAddress + SupportList[3];
            string Mail = WeChatMsgScan.GetMail(WeChatProcess, (IntPtr)WeChatMail);
            log.Info($"weixin mail is {Mail}");
            if (!string.IsNullOrWhiteSpace(Mail) != false)
            {
                WeChatMsg.mail = Mail;
            }
            return WeChatMsg;
        }
        
        public static string GetName(Process p, IntPtr lpBaseAddress, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, lpBaseAddress, array, nSize, 0) == 0)
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
        public static string GetAccount(Process p, IntPtr lpBaseAddress, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, lpBaseAddress, array, nSize, 0) == 0)
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
        public static string GetMobile(Process p, IntPtr lpBaseAddress, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, lpBaseAddress, array, nSize, 0) == 0)
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
        public static string GetMail(Process p, IntPtr lpBaseAddress, int nSize = 100)
        {
            byte[] array = new byte[nSize];
            if (WeChatMsgScan.ReadProcessMemory(p.Handle, lpBaseAddress, array, nSize, 0) == 0)
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
            Int64 WeChatKey = (Int64)WeChatMsgScan.WeChatWinBaseAddress + KeyAddr;
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
        public static Dictionary<string, List<int>> VersionList = new Dictionary<string, List<int>>
        {
            {
                "3.2.1.154",
                new List<int>
                {
                    328121948,
                    328122328,
                    328123056,
                    328121976,
                    328123020
                }
            },
            {
                "3.3.0.115",
                new List<int>
                {
                    31323364,
                    31323744,
                    31324472,
                    31323392,
                    31324436
                }
            },
            {
                "3.3.0.84",
                new List<int>
                {
                    31315212,
                    31315592,
                    31316320,
                    31315240,
                    31316284
                }
            },
            {
                "3.3.0.93",
                new List<int>
                {
                    31323364,
                    31323744,
                    31324472,
                    31323392,
                    31324436
                }
            },
            {
                "3.3.5.34",
                new List<int>
                {
                    30603028,
                    30603408,
                    30604120,
                    30603056,
                    30604100
                }
            },
            {
                "3.3.5.42",
                new List<int>
                {
                    30603012,
                    30603392,
                    30604120,
                    30603040,
                    30604084
                }
            },
            {
                "3.3.5.46",
                new List<int>
                {
                    30578372,
                    30578752,
                    30579480,
                    30578400,
                    30579444
                }
            },
            {
                "3.4.0.37",
                new List<int>
                {
                    31608116,
                    31608496,
                    31609224,
                    31608144,
                    31609188
                }
            },
            {
                "3.4.0.38",
                new List<int>
                {
                    31604044,
                    31604424,
                    31605152,
                    31604072,
                    31605116
                }
            },
            {
                "3.4.0.50",
                new List<int>
                {
                    31688500,
                    31688880,
                    31689608,
                    31688528,
                    31689572
                }
            },
            {
                "3.4.0.54",
                new List<int>
                {
                    31700852,
                    31701248,
                    31700920,
                    31700880,
                    31701924
                }
            },
            {
                "3.4.5.27",
                new List<int>
                {
                    32133788,
                    32134168,
                    32134896,
                    32133816,
                    32134860
                }
            },
            {
                "3.4.5.45",
                new List<int>
                {
                    32147012,
                    32147392,
                    32147064,
                    32147040,
                    32148084
                }
            },
            {
                "3.5.0.20",
                new List<int>
                {
                    35494484,
                    35494864,
                    35494536,
                    35494512,
                    35495556
                }
            },
            {
                "3.5.0.29",
                new List<int>
                {
                    35507980,
                    35508360,
                    35508032,
                    35508008,
                    35509052
                }
            },
            {
                "3.5.0.33",
                new List<int>
                {
                    35512140,
                    35512520,
                    35512192,
                    35512168,
                    35513212
                }
            },
            {
                "3.5.0.39",
                new List<int>
                {
                    35516236,
                    35516616,
                    35516288,
                    35516264,
                    35517308
                }
            },
            {
                "3.5.0.42",
                new List<int>
                {
                    35512140,
                    35512520,
                    35512192,
                    35512168,
                    35513212
                }
            },
            {
                "3.5.0.44",
                new List<int>
                {
                    35510836,
                    35511216,
                    35510896,
                    35510864,
                    35511908
                }
            },
            {
                "3.5.0.46",
                new List<int>
                {
                    35506740,
                    35507120,
                    35506800,
                    35506768,
                    35507812
                }
            },
            {
                "3.6.0.18",
                new List<int>
                {
                    35842996,
                    35843376,
                    35843048,
                    35843024,
                    35844068
                }
            },
            {
                "3.6.5.7",
                new List<int>
                {
                    35864356,
                    35864736,
                    35864408,
                    35864384,
                    35865428
                }
            },
            {
                "3.6.5.16",
                new List<int>
                {
                    35909428,
                    35909808,
                    35909480,
                    35909456,
                    35910500
                }
            },
            {
                "3.7.0.26",
                new List<int>
                {
                    37105908,
                    37106288,
                    37105960,
                    37105936,
                    37106980
                }
            },
            {
                "3.7.0.29",
                new List<int>
                {
                    37105908,
                    37106288,
                    37105960,
                    37105936,
                    37106980
                }
            },
            {
                "3.7.0.30",
                new List<int>
                {
                    37118196,
                    37118576,
                    37118248,
                    37118224,
                    37119268
                }
            },
            {
                "3.7.5.11",
                new List<int>
                {
                    37883280,
                    37884088,
                    37883136,
                    37883008,
                    37884052
                }
            },
            {
                "3.7.5.23",
                new List<int>
                {
                    37895736,
                    37896544,
                    37895592,
                    37883008,
                    37896508
                }
            },
            {
                "3.7.5.27",
                new List<int>
                {
                    37895736,
                    37896544,
                    37895592,
                    37895464,
                    37896508
                }
            },
            {
                "3.7.5.31",
                new List<int>
                {
                    37903928,
                    37904736,
                    37903784,
                    37903656,
                    37904700
                }
            },
            {
                "3.7.6.24",
                new List<int>
                {
                    38978840,
                    38979648,
                    38978696,
                    38978604,
                    38979612
                }
            },
            {
                "3.7.6.29",
                new List<int>
                {
                    38986376,
                    38987184,
                    38986232,
                    38986104,
                    38987148
                }
            },
            {
                "3.7.6.44",
                new List<int>
                {
                    39016520,
                    39017328,
                    39016376,
                    38986104,
                    39017292
                }
            },
            {
                "3.8.0.31",
                new List<int>
                {
                    46064088,
                    46064912,
                    46063944,
                    38986104,
                    46064876
                }
            },
            {
                "3.8.0.33",
                new List<int>
                {
                    46059992,
                    46060816,
                    46059848,
                    38986104,
                    46060780
                }
            },
            {
                "3.8.0.41",
                new List<int>
                {
                    46064024,
                    46064848,
                    46063880,
                    38986104,
                    46064812
                }
            },
            {
                "3.8.1.26",
                new List<int>
                {
                    46409448,
                    46410272,
                    46409304,
                    38986104,
                    46410236
                }
            },
            {
                "3.9.0.28",
                new List<int>
                {
                    48418376,
                    48419280,
                    48418232,
                    38986104,
                    48419244
                }
            },
            {
                "3.9.2.23",
                new List<int>
                {
                    50320784,
                    50321712,
                    50320640,
                    38986104,
                    50321676
                }
            },
            {
                "3.9.2.26",
                new List<int>
                {
                    50329040,
                    50329968,
                    50328896,
                    38986104,
                    50329932
                }
            },
            {
                "3.9.5.91",
                new List<int>
                {
                    61654904,
                    61654680,
                    61654712,
                    38986104,
                    61656176
                }
             },
            {
                "3.9.6.19",
                new List<int>
                {
                    61997688,
                    61997464,
                    61997496,
                    38986104,
                    61998960
                }
            },
            { "3.9.6.33",
                new List<int>
                {
                    62030600,
                    62031936,
                    62030408,
                    38986104,
                    62031872
                }

            },
            { "3.9.10.19",
                new List<int>
                {
                    95129768,
                    95131104,
                    95129576,
                    0,
                    95131040
                }

            },
            { "3.9.10.27",
                new List<int>
                {
                    95125656,
                    95126992,
                    95125464,
                    0,
                    95126928
                }
            },
            { "3.9.11.17",
                new List<int>
                {
                    93550360,
                    93551696,
                    93550168,
                    0,
                    93551632
                }
            }
        };
        private static IntPtr WeChatWinBaseAddress = IntPtr.Zero;

    }
}
