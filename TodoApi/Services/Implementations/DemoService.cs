using TodoApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace TodoApi.Services.Implementations
{
    public abstract class BaseDemoService
    {
        protected readonly Guid _instanceId;
        protected readonly DateTime _createdAt;
        protected readonly ILogger _logger;

        protected BaseDemoService(ILogger logger)
        {
            _instanceId = Guid.NewGuid();
            _createdAt = DateTime.UtcNow;
            _logger = logger;
        }

        public string GetInstanceId() => _instanceId.ToString();
        public DateTime GetCreatedAt() => _createdAt;
    }

    public class TransientDemoService : BaseDemoService, IDemoService
    {
        public TransientDemoService(ILogger<TransientDemoService> logger) : base(logger) 
        {
            _logger.LogInformation("🚀 NEW TRANSIENT instance created: {InstanceId}", _instanceId);
        }

        public Dictionary<string, object> GetInfo() => new()
        {
            ["Lifetime"] = "Transient",
            ["InstanceId"] = GetInstanceId(),
            ["CreatedAt"] = GetCreatedAt().ToString("HH:mm:ss.fff"),
            ["Description"] = "New instance EVERY time - even within same request"
        };
    }

    public class ScopedDemoService : BaseDemoService, IDemoService
    {
        public ScopedDemoService(ILogger<ScopedDemoService> logger) : base(logger)
        {
            _logger.LogInformation("📦 NEW SCOPED instance created: {InstanceId}", _instanceId);
        }

        public Dictionary<string, object> GetInfo() => new()
        {
            ["Lifetime"] = "Scoped",
            ["InstanceId"] = GetInstanceId(),
            ["CreatedAt"] = GetCreatedAt().ToString("HH:mm:ss.fff"),
            ["Description"] = "Same instance WITHIN a request, different BETWEEN requests"
        };
    }

    public class SingletonDemoService : BaseDemoService, IDemoService
    {
        public SingletonDemoService(ILogger<SingletonDemoService> logger) : base(logger)
        {
            _logger.LogInformation("💎 SINGLETON instance created: {InstanceId}", _instanceId);
        }

        public Dictionary<string, object> GetInfo() => new()
        {
            ["Lifetime"] = "Singleton",
            ["InstanceId"] = GetInstanceId(),
            ["CreatedAt"] = GetCreatedAt().ToString("HH:mm:ss.fff"),
            ["Description"] = "SAME instance for entire application lifetime"
        };
    }
}