using Microsoft.AspNetCore.Mvc;
using TodoApi.Services.Interfaces;

namespace TodoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        private readonly ILifetimeDemoService _lifetimeDemoService;
        private readonly IDemoService _transientDirect;
        private readonly IDemoService _scopedDirect;
        private readonly IDemoService _singletonDirect;
        private readonly ILogger<DemoController> _logger;

        public DemoController(
            [FromKeyedServices("transient")] IDemoService transientDirect,
            [FromKeyedServices("scoped")] IDemoService scopedDirect,
            [FromKeyedServices("singleton")] IDemoService singletonDirect,
            ILifetimeDemoService lifetimeDemoService,
            ILogger<DemoController> logger)
        {
            _transientDirect = transientDirect;
            _scopedDirect = scopedDirect;
            _singletonDirect = singletonDirect;
            _lifetimeDemoService = lifetimeDemoService;
            _logger = logger;
        }

        [HttpGet("lifetimes")]
        public IActionResult ShowLifetimes()
        {
            _logger.LogInformation("=== LIFETIME DEMO START ===");
            
            var result = new
            {
                Message = "Call this endpoint multiple times to see lifetime behavior",
                DirectInjections = new
                {
                    TransientDirect = _transientDirect.GetInfo(),
                    ScopedDirect = _scopedDirect.GetInfo(),
                    SingletonDirect = _singletonDirect.GetInfo()
                },
                FromServiceConsumer = _lifetimeDemoService.GetLifetimeInfo(),
                Explanation = new
                {
                    Transient = "🟡 Transient: Each instance has DIFFERENT IDs - even within same request",
                    Scoped = "🟢 Scoped: Instances have SAME ID within this request",
                    Singleton = "🔵 Singleton: SAME ID across ALL requests (app lifetime)"
                },
                NextSteps = "Try refreshing this page multiple times and watch the console logs!"
            };

            _logger.LogInformation("=== LIFETIME DEMO END ===");
            return Ok(result);
        }

        [HttpGet("lifetimes/multiple")]
        public async Task<IActionResult> ShowMultipleRequests()
        {
            // Simulate multiple service calls within same request
            var tasks = new List<Task<Dictionary<string, object>>>();
            
            for (int i = 0; i < 3; i++)
            {
                tasks.Add(Task.Run(() => _lifetimeDemoService.GetLifetimeInfo()));
            }

            var results = await Task.WhenAll(tasks);
            
            return Ok(new
            {
                Message = "Multiple parallel operations within same request",
                Results = results,
                Observation = new
                {
                    Transient = "All transient instances will be DIFFERENT across all parallel tasks",
                    Scoped = "All scoped instances will be SAME across all parallel tasks",
                    Singleton = "All singleton instances will be SAME across all parallel tasks"
                }
            });
        }
    }
}