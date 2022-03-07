using System.Linq;
using Newtonsoft.Json;

namespace LoremipsumSharp.Diagnostic.EfCore
{
    public class EfCoreSlowSqlAlertMessage:AlertMessageBase
    {
        public EfCoreSlowSqlAlertMessage(string serviceName) : base(serviceName)
        {
        }

        public string CommandText { get; set; }
        public double CommandDuration { get; set; }
        public string RequestId { get; set; }
        public string SourceIp { get; set; }
        public string BodyString { get; set; }
        public string RequestUrl { get; set; }

        public override string ToString()
        {
            var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .ToList();
            return string.Join("\n\n", properties.Select(x => $"{x.Name}:{x.GetValue(this)}"));
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(this.CommandText, this.RequestUrl);
        }

    }
}