using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LoremipsumSharp.Diagnostic.Extensions
{
    public static class HttpContextExtensions
    {
        public static bool IsJsonRequest(this HttpContext httpContext)
        {
            return httpContext.Request.ContentType?.Contains("application/json")??false; 
        }
    }
}