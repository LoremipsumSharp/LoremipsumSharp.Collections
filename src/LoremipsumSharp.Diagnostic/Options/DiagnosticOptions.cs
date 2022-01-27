using System;
using System.Collections.Generic;


namespace LoremipsumSharp.Diagnostic
{
    public class DiagnosticOptions
    {
        public long AspNetCoreRequestTimeoutElapsedThreshold { get; set; }
        public bool EnableAspNetCoreDiagnostic { get; set; }
        public long EfCoreSlowSqlThreshold { get; set; }
        public bool EnableEfCoreSlowQueryDiagnostic { get; set; }
        public long AlertThrottleInterval { get; set; }
        public List<string> ExceptionNameFilters { get; set; }
    }
}