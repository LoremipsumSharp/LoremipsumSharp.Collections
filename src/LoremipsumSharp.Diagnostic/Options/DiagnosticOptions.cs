using System;
using System.Collections.Generic;


namespace LoremipsumSharp.Diagnostic
{
    public class DiagnosticOptions
    {
        public DiagnosticOptions()
        {
            this.AlertThrottleInterval = (long)(TimeSpan.FromMinutes(5).TotalMilliseconds);
            this.EfCoreSlowSqlThreshold = (long)(TimeSpan.FromSeconds(2).TotalMilliseconds);
            this.AspNetCoreRequestTimeoutElapsedThreshold = (long)(TimeSpan.FromSeconds(2).TotalMilliseconds);
            this.ExceptionNameFilters = new List<string>();
        }
        public long AspNetCoreRequestTimeoutElapsedThreshold { get; set; }
        public bool EnableAspNetCoreDiagnostic { get; set; }
        public long EfCoreSlowSqlThreshold { get; set; }
        public bool EnableEfCoreSlowQueryDiagnostic { get; set; }
        public long AlertThrottleInterval { get; set; }
        public List<string> ExceptionNameFilters { get; set; }
        public string ServiceName { get; set; }
    }
}