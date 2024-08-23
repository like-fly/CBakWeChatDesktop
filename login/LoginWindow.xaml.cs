using CBakWeChatDesktop.Helpers;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CBakWeChatDesktop.login
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LoginWindow));

        LoginForm f = new LoginForm();

        

        public LoginWindow()
        {
            InitializeComponent();
            
            f.server = "http://127.0.0.1:9527";
            f.username = "admin";
            DataContext = f;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var loginData = new Dictionary<string, string>
            {
                { "username", f.username },
                { "password", this.passwordBox.Password }
            };
            log.Info($"登录，用户名{f.username}");
            try
            {
                JObject response = await ApiHelpers.Login(f.server, loginData);
                string token = response["access_token"].ToString();
                log.Info("登录成功");

                // 设置 token，server
                Properties.Settings.Default.token = token;
                Properties.Settings.Default.isLogin = true;
                Properties.Settings.Default.server = f.server;
                Properties.Settings.Default.Save();

                // 打开主窗体
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                // 关闭登录窗体
                this.Close();
            }
            catch (HttpRequestException ex)
            {
                log.Error("登录失败", ex);
                var serverError = ex.Data["ResponseBody"];
                if (serverError != null)
                {
                    var responseBody = serverError.ToString();
                    var jsonResponse = JObject.Parse(responseBody);
                    MessageBox.Show($"登录失败: {jsonResponse["detail"].ToString()}", "登录失败");
                } 
                else
                {
                    MessageBox.Show($"登录失败: {ex.Message}", "登录失败");
                }
                
            }
        }
    }

    class LoginForm
    {
        public string server {  get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
