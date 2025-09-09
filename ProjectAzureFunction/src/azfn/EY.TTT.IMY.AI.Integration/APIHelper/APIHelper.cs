using EY.TTT.IMY.AI.Domain.Interfaces.Helper;
using EY.TTT.IMY.AI.Infrastructure.Models;
using EY.TTT.IMY.AI.Integration.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EY.TTT.IMY.AI.Integration.APIHelper
{
    public class APIHelper: IAPIHelper
    {
        private readonly ILogger<APIHelper> _logger;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public APIHelper(ILogger<APIHelper> logger, IConfiguration configuration, HttpClient httpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _baseUrl = _configuration.GetValue<string>("OmniFormsURLs:BaseUrl", string.Empty);
        }
        public async Task<string> GetAccessToken()
        {
            var form = new Dictionary<string, string>
            {
                { "grant_type", AccessTokenConstants.GrantType },
                { "client_id", AccessTokenConstants.ClientId },
                { "client_secret", AccessTokenConstants.ClientSecret },
                { "scope", AccessTokenConstants.Scope }
            };
            var tokenUrl = _configuration["OmniFormsURLs:TokenUrl"];

            try
            {
                var response = await _httpClient.PostAsync(tokenUrl, new FormUrlEncodedContent(form));
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var token = JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString();
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain Access Token");
                throw;
            }
        }
        public async Task<string> RequestExtraction(string token, string fileName, byte[] file)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/Tdd.Interactive.Api/extraction");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var form = new MultipartFormDataContent
                {
                    { new StringContent(ExtractionConstants.ExtractionScope), "extractionScope" },
                    { new ByteArrayContent(file) { Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") } }, "file", fileName }
                };
                request.Content = form;

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var extractionId = JsonDocument.Parse(json).RootElement.GetProperty("extractionId").GetString();
                return extractionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequestExtraction failed");
                throw;
            }
        }
        public async Task<List<OutputFilesInfo>> GetExtractionStatus(string token, string extractionId)
        {
            int pollingRetryCountTracker = 0;
            int pollingDelay = ExtractionStatusConstants.InitialPollingDelay;
            APIExtractionStatusModel extractionStatus = null;

            while (pollingRetryCountTracker < ExtractionStatusConstants.MaxPollingRetryCount)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/Tdd.Interactive.Api/extraction?extractionId={extractionId}");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    request.Headers.Add(ExtractionStatusConstants.RaptorAppIdHeaderKey, ExtractionStatusConstants.RaptorAppIdHeaderValue);

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    extractionStatus = JsonSerializer.Deserialize<APIExtractionStatusModel>(json);

                    if (extractionStatus?.outputFiles?.Count > 0 && extractionStatus.status == "Completed")
                    {
                        _logger.LogInformation("Output received: {@OutputFiles}", extractionStatus.outputFiles);
                        return extractionStatus.outputFiles;
                    }

                    _logger.LogInformation("Status: {Status}. Retrying in {Delay}ms...", extractionStatus?.status, pollingDelay);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to obtain File Extraction Status");
                }

                pollingRetryCountTracker++;
                await Task.Delay(pollingDelay);
                pollingDelay = Math.Min(pollingDelay * 2, ExtractionStatusConstants.MaxPollingDelay); // exponential backoff
            }

            throw new InvalidOperationException("Unexpected exit from GetExtractionStatus polling loop.");
        }
        public List<string> GetWithholdingStatementFileExtractNames(List<OutputFilesInfo> outputFiles)
        {
            List<string> fileExtractNames = new List<string>();
            foreach (var outputFile in outputFiles)
            {
                if (outputFile.fileName.Contains(ExtractionConstants.WithHoldingStatementsIndicator))
                    fileExtractNames.Add(outputFile.fileName);
            }

            return fileExtractNames;

        }
        public async Task<JsonObject> GetExtractionResult(string token, string extractionId, string fileExtractName)
        {
            var url = $"{_baseUrl}/Tdd.Interactive.Api/extractionResult?path=output/{extractionId}/{fileExtractName}&extractionId={extractionId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode is JsonObject jsonObject)
                {
                    return jsonObject;
                }

                throw new InvalidOperationException("Response is not a JSON object.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain File Extraction Result");
                throw;
            }
        }

    }
}
