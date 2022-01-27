
using System.Linq;

namespace LoremipsumSharp.Diagnostic.AspNetCore
{
    public class AspNetCoreRequestTimeoutMessage
    {
        public string Type => "超时告警";
        public string RequestId { get; set; }
        public string SourceIp { get; set; }
        private string _bodyString = string.Empty;
        public string BodyString { get { return _bodyString.Length > 30 ? _bodyString.Substring(0, 30) : _bodyString; } set { _bodyString = value; } }
        public string RequestUrl { get; set; }
        public double RequestDuration { get; set; }


        public override string ToString()
        {
            var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .ToList();
            var messageBody = string.Join("\n\n", properties.Select(x => $"{x.Name}:{x.GetValue(this)}"));
            var messageTitle = "接口超时告警";
            return messageTitle + "\n\n" + messageBody;
        }
    }
}