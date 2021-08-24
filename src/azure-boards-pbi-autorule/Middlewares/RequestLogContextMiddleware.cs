using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using shortid;
using shortid.Configuration;

namespace azure_boards_pbi_autorule.Middlewares
{
    public class RequestLogContextMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLogContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var opt = new GenerationOptions
            {
                Length = 8,
                UseNumbers = false,
                UseSpecialCharacters = false,
            };
            
            using (LogContext.PushProperty("CorrelationId", ShortId.Generate(opt)))
            {
                return _next.Invoke(context);
            }
        }
    }
}