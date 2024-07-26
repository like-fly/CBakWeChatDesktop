using CBakWeChatDesktop.Helpers;
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

namespace CBakWeChatDesktop
{
    public partial class MainWindow : Window
    {
        MainViewModel viewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            // 加载sessions数据
            LoadSessions();
            // 数据绑定
            DataContext = viewModel;
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
            await TraverseDirectory(this.viewModel.Session.wx_dir);
            SetEvent("数据同步完成", "");
            await ApiHelpers.Decrypt(this.viewModel.Session.id);
            SetDesc("服务器正在解析数据，稍后在网页端查看结果...");
        }

        private async Task TraverseDirectory(string path)
        {
            try
            {
                // 处理当前目录中的所有文件
                foreach (string file in Directory.GetFiles(path))
                {
                    SetDesc(file);
                    if (this.viewModel.Session != null)
                    {
                        string resp = await ApiHelpers.UploadSingle(file, this.viewModel.Session);
                    }
                }

                // 递归处理所有子目录
                foreach (string directory in Directory.GetDirectories(path))
                {
                    await TraverseDirectory(directory);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(e.Message);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private async void SyncClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("确定将数据同步到服务器吗？这需要花一些时间，请选退出微信再开始同步。", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question);

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


}
