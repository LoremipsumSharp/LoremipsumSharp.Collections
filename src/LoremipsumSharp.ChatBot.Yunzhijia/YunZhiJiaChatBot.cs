using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LoremipsumSharp.ChatBot.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LoremipsumSharp.ChatBot.YunZhiJia
{
    public class YunZhiJiaChatBot : IChatBot
    {
        private readonly IHttpClientFactory _factory;
        private YunZhiJiaChatBotOptions _options;

        public YunZhiJiaChatBot(IHttpClientFactory factory, IOptionsMonitor<YunZhiJiaChatBotOptions> options)
        {
            _factory = factory;
            _options = options.CurrentValue;

            options.OnChange(@new =>
            {
                this._options = @new;
            });
        }

        public async Task SendNotificationAsync(string message)
        {
            var client = _factory.CreateClient();
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.WebHookUrl);
            var stringContent = new StringContent(JsonConvert.SerializeObject(new
            {
                content = message
            }), Encoding.UTF8, "application/json");
            httpRequest.Content = stringContent;

             await client.SendAsync(httpRequest);

        }
    }
}