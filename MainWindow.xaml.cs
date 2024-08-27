﻿using CBakWeChatDesktop.Helpers;
using CBakWeChatDesktop.login;
using CBakWeChatDesktop.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CBakWeChatDesktop.Model;
using System.IO;
using System.Windows.Documents;
using CBakWeChatDesktop.helper;
using System.Reflection;

namespace CBakWeChatDesktop
{
    public partial class MainWindow : Window
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));
        MainViewModel viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            // 加载sessions数据
            LoadSessions();
            // 数据绑定
            DataContext = viewModel;
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            viewModel.Version = version;
        }


        private async void LoadSessions()
        {
            viewModel.Sessions = new ObservableCollection<Session>();
            try
            {
                List<Session> UserSessions = await ApiHelpers.LoadSession();
                UserSessions.ForEach(session =>
                {
                    viewModel.Sessions.Add(session);
                });
            }
            catch (Exception ex) { 
                MessageBox.Show(ex.Message);
            }
            

            this.SessionList.ItemsSource = viewModel.Sessions;
        }

        private void SessionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionList.SelectedItem is Session selectedSession)
            {
                viewModel.Session = selectedSession;
            }
            else
            {
                viewModel.Session = null;
            }
        }

        private void SessionAddClick(object sender, RoutedEventArgs e)
        {
            AddSessionWindow addSessionWindow = new AddSessionWindow(this);
            addSessionWindow.ShowDialog();
        }

        public void SessionAdd(Session session)
        {
            viewModel.Sessions?.Add(session);
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.isLogin = false;
            Properties.Settings.Default.Save();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            // 关闭主窗体
            this.Close();
        }



        private async void SyncData()
        {
            if (this.viewModel.Session == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(this.viewModel.Session.wx_dir))
            {
                SetEvent("session未绑定微信目录");
                return;
            }
            if (!Path.Exists(this.viewModel.Session.wx_dir))
            {
                SetEvent($"目录不存在: {this.viewModel.Session.wx_dir}");
                return;
            }
            SetEvent("开始同步数据...");
            log.Info("开始同步数据...");
            try
            {
                UploadContext uploadContext = new UploadContext();
                uploadContext.Ignore = UploadHelper.IgnorePath();
                uploadContext.ForceTypes = UploadHelper.ForceTypes();
                uploadContext.FileHashSet = UploadHelper.LoadUploadedFileHashes(this.viewModel.Session.id);
                uploadContext.NewFileHashSet = new HashSet<string>();
                uploadContext.Path = this.viewModel.Session.wx_dir;

                // 使用 Task.Run 将耗时操作移至后台线程
                await Task.Run(async () =>
                {
                    await TraverseDirectory(uploadContext);
                });

                SetEvent("数据同步完成", "");
                await ApiHelpers.Decrypt(this.viewModel.Session.id);

                // 保存哈希到文件也放入后台线程
                await Task.Run(() =>
                {
                    UploadHelper.SaveHashToFile(uploadContext.NewFileHashSet, this.viewModel.Session.id);
                });

                SetDesc("服务器正在解析数据，稍后在网页端查看结果...");
            }
            catch (Exception ex)
            {
                log.Error("数据同步异常", ex);
            }

        }


        private async Task TraverseDirectory(UploadContext context)
        {
            try
            {
                // 处理当前目录中的所有文件
                foreach (string file in Directory.GetFiles(context.Path))
                {
                    
                    bool neetUpload = false;
                    var fileHash = UploadHelper.ComputeFileHash(file);
                    // string fileHash = await UploadHelper.ComputeFileHashAsync(file);
                    bool force = UploadHelper.IsForce(file, context.ForceTypes);
                    if (force)
                    {
                        neetUpload = true;
                    }
                    else
                    {
                        // 判断是否已经上传
                        if (!context.FileHashSet.Contains(fileHash))
                        {
                            neetUpload = true;
                        }
                    }
                    if (neetUpload)
                    {
                        SetDesc(file);
                        if (this.viewModel.Session != null)
                        {
                            log.Info("上传文件：" + file);
                            string resp = await ApiHelpers.UploadSingle(file, this.viewModel.Session);
                            if (!force)
                            {
                                context.NewFileHashSet.Add(fileHash);
                            }
                        }
                    }
                }

                // 递归处理所有子目录
                foreach (string directory in Directory.GetDirectories(context.Path))
                {
                    if (UploadHelper.IsIgnore(directory, context.Ignore)) 
                    {
                        log.Info($"文件夹忽略：{directory}");
                        continue; 
                    }
                    context.Path = directory;
                    await TraverseDirectory(context);
                }
            }
            catch (Exception ex) 
            {
                log.Error("数据同步异常", ex);
            }
        }

        private async void SyncClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确定将数据同步到服务器吗？这需要花一些时间，请务必先退出微信再开始同步，否则微信占用数据库文件，造成上传失败。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // 根据用户的选择执行相应的操作
            if (result == MessageBoxResult.Yes)
            {
                SyncData();
            }
        }

        private void SetEvent(string title, string desc = "")
        {
            this.viewModel.EventTitle = title;
            this.viewModel.EventDesc = desc;
        }

        private void SetDesc(String desc)
        {
            this.viewModel.EventDesc = desc;
        }
       
    }

    public class UploadContext
    {
        public string Path { get; set; }
        public List<string> Ignore { get; set; }
        public List<string> ForceTypes { get; set; }
        public HashSet<string> FileHashSet { get; set; }
        public HashSet<string> NewFileHashSet { get; set; }
    }


}
