using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoremipsumSharp.RabbitMq.Abstraction;

namespace LoremipsumSharp.RabbitMq.Extensions
{
    public static class MessageExtensions
    {
        public static bool TryGetDescription(this IMessage msg, out string description)
        {
            description = string.Empty;
            try
            {
                description = msg.GetDescription();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}