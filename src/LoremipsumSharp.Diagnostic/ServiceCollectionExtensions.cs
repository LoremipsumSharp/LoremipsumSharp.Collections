using System;
using LoremipsumSharp.Diagnostic.AspNetCore;
using LoremipsumSharp.Diagnostic.Options;
using LoremipsumSharp.EfCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoremipsumSharp.Diagnostic
{
    public static class ServiceCollectionExtensions
    {
        public static DiagnosticOptionsBuilder AddDiagnostic(this IServiceCollection services, Action<DiagnosticOptions> configure)
        {
            var builder = new DiagnosticOptionsBuilder(services);
            builder.Services.AddSingleton<EfCoreDiagnosticsObserver>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<AspNetCoreDiagnosticObserver>();
            builder.Services.AddSingleton<Alerter>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            builder.Services.AddHostedService<DiagnosticBoostraper>();
            builder.Services.Configure(configure);
            return builder;
        }

        public static DiagnosticOptionsBuilder AddDiagnostic(this IServiceCollection services)
        {
            var options = new DiagnosticOptions();
            return AddDiagnostic(services, (opt) =>
            {
                opt = options;
            });
        }
    }
}