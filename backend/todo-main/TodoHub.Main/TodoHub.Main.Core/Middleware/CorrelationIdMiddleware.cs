using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace TodoHub.Main.Core.Middleware
{
    public class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var cid = Guid.NewGuid().ToString("N");

            context.Response.Headers[HeaderName] = cid;

            using (LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(context);
            }
        }

    }
}
