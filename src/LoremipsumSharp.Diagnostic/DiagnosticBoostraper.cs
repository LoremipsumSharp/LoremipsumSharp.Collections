using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using LoremipsumSharp.Diagnostic.AspNetCore;
using LoremipsumSharp.EfCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoremipsumSharp.Diagnostic
{
    public class DiagnosticBoostraper : IHostedService, IObserver<DiagnosticListener>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CompositeDisposable _disposeables = new CompositeDisposable();
        private DiagnosticOptions _options;
        private readonly ILogger<DiagnosticBoostraper> _logger;

        public DiagnosticBoostraper(IServiceScopeFactory serviceScopeFactory,
                                    IHttpContextAccessor httpContextAccessor,
                                    IOptionsMonitor<DiagnosticOptions> options,
                                    ILogger<DiagnosticBoostraper> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _options = options.CurrentValue;

            options.OnChange(@new =>
            {
                _options = @new;
            });
        }

        public void OnCompleted()
        {
            _logger.LogInformation("the observable of DiagnosticListener.AllListeners completed ! ");
        }

        public void OnError(Exception error)
        {
            _logger.LogError($"the observable of DiagnosticListener.AllListeners has error:${error.ToString()}");
        }

        public void OnNext(DiagnosticListener value)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            if (value.Name == "Microsoft.AspNetCore")
            {
                if (_options.EnableAspNetCoreDiagnostic)
                {
                    AspNetCoreDiagnosticObserver aspNetCoreDiagnosticObserver = scope.ServiceProvider.GetService<AspNetCoreDiagnosticObserver>();
                    _disposeables.Add(value.SubscribeWithAdapter(aspNetCoreDiagnosticObserver));
                }
            }
            if (value.Name == "Microsoft.EntityFrameworkCore")
            {
                if (_options.EnableEfCoreSlowQueryDiagnostic)
                {
                    EfCoreDiagnosticsObserver efCoreDiagnosticsObserver = scope.ServiceProvider.GetService<EfCoreDiagnosticsObserver>();
                    _disposeables.Add(value.Subscribe(efCoreDiagnosticsObserver));
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this._disposeables.Add(DiagnosticListener.AllListeners.Subscribe(this));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this._disposeables.Dispose();
            return Task.CompletedTask;
        }
    }
}