using CBakWeChatDesktop.Helpers;
using System;
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

namespace CBakWeChatDesktop
{
    /// <summary>
    /// AddSessionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddSessionWindow : Window
    {
        private MainWindow mainWindow;
        private Session session;
        public AddSessionWindow(MainWindow mainWindow, WeChatMsg weChatMsg)
        {
            InitializeComponent();
            this.session = new Session();
            this.mainWindow = mainWindow;
            BindData(weChatMsg);
        }

        private void BindData(WeChatMsg msg)
        {
            this.session.wx_name = msg.name;
            this.session.wx_acct_name = msg.accountname;
            this.session.wx_key = msg.key;
            this.session.wx_id = msg.wxid;
            this.session.wx_mobile = msg.mobile;
            this.session.wx_email = msg.mail;
            this.session.pid = msg.pid;
            DataContext = this.session;
        }


        private void CloseDialog(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void ClickAddSession(object sender, RoutedEventArgs e)
        {
            try
            {
                this.session.name = this.sessionNameTextBox.Text;
                this.session.desc = this.sessionDescTextBox.Text;
                JObject response = await ApiHelpers.AddSession(this.session);
                this.session.id = (int)response["id"];
                MessageBox.Show(response["id"].ToString(), "调试");
                // 添加到 Main 中显示
                mainWindow.SessionAdd(this.session);
                this.Close();
            } catch (Exception ex)
            {
                var serverError = ex.Data["ResponseBody"];
                if (serverError != null)
                {
                    var responseBody = serverError.ToString();
                    var jsonResponse = JObject.Parse(responseBody);
                    MessageBox.Show($"添加失败: {jsonResponse["detail"].ToString()}", "添加 session 失败");
                }
                else
                {
                    MessageBox.Show($"添加失败: {ex.Message}", "添加 session 失败");
                }
            }
            
        }
    }

    
}
