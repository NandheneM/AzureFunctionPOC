using Azure.Storage.Queues.Models;
using EY.TTT.IMY.AI.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;

namespace EY.TTT.IMY.AI.QueueTriggerFunction;

public class IMYAIQueueTriggerFunction
{
    private readonly ILogger<IMYAIQueueTriggerFunction> _logger;
    private readonly ICaseFileExtractionService _fileExtractionService;

    public IMYAIQueueTriggerFunction(ILogger<IMYAIQueueTriggerFunction> logger, ICaseFileExtractionService fileExtractionService)
    {
        _logger = logger;
        _fileExtractionService = fileExtractionService;
    }

    [Function(nameof(IMYAIQueueTriggerFunction))]
    public async Task<IActionResult> Run([QueueTrigger("imy-ai-case-submissions", Connection = "AzureWebJobsStorage")] QueueMessage queueMessage)
    {
        _logger.LogInformation("IMYAIQueueTriggerFunction request started...");
        _logger.LogInformation("Processing message: {messageText}", queueMessage.MessageText);

        var (isSuccess, result) = await _fileExtractionService.FileExtractionProcess(queueMessage.MessageText);
        if (!isSuccess)
        {
            _logger.LogError("File Extraction failed: {ErrorMessage}", result);
            return new ObjectResult(result) { StatusCode = 500 };
        }

        _logger.LogInformation("Queue message {messageText} processed successfully", queueMessage.MessageText);
        return new OkObjectResult(result);
    }

}