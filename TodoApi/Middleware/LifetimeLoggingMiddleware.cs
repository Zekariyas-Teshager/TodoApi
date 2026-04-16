using TodoApi.Services.Interfaces;

namespace TodoApi.Middleware
{
    public class LifetimeLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LifetimeLoggingMiddleware> _logger;
        private readonly IDemoService _singleton; // Use IDemoService instead

        public LifetimeLoggingMiddleware(
            RequestDelegate next, 
            ILogger<LifetimeLoggingMiddleware> logger,
            [FromKeyedServices("singleton")] IDemoService singleton) // Keyed for singleton
        {
            _next = next;
            _logger = logger;
            _singleton = singleton;
        }

        public async Task InvokeAsync(
            HttpContext context,
            [FromKeyedServices("transient")] IDemoService transient, // Keyed for transient
            [FromKeyedServices("scoped")] IDemoService scoped)       // Keyed for scoped
        {
            _logger.LogInformation("=== NEW REQUEST ===");
            _logger.LogInformation("Middleware Singleton ID: {Id} - Created: {CreatedAt}", 
                _singleton.GetInstanceId(), _singleton.GetCreatedAt());
            _logger.LogInformation("Middleware Scoped ID: {Id} - Created: {CreatedAt}", 
                scoped.GetInstanceId(), scoped.GetCreatedAt());
            _logger.LogInformation("Middleware Transient ID: {Id} - Created: {CreatedAt}", 
                transient.GetInstanceId(), transient.GetCreatedAt());

            await _next(context);
        }
    }
}