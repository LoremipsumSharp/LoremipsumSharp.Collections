using System;
using System.Linq;

namespace LoremipsumSharp.RabbitMq
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RabbitSubscription : Attribute
    {

        public string Exchange { get; set; }
        public string Queue { get; set; }
        public string RoutingKey { get; set; }
        public ushort PrefetchCount { get; set; } = 1;
        public string ConnectionKey { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .ToList();
            return string.Join(", ", properties.Select(x => $"{x.Name}:{x.GetValue(this)}"));
        }
    }
}
