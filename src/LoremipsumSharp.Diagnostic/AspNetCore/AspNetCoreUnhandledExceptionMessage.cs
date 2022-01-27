using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LoremipsumSharp.Diagnostic.AspNetCore
{
    public class AspNetCoreUnhandledExceptionMessage
    {
        public string Type => "异常告警";
        public string SourceIp { get; set; }
        public string RequestId { get; set; }
        public string RequestUrl { get; set; }
        public double RequestDuration { get; set; }

        private string _exceptionMessage = string.Empty;
        public string ExceptionMessage { get { return _exceptionMessage.Length > 30 ? _exceptionMessage.Substring(0, 30) : _exceptionMessage; } set { _exceptionMessage = value; } }

        [JsonIgnore]
        public string ExceptionStackTrace { get; set; }
        private string _bodyString = string.Empty;
        public string BodyString { get { return _bodyString.Length > 30 ? _bodyString.Substring(0, 30) :_bodyString; } set { _bodyString = value; } }


        public override int GetHashCode()
        {
            return System.HashCode.Combine(this.RequestUrl, this.ExceptionStackTrace);
        }

        // public override string ToString()
        // {
        //     var properties = this.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
        //     .ToList();
        //     var messageBody = string.Join("\n\n", properties.Select(x => $"{x.Name}:{x.GetValue(this)}"));
        //     var messageTitle = "接口异常告警";
        //     return messageTitle + "\n\n" + messageBody;
        // }

    }
}