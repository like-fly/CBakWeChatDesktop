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

namespace CBakWeChatDesktop
{
    /// <summary>
    /// AddSessionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddSessionWindow : Window
    {
        private MainWindow mainWindow;
        private SessionFormData sessionFormData;
        public AddSessionWindow(MainWindow mainWindow, WeChatMsg weChatMsg)
        {
            InitializeComponent();
            this.sessionFormData = new SessionFormData();
            this.mainWindow = mainWindow;
            bindData(weChatMsg);
        }

        private void bindData(WeChatMsg msg)
        {
            this.sessionFormData.wx_name = msg.name;
            this.sessionFormData.wx_acct_name = msg.accountname;
            this.sessionFormData.wx_key = msg.key;
            this.sessionFormData.wx_id = msg.wxid;
            this.sessionFormData.wx_mobile = msg.mobile;
            this.sessionFormData.wx_email = msg.mail;
            this.sessionFormData.pid = msg.pid;
            DataContext = this.sessionFormData;
        }
    }

    class SessionFormData
    {
        public string name { get; set; }
        public string desc { get; set; }
        public string wx_id { get; set; }
        public string wx_name { get; set; }
        public string wx_acct_name { get; set; }
        public string wx_key { get; set; }
        public string wx_mobile { get; set; }
        public string wx_email { get; set; }
        public string pid { get; set; }
    }
}
