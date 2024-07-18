using CBakWeChatDesktop.helper.form;
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


        private void LoadSessions()
        {
            viewModel.Sessions = new ObservableCollection<Session>();

            var session = new Session();
            session.id = 1;
            session.name = "s1";
            session.desc = "s1";
            session.wx_id = "s1_wx_id";
            session.wx_name = "s1_wx_name";
            session.wx_acct_name = "s1_wx_acct_name";
            session.wx_key = "s1_wx_key";
            session.wx_mobile = "s1_wx_mobile";

            viewModel.Sessions.Add(session);
            this.SessionList.ItemsSource = viewModel.Sessions;
        }

        private void SessionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionList.SelectedItem is Session selectedSession)
            {
                StudentName.Text = selectedSession.name;
            }
            else
            {
                StudentName.Text = "";
                StudentAge.Text = "";
                StudentGrade.Text = "";
                StudentAddress.Text = "";
            }
        }

        private void SessionAddClick(object sender, RoutedEventArgs e)
        {
            var msg = WeChatMsgScan.ReadProcess();
            AddSessionWindow addSessionWindow = new AddSessionWindow(this, msg);
            addSessionWindow.ShowDialog();
        }

        public void SessionAdd(Session session)
        {
            viewModel.Sessions.Add(session);
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
    }


}
