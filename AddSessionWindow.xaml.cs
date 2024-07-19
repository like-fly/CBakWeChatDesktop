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
using CBakWeChatDesktop.helper.form;
using Newtonsoft.Json.Linq;
using log4net;
using CBakWeChatDesktop.Model;
using CBakWeChatDesktop.ViewModel;
using System.Diagnostics;

namespace CBakWeChatDesktop
{
    /// <summary>
    /// AddSessionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddSessionWindow : Window
    {
        private MainWindow mainWindow;
        
        private AddSessionViewModel ViewModel = new AddSessionViewModel();
        public AddSessionWindow(MainWindow mainWindow, WeChatMsg weChatMsg)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            DataContext = ViewModel;
            LoadProcesses();
        }


        private void LoadProcesses()
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



        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ClickAddSession(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProcess == null)
            {
                MessageBox.Show("请选择进程", "错误");
                return;
            }
            if (string.IsNullOrEmpty(ViewModel.SessionName))
            {
                MessageBox.Show("请输入session名", "错误");
                return;
            }

            Process[] ProcessList = Process.GetProcessesByName("WeChat");
            if (ProcessList.Length == 0)
            {
                MessageBox.Show("没有找到微信进程", "错误");
                return;
            }
            if (ProcessList.Length > 1)
            {
                MessageBox.Show("无法处理多个微信进程", "错误");
                return;
            }

            Process WeChatProcess = ProcessList[0];

            nint WeChatWinBaseAddress = 0;
            string FileVersion = null;
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
            if (string.IsNullOrEmpty(FileVersion))
            {
                MessageBox.Show("微信版本号获取失败");
                return;
            }
            if (WeChatWinBaseAddress == 0)
            {
                MessageBox.Show("微信基址获取失败");
                return;
            }

            // 获取版本号对应的地址偏移量数据
            AddrInfo AddrInfo = GetVersionInfo(FileVersion);

            if (AddrInfo == null)
            {
                MessageBox.Show("不支持的版本号：" + FileVersion);
                return;
            }



            try
            {
                
                JObject response = await ApiHelpers.AddSession(ViewModel.Session);
                var id = response["id"];
                if (id != null)
                {
                    ViewModel.Session.id = (int)id;
                    // 添加到 Main 中显示
                    mainWindow.SessionAdd(ViewModel.Session);
                    this.Close();
                } else
                {
                    MessageBox.Show("错误，session id 不存在");
                }
                
            } catch (Exception ex)
            {
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

        private AddrInfo GetVersionInfo(string version)
        {
            return null;
        }

        private Session RedProcess(Process p, nint Base, AddrInfo info)
        {
            var session = new Session();
            string Key = WeChatMsgScan.GetKey(p, Base, info.key);
            //string Name = WeChatMsgScan.GetNam
            return session;
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
