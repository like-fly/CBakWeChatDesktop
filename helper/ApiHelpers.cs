using Newtonsoft.Json.Linq;
using System.Security.Policy;
using CBakWeChatDesktop.Model;

namespace CBakWeChatDesktop.Helpers
{
    public class ApiHelpers
    {


        private static string getServer()
        { 
            return Properties.Settings.Default.server;
        }

        private static string getUrl(string url)
        {
            return getServer() + url;
        }

        public static async Task<JObject> Login(string server, Dictionary<string, string> loginData)
        {
            string url = "/api/auth/token";
            string response = await HttpHelper.PostAsyncWithUrlEncoded(server + url, loginData);
            return JObject.Parse(response);
        }

        public static async Task<JObject> AddSession(Session session)
        {
            string url = "/api/user/sys-session";
            string response = await HttpHelper.PostAsync(url, session);
            return JObject.Parse(response);
        }

    }
}
