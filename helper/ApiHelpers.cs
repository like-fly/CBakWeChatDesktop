using Newtonsoft.Json.Linq;
using System.Security.Policy;
using CBakWeChatDesktop.Model;
using Newtonsoft.Json;

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

        public static async Task<List<Session>> LoadSession()
        {
            string url = "/api/user/sys-sessions";
            string response = await HttpHelper.GetAsync(url);
            if (string.IsNullOrEmpty(response))
            {
                return null;
            }
            return JsonConvert.DeserializeObject<List<Session>>(response);
        }

        public static async Task<string> UploadSingle(string FilePath, Session Session)
        {
            string url = "/api/wx/upload-single/";
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("file_path", FilePath.Replace(Session.wx_dir, ""));
            data.Add("sys_session_id", Session.id.ToString());
            data.Add("wx_id", Session.wx_id);
            return await HttpHelper.UploadFile(url, FilePath, data);
        }

        public static async Task<string> Decrypt(int Id, long StartSyncTime)
        {
            string url = $"/api/wx/do-decrypt/{Id}";
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("update_time", StartSyncTime.ToString());
            return await HttpHelper.PostAsync(url);
        }


    }
}
