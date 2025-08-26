using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace MyDemoAzureFunction.Functions
{
    public class ProcessQueueFunction
    {
        private readonly ILogger<ProcessQueueFunction> _logger;

        public ProcessQueueFunction(ILogger<ProcessQueueFunction> logger)
        {
            _logger = logger;
        }

        [Function("ProcessQueueFunction")]
        public void Run(
            [QueueTrigger("case-queue", Connection = "AzureWebJobsStorage")] string message)
        {
            _logger.LogInformation($"Processing message: {message}");
        }
    }
}
