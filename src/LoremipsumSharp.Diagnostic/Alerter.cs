using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using LoremipsumSharp.ChatBot.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LoremipsumSharp.Diagnostic
{
    public class Alerter
    {
        private readonly IMemoryCache _cache;
        private readonly IChatBot _chatBot;
        private ILogger<Alerter> _logger;

        public Alerter(IMemoryCache cache, IChatBot chatBot, ILogger<Alerter> logger)
        {
            _cache = cache;
            _chatBot = chatBot;
            _logger = logger;
        }

        public virtual async Task<bool> TryAlert(string key, string message, long throttleInterval)
        {
            if (_cache.TryGetValue(key, out var flag)) return false;
            _cache.Set(key, true, DateTime.Now + TimeSpan.FromMilliseconds(throttleInterval));

            try
            {
                await _chatBot.SendNotificationAsync(message);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return false;
            }


        }
    }
}