using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Reflection.Metadata;

namespace CBakWeChatDesktop.Helpers
{
    public class HttpHelper
    {
        private static readonly HttpClient client = new HttpClient(new CustomHttpClientHandler());
        private static readonly HttpClient uploadClient;

        static HttpHelper()
        {
            uploadClient = new HttpClient(new CustomHttpClientHandler())
            {
                Timeout = TimeSpan.FromSeconds(1200) // 设置为 1200 秒 (20分钟)
            };
        }

        // 设置 token

        public static string GetServer()
        {
            return Properties.Settings.Default.server;
        }

        public static string GetToken()
        {
            return Properties.Settings.Default.token;
        }

        // 设置 server 地址

        // 发送 GET 请求
        public static async Task<string> GetAsync(string path)
        {
            var token = GetToken();
            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            HttpResponseMessage response = await client.GetAsync(GetServer() + path);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // 发送 POST 请求
        public static async Task<string> PostAsync(string path, object data = null)
        {
            var token = GetToken();
            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string json = "";
            if (data != null)
            {
                json = JsonConvert.SerializeObject(data);
            }
            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            
            HttpResponseMessage response = await client.PostAsync(GetServer() + path, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> PostAsyncWithUrlEncoded(string path, Dictionary<string, string> data)
        {
            var content = new FormUrlEncodedContent(data);

            HttpResponseMessage response = await client.PostAsync(path, content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


        public static async Task<string> UploadFile(string path, string filePath, Dictionary<string, string> data = null)
        {
            var token = GetToken();
            if (token != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            var form = new MultipartFormDataContent();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            var fileName = Path.GetFileName(filePath);
            form.Add(fileContent, "file", fileName);
            if (data != null)
            {
                foreach (KeyValuePair<string, string> kvp in data)
                {
                    form.Add(new StringContent(kvp.Value), kvp.Key);
                }
            }
            HttpResponseMessage response = await uploadClient.PostAsync(GetServer() + path, form);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }


    }
}
