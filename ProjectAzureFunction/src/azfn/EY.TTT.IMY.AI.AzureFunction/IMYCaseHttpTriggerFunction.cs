using EY.TTT.IMY.AI.Domain.Helper;
using EY.TTT.IMY.AI.Domain.Interfaces.Services;
using EY.TTT.IMY.AI.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;

namespace EY.TTT.IMY.AI.AzureFunction;

public class IMYCaseHttpTriggerFunction
{
    private readonly ILogger<IMYCaseHttpTriggerFunction> _logger;
    private readonly ICaseSubmissionService _submissionService;

    public IMYCaseHttpTriggerFunction(ILogger<IMYCaseHttpTriggerFunction> logger, ICaseSubmissionService submissionService)
    {
        _logger = logger;
        _submissionService = submissionService;
    }

    [Function("IMYCaseHttpTriggerFunction")]
    [OpenApiOperation(operationId: "SubmitIMYCase", tags: new[] { "IMYAIHttpTriggerFunction" }, Description = "Submits a new IMY case for processing.")]
    [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(IMYCaseRequestMetadataModel), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Submission successful")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(List<string>), Description = "Validation errors")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string), Description = "Submission failed")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "imyaihttptriggerfunction")] 
        HttpRequest req)
    {
        _logger.LogInformation("IMYCaseHttpTriggerFunction request started...");

        var (model, errors) = await IMYCaseValidationRequestBinder.BindAsync(req);
        if (errors.Count > 0)
        {
            _logger.LogInformation("Validation Failed. One or more required items are missing");
            foreach (var error in errors)
            {
                _logger.LogError($"Validation Error: {error}");
            }

            return new BadRequestObjectResult(errors);
        }

        var (isSuccess, message) = await _submissionService.CaseSubmission(model);
        if (!isSuccess)
        {
            _logger.LogError("Case submission failed: {ErrorMessage}", message);
            return new ObjectResult(message) { StatusCode = 500 };
        }

        _logger.LogInformation("IMYCaseHttpTriggerFunction processed successfully. Submission ID: {message}",message);
        return new OkObjectResult($"Submission successful. ID: {message}");

    }
}