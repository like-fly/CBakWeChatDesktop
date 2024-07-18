using Newtonsoft.Json.Linq;

namespace CBakWeChatDesktop.Helpers
{
    public class ApiHelpers
    {


        private string getServer()
        { 
            return Properties.Settings.Default.server;
        }

        public static async Task<JObject> Login(string server, Dictionary<string, string> loginData)
        {
            string url = "/api/auth/token";
            string response = await HttpHelper.PostAsyncWithUrlEncoded(server + url, loginData);
            return JObject.Parse(response);
        }

    }
}
