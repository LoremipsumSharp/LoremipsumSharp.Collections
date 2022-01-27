using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using LoremipsumSharp.ChatBot.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace LoremipsumSharp.Diagnostic
{
    public class Alerter
    {
        private readonly IMemoryCache _cache;
        private readonly IChatBot _chatBot;

        public Alerter(IMemoryCache cache, IChatBot chatBot)
        {
            _cache = cache;
            _chatBot = chatBot;
        }

        public virtual void Alert(string key, string message, long throttleInterval)
        {
            if (_cache.TryGetValue(key, out var flag)) return;
            _cache.Set(key, true, DateTime.Now + TimeSpan.FromMilliseconds(throttleInterval));
            _chatBot.SendNotificationAsync(message);
        }
    }
}