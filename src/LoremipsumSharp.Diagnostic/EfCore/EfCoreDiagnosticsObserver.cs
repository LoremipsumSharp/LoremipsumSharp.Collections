using System;
using System.Collections.Generic;
using LoremipsumSharp.ChatBot.Abstractions;
using LoremipsumSharp.Diagnostic;
using LoremipsumSharp.Diagnostic.EfCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Options;

namespace LoremipsumSharp.EfCore
{
    public class EfCoreDiagnosticsObserver : IObserver<KeyValuePair<string, object?>>
    {
        private readonly IChatBot _chatBot;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DiagnosticOptions _options;
        private readonly Alerter _alerter;

        public EfCoreDiagnosticsObserver(IChatBot chatBot, IHttpContextAccessor httpContextAccessor, IOptionsMonitor<DiagnosticOptions> options, Alerter alerter)
        {
            _chatBot = chatBot;
            _httpContextAccessor = httpContextAccessor;
            _options = options.CurrentValue;
            _alerter = alerter;
        }

        [DiagnosticName("Microsoft.EntityFrameworkCore.Database.Command.CommandExecuted")]
        public void CommandExecuted(CommandExecutedEventData eventData)
        {
            if (eventData == null)
            {
                return;
            }
            var context = _httpContextAccessor.HttpContext;
            var alertMessage = new EfCoreSlowSqlAlertMessage(_options.ServiceName);
            alertMessage.CommandText = eventData.Command.CommandText;
            alertMessage.CommandDuration = eventData.Duration.TotalMilliseconds;
            if (alertMessage.CommandDuration < _options.EfCoreSlowSqlThreshold) return;
            if (context != null)
            {
                alertMessage.RequestId = context.TraceIdentifier;
                alertMessage.SourceIp = context.Connection.RemoteIpAddress.ToString();
                alertMessage.RequestUrl = $"{context.Request.Scheme} {context.Request.Host}{context.Request.Path} {context.Request.QueryString} ";
                alertMessage.BodyString = context.Items.TryGetValue("AspNetCoreDiagnosticObserver.BodyString", out var body) ? body.ToString() : string.Empty;
            }
            var throttleKey = $"EfCoreDiagnosticsObserver:SlowSql:Alert:{alertMessage.GetHashCode()}";
            _ = _alerter.TryAlert(throttleKey, alertMessage.ToString(), _options.AlertThrottleInterval);

        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (value.Key == RelationalEventId.CommandExecuted.Name && value.Value is CommandExecutedEventData commandExecutedEventData)
            {
                this.CommandExecuted(commandExecutedEventData);
            }
        }
    }
}