namespace CurrencyConverter.Api.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var startTime = DateTime.UtcNow;
            await _next(context);
            var endTime = DateTime.UtcNow;

            _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}, " +
                $"Client IP: {context.Connection.RemoteIpAddress}, " +
                $"Response Code: {context.Response.StatusCode}, " +
                $"Duration: {endTime - startTime}ms");
        }
    }
}
