using CBakWeChatDesktop.Helpers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using log4net;
using CBakWeChatDesktop.Model;
using CBakWeChatDesktop.ViewModel;
using System.Diagnostics;
using Newtonsoft.Json;

namespace CBakWeChatDesktop
{
    /// <summary>
    /// AddSessionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddSessionWindow : Window
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AddSessionWindow));
        private MainWindow mainWindow;

        private AddSessionViewModel ViewModel = new AddSessionViewModel();
        public AddSessionWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            DataContext = ViewModel;
            LoadProcesses();
        }


        private void LoadProcesses()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("wechat");
                foreach (Process p in processes)
                {
                    var lHandles = NativeAPIHelper.GetHandleInfoForPID((uint)p.Id);
                    foreach (var h in lHandles)
                    {
                        string name = NativeAPIHelper.FindHandleName(h, p);
                        if (name != "")
                        {
                            // 预留handle log
                            if (File.Exists("handle.log"))
                            {
                                File.AppendAllText("handle.log", string.Format("{0}|{1}|{2}|{3}\n", p.Id, h.ObjectTypeIndex, h.HandleValue, name));
                            }
                            if (name.EndsWith("Applet.db"))
                            {
                                ProcessInfo info = new ProcessInfo();
                                info.ProcessId = p.Id.ToString();
                                info.ProcessName = p.ProcessName;
                                info.HandleName = DevicePathMapper.FromDevicePath(name);
                                ViewModel.Processes.Add(info);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("获取WX进程错误", ex);
                MessageBox.Show("获取 wx 进程错误： " + ex.Message);
            }
            
        }



        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ClickAddSession(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProcess == null)
            {
                ShowError("请选择进程");
                return;
            }
            if (string.IsNullOrEmpty(ViewModel.SessionName))
            {
                ShowError("请输入session名");
                return;
            }

            Process[] ProcessList = Process.GetProcessesByName("WeChat");
            if (ProcessList.Length == 0)
            {
                ShowError("没有找到微信进程");
                return;
            }
            if (ProcessList.Length > 1)
            {
                ShowError("无法处理多个微信进程");
                return;
            }

            Process WeChatProcess = ProcessList[0];

            nint WeChatWinBaseAddress = 0;
            string FileVersion = null;
            try
            {
                foreach (object obj in WeChatProcess.Modules)
                {
                    ProcessModule processModule = (ProcessModule)obj;
                    string processModuleName = processModule.ModuleName;
                    if (processModule.ModuleName == "WeChatWin.dll")
                    {
                        var FileVersionInfo = processModule.FileVersionInfo;
                        if (FileVersionInfo != null)
                        {
                            // 版本号
                            FileVersion = FileVersionInfo.FileVersion;
                        }
                        // 基址
                        WeChatWinBaseAddress = processModule.BaseAddress;
                        break;
                    }
                }
            } catch (Exception ex)
            {
                log.Error("获取 wx 版本号和基址错误", ex);
                ShowError("获取 wx 版本号和基址错误：" + ex.Message);
                return;
            }
            
            if (string.IsNullOrEmpty(FileVersion))
            {
                ShowError("微信版本号获取失败");
                return;
            }
            if (WeChatWinBaseAddress == 0)
            {
                ShowError("微信基址获取失败");
                return;
            }

            // 获取版本号对应的地址偏移量数据
            AddrInfo AddrInfo = null;
            try
            {
                AddrInfo = GetVersionInfo(FileVersion);
            }
            catch (Exception ex) {
                log.Error("获取 wx 偏移地址错误", ex);
                ShowError("获取 wx 偏移地址错：" + ex.Message);
                return;
            }
            

            if (AddrInfo == null)
            {
                ShowError($"不支持的版本号: {FileVersion}");
                return;
            }

            Session Session = RedProcess(WeChatProcess, WeChatWinBaseAddress, AddrInfo);
            Session.wx_id = GetWxId(ViewModel.SelectedProcess);
            Session.wx_dir = GetWxDir(ViewModel.SelectedProcess);
            if (string.IsNullOrEmpty(Session.wx_key))
            {
                ShowError("获取微信 key 失败");
                return;
            }
            try
            {

                JObject response = await ApiHelpers.AddSession(Session);
                var id = response["id"];
                if (id != null)
                {
                    Session.id = (int)id;
                    // 添加到 Main 中显示
                    mainWindow.SessionAdd(Session);
                    this.Close();
                } else
                {
                    ShowError($"服务端返回的数据异常， sesison id 不存在: {id}");
                }

            } catch (Exception ex)
            {
                log.Error("添加 session 异常", ex);
                var serverError = ex.Data["ResponseBody"];
                if (serverError != null)
                {
                    var responseBody = serverError.ToString();
                    if (responseBody != null)
                    {
                        var jsonResponse = JObject.Parse(responseBody);
                        var detail = jsonResponse["detail"];
                        if (detail != null)
                        {
                            MessageBox.Show($"添加失败: {detail.ToString()}", "添加 session 失败");
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"添加失败: {ex.Message}", "添加 session 失败");
                }
            }

        }

        private string GetWxId(ProcessInfo processInfo)
        {
            string[] dirs = processInfo.HandleName.Split("\\");
            return dirs[dirs.Length - 3];
        }

        private string GetWxDir(ProcessInfo processInfo)
        {
            string db = processInfo.HandleName;
            return db.Replace("Msg\\Applet.db", "");
        }

        private AddrInfo GetVersionInfo(string version)
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.json");
            string jsonString = File.ReadAllText(filePath);
            var jsonData = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(jsonString);
            if (jsonData != null && jsonData.TryGetValue(version, out var item))
            {
                AddrInfo info = new AddrInfo
                {
                    key = item.Count > 4 ? item[4] : 0,
                    name = item.Count > 0 ? item[0] : 0,
                    acctname = item.Count > 1 ? item[1] : 0,
                    mobile = item.Count > 2 ? item[2] : 0
                };
                return info;
            }

            return null;
        }

        private Session RedProcess(Process p, nint Base, AddrInfo info)
        {
            var session = new Session();
            string Key = WeChatMsgScan.GetKey(p, Base, info.key);
            var Name = WeChatMsgScan.GetName(p, Base, info.name);
            var Acctname = WeChatMsgScan.GetMobile(p, Base, info.acctname);
            var Mobile = WeChatMsgScan.GetMobile(p, Base, info.mobile);

            session.wx_key = Key;
            session.wx_name = Name;
            session.wx_acct_name = Acctname;
            session.wx_mobile = Mobile;
            session.name = ViewModel.SessionName;
            session.desc = ViewModel.SessionDesc;
            return session;
        }
        private void ShowError(string msg)
        {
            MessageBox.Show(msg);
            ViewModel.EventDesc = string.Empty;
        }

        private void ShowProgress(string msg)
        {
            ViewModel.EventDesc = msg;
        }
    }


    public class AddrInfo
    {
        public int key;
        public int name;
        public int acctname;
        public int mobile;
    }

}
