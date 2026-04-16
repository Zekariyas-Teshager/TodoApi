using Microsoft.Extensions.Logging;
using TodoApi.Services.Interfaces;

namespace TodoApi.Services.Implementations
{
    public class LifetimeDemoService : ILifetimeDemoService
    {
        private readonly IDemoService _transient1;
        private readonly IDemoService _transient2;
        private readonly IDemoService _scoped1;
        private readonly IDemoService _scoped2;
        private readonly IDemoService _singleton1;
        private readonly IDemoService _singleton2;
        private readonly ILogger<LifetimeDemoService> _logger;

        public LifetimeDemoService(
            // Inject multiple instances to show lifetime behavior
            [FromKeyedServices("transient")] IDemoService transient1,
            [FromKeyedServices("transient")] IDemoService transient2,
            [FromKeyedServices("scoped")] IDemoService scoped1,
            [FromKeyedServices("scoped")] IDemoService scoped2,
            [FromKeyedServices("singleton")] IDemoService singleton1,
            [FromKeyedServices("singleton")] IDemoService singleton2,
            ILogger<LifetimeDemoService> logger)
        {
            _transient1 = transient1;
            _transient2 = transient2;
            _scoped1 = scoped1;
            _scoped2 = scoped2;
            _singleton1 = singleton1;
            _singleton2 = singleton2;
            _logger = logger;
        }

        public Dictionary<string, object> GetLifetimeInfo()
        {
            var info = new Dictionary<string, object>
            {
                ["Transient"] = new
                {
                    Instance1 = _transient1.GetInfo(),
                    Instance2 = _transient2.GetInfo(),
                    SameInstance = _transient1.GetInstanceId() == _transient2.GetInstanceId()
                        ? "❌ DIFFERENT instances (Transient)"
                        : "✅ DIFFERENT instances (Transient)"
                },
                ["Scoped"] = new
                {
                    Instance1 = _scoped1.GetInfo(),
                    Instance2 = _scoped2.GetInfo(),
                    SameInstance = _scoped1.GetInstanceId() == _scoped2.GetInstanceId()
                        ? "✅ SAME instance (Scoped)"
                        : "❌ DIFFERENT instances (Expected Scoped behavior failed)"
                },
                ["Singleton"] = new
                {
                    Instance1 = _singleton1.GetInfo(),
                    Instance2 = _singleton2.GetInfo(),
                    SameInstance = _singleton1.GetInstanceId() == _singleton2.GetInstanceId()
                        ? "✅ SAME instance (Singleton)"
                        : "❌ DIFFERENT instances (Expected Singleton behavior failed)"
                }
            };

            _logger.LogInformation("Lifetime demo executed");
            return info;
        }
    }
}