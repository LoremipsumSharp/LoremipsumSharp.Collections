using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoremipsumSharp.Diagnostic.AspNetCore;
using LoremipsumSharp.Diagnostic.Options;
using LoremipsumSharp.EfCore;
using Microsoft.Extensions.DependencyInjection;

namespace LoremipsumSharp.Diagnostic
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiagnostic(this IServiceCollection services, Action<DiagnosticOptionsBuilder> configurer)
        {
            var builder = new DiagnosticOptionsBuilder(services);
            builder.Services.AddSingleton<EfCoreDiagnosticsObserver>();
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<AspNetCoreDiagnosticObserver>();
            builder.Services.AddSingleton<Alerter>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddMemoryCache();
            builder.Services.AddHostedService<DiagnosticBoostraper>();
            configurer.Invoke(builder);
            return services;
        }
    }
}