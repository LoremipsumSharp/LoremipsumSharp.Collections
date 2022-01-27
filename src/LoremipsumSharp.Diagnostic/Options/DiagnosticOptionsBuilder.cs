using System;
using System.Collections.Generic;
using LoremipsumSharp.ChatBot.Abstractions;
using LoremipsumSharp.ChatBot.YunZhiJia;
using Microsoft.Extensions.DependencyInjection;

namespace LoremipsumSharp.Diagnostic.Options
{
    public class DiagnosticOptionsBuilder
    {
        public IServiceCollection Services { get; }
        private readonly long _alertThrottleInterval;


        public DiagnosticOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public DiagnosticOptionsBuilder EnableEfCoreSqlQueryDiagnostic(long slowSqlThreshold)
        {
            return Configure(opt =>
            {
                opt.EnableEfCoreSlowQueryDiagnostic = true;
                opt.EfCoreSlowSqlThreshold = slowSqlThreshold;
            });
        }

        public DiagnosticOptionsBuilder EnableAspNetCoreDiagnostic(long requestTimeoutThreshold, List<string> exceptionFilters)
        {
            return Configure(opt =>
            {
                opt.AspNetCoreRequestTimeoutElapsedThreshold = requestTimeoutThreshold;
                opt.EnableAspNetCoreDiagnostic = true;
                opt.ExceptionNameFilters = exceptionFilters;
            });
        }


        public DiagnosticOptionsBuilder Configure(Action<DiagnosticOptions> configuration)
        {
            Services.Configure(configuration);
            return this;
        }

        public DiagnosticOptionsBuilder UseYunZhiJiaChatBot(Action<YunZhiJiaChatBotOptions> configuration)
        {
            this.Services.AddSingleton<IChatBot, YunZhiJiaChatBot>();
            this.Services.Configure(configuration);
            return this;
        }

        public DiagnosticOptionsBuilder WithAlertThrottleInterval(long alertThrottleInterval)
        {
            return Configure(opt =>
            {
                opt.AlertThrottleInterval = alertThrottleInterval;
            });

        }
    }
}