using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoremipsumSharp.Diagnostic
{
    public class AlertMessageBase
    {
        public string ServiceName{get;set;}

        public AlertMessageBase(string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}