using System;
using System.Reflection;
using System.Threading.Tasks;
using LoremipsumSharp.EfCore;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace LoremipsumSharp.AspNetCore.Filters
{
    public class UowActionActionFilter : IAsyncActionFilter
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        public UowActionActionFilter(IServiceProvider serviceProvider, ILogger<UowActionActionFilter> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (controllerActionDescriptor == null)
            {
                await next();
                return;
            }

            var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();
            if (unitOfWork == null)
            {
                await next();
                return;
            }

            var unitOfWorkAttr = controllerActionDescriptor.MethodInfo.GetCustomAttribute<UnitOfWorkAttribute>();
            if (unitOfWorkAttr != null)
            {
                if (unitOfWorkAttr.IsTransactional)
                {
                    unitOfWork.Begin(unitOfWorkAttr.IsolationLevel);
                }
            }

            var result = await next();

            if (result.Exception == null || result.ExceptionHandled)
            {
                await unitOfWork.CompleteAsync();
            }
        }
    }
}