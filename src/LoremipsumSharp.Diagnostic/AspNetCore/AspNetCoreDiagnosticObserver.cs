using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LoremipsumSharp.Diagnostic.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LoremipsumSharp.Diagnostic.AspNetCore
{
    public class AspNetCoreDiagnosticObserver
    {
        private DiagnosticOptions _options;
        private readonly IMemoryCache _memCache;
        private readonly Alerter _alerter;


        public AspNetCoreDiagnosticObserver(IOptionsMonitor<DiagnosticOptions> options, IMemoryCache memoryCache, Alerter alerter)
        {
            _options = options.CurrentValue;
            _memCache = memoryCache;
            _alerter = alerter;
        }

        [DiagnosticName("Microsoft.AspNetCore.Hosting.UnhandledException")]
        public void HostingUnhandledException(HttpContext httpContext, Exception exception)
        {
            var alertMessage = new AspNetCoreUnhandledExceptionMessage(_options.ServiceName);
            alertMessage.ExceptionMessage = exception.Message?.ToString() ?? string.Empty;
            alertMessage.ExceptionStackTrace = exception.StackTrace?.ToString() ?? string.Empty;

            foreach (var filter in _options.ExceptionNameFilters)
            {
                var regex = new Regex(filter);
                var m = regex.Match(exception.GetType().Name);
                if (m.Success)
                    return;
            }

            var stopwatch = (Stopwatch)httpContext.Items["AspNetCoreDiagnosticObserver.Stopwatch"];
            if (stopwatch != null)
                alertMessage.RequestDuration = stopwatch.Elapsed.TotalMilliseconds;
            alertMessage.RequestId = httpContext.TraceIdentifier;
            alertMessage.SourceIp = httpContext.Connection.RemoteIpAddress.ToString();
            alertMessage.RequestUrl = $"{httpContext.Request.Scheme} {httpContext.Request.Host}{httpContext.Request.Path} {httpContext.Request.QueryString} ";
            alertMessage.BodyString = httpContext.Items.TryGetValue("AspNetCoreDiagnosticObserver.BodyString", out var body) ? body.ToString() : string.Empty;


            var throttleKey = $"AspNetCoreDiagnosticObserver:UnhandledException:Alert:{alertMessage.GetHashCode()}";
            _ = _alerter.TryAlert(throttleKey, JsonConvert.SerializeObject(alertMessage), _options.AlertThrottleInterval);
        }


        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Start")]
        public void BeginRequest(HttpContext httpContext)
        {
            httpContext.Items["AspNetCoreDiagnosticObserver.Stopwatch"] = Stopwatch.StartNew();
            httpContext.Items["AspNetCoreDiagnosticObserver.BodyString"] = CollectBody(httpContext);
        }


        [DiagnosticName("Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop")]
        public void EndRequest(HttpContext httpContext)
        {
            var alertMessage = new AspNetCoreRequestTimeoutMessage(_options.ServiceName);
            var stopwatch = (Stopwatch)httpContext.Items["AspNetCoreDiagnosticObserver.Stopwatch"];
            if (stopwatch == null) return;
            alertMessage.RequestDuration = stopwatch.Elapsed.TotalMilliseconds;
            if (alertMessage.RequestDuration < _options.AspNetCoreRequestTimeoutElapsedThreshold) return;

            alertMessage.RequestId = httpContext.TraceIdentifier;
            alertMessage.SourceIp = httpContext.Connection.RemoteIpAddress.ToString();
            alertMessage.RequestUrl = $"{httpContext.Request.Scheme} {httpContext.Request.Host}{httpContext.Request.Path} {httpContext.Request.QueryString} ";
            alertMessage.BodyString = httpContext.Items["AspNetCoreDiagnosticObserver.BodyString"]?.ToString();

            var throttleKey = $"AspNetCoreDiagnosticObserver:Timeout:Alert:{alertMessage.RequestUrl}";
            _ = _alerter.TryAlert(throttleKey, JsonConvert.SerializeObject(alertMessage), _options.AlertThrottleInterval);
        }

        private string CollectBody(HttpContext httpContext)
        {
            if (httpContext.Request.ContentLength <= 0 || !httpContext.IsJsonRequest()) return string.Empty;
#if NETSTANDARD2_0
            httpContext.Request.EnableRewind();
#else
            httpContext.Request.EnableBuffering();
#endif
            httpContext.Request.Body.Position = 0;

            try
            {
                using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true);
                var body = reader.ReadToEndAsync().Result;
                return body;
            }
            finally
            {
                httpContext.Request.Body.Position = 0;
            }



        }
    }
}