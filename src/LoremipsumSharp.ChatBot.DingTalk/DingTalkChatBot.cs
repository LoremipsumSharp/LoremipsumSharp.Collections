using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LoremipsumSharp.ChatBot.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LoremipsumSharp.ChatBot.DingTalk
{
    public class DingTalkChatBot : IChatBot
    {
        private readonly IHttpClientFactory _factory;
        private DingTalkChatBotOptions _options;

        public DingTalkChatBot(IHttpClientFactory factory, IOptionsMonitor<DingTalkChatBotOptions> options)
        {
            _factory = factory;
            _options = options.CurrentValue;

            options.OnChange(@new =>
            {
                _options = @new;
            });
        }


        public Task SendNotificationAsync(string message)
        {
            var payload = new
            {
                msgtype = "text",
                text = new
                {
                    content = message
                }
            };

            var timestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            var sign = Uri.EscapeDataString(SignSecret(_options.Secret, timestamp));
            var requestUrl = $"{_options.WebHookUrl}?timestamp={timestamp.ToString()}&sign={sign}";
            var client = _factory.CreateClient();
            return client.PostAsync(requestUrl,new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

        }

        private string SignSecret(string secret, long timestamp)
        {
            var stringToSign = timestamp + "\n" + secret;
            var encoding = new System.Text.UTF8Encoding();
            var keyByte = encoding.GetBytes(secret);
            var messageBytes = encoding.GetBytes(stringToSign);
            using var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyByte);
            var signData = hmacsha256.ComputeHash(messageBytes);
            var sign = Convert.ToBase64String(signData);
            return sign;

        }

    }
}