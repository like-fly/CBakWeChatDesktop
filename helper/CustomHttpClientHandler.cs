using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CBakWeChatDesktop.Helpers
{
    public class CustomHttpClientHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var exception = new HttpRequestException($"Request failed with status code {response.StatusCode}")
                {
                    Data = { ["ResponseBody"] = responseContent }
                };
                throw exception;
            }

            return response;
        }
    }
}
