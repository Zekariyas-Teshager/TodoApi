using Microsoft.AspNetCore.Mvc;

[Route("")]
[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        var html =
            @"
<!DOCTYPE html>
<html>
<head>
    <title>DI Lifetime Demo</title>
    <style>
        body { font-family: Arial; padding: 20px; }
        .card { border: 1px solid #ddd; padding: 15px; margin: 10px; border-radius: 5px; }
        .transient { background: #fff3cd; }
        .scoped { background: #d4edda; }
        .singleton { background: #cce5ff; }
        button { padding: 10px; margin: 5px; cursor: pointer; }
        pre { background: #f4f4f4; padding: 10px; border-radius: 5px; }
    </style>
</head>
<body>
    <h1>Dependency Injection Lifetime Demo</h1>
    
    <div class='card transient'>
        <h3>🟡 Transient</h3>
        <p>Created EVERY time requested - different IDs everywhere</p>
    </div>
    
    <div class='card scoped'>
        <h3>🟢 Scoped</h3>
        <p>Created ONCE per request - same IDs within request</p>
    </div>
    
    <div class='card singleton'>
        <h3>🔵 Singleton</h3>
        <p>Created ONCE for app lifetime - same IDs forever</p>
    </div>
    
    <button onclick='callApi()'>Call API</button>
    <button onclick='callMultiple()'>Call Multiple Times</button>
    
    <pre id='output'>Click buttons to see results...</pre>
    
    <script>
        async function callApi() {
            const response = await fetch('/api/demo/lifetimes');
            const data = await response.json();
            document.getElementById('output').textContent = 
                JSON.stringify(data, null, 2);
        }
        
        async function callMultiple() {
            const response = await fetch('/api/demo/lifetimes/multiple');
            const data = await response.json();
            document.getElementById('output').textContent = 
                JSON.stringify(data, null, 2);
        }
    </script>
</body>
</html>
";
        return Content(html, "text/html");
    }
}
