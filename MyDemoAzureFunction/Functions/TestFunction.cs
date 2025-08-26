using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MyDemoAzureFunction.Functions
{
    public class TestFunction
    {
        private readonly ILogger<HttpTriggerFunction> _logger;

        public TestFunction(ILogger<HttpTriggerFunction> logger)
        {
            _logger = logger;
        }

        [Function("TestFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
