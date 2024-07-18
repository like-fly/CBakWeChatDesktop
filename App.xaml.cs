using System.Windows;
using CBakWeChatDesktop.login;
using CBakWeChatDesktop.Properties;

namespace CBakWeChatDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (IsUserLoggedIn())
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }

        // 模拟检查用户登录状态的方法
        private bool IsUserLoggedIn()
        {
            // 这里你可以实现你自己的登录状态检查逻辑
            // 例如，检查某个配置文件或用户凭证等
            // 目前我们假设用户未登录
            return Settings.Default.isLogin;
        }
    }

}
