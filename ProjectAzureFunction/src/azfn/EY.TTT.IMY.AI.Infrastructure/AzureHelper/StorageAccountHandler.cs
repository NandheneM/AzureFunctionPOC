using Azure.Storage.Queues;
using Azure.Storage.Blobs;
using EY.TTT.IMY.AI.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace EY.TTT.IMY.AI.Infrastructure.AzureHelper
{
    public class StorageAccountHandler: IStorageAccountHandler
    {
        private readonly ILogger<StorageAccountHandler> _logger;
        private readonly QueueClient _queueClient;
        private readonly IConfiguration _configuration;
        private readonly BlobServiceClient _blobServiceClient;
        public StorageAccountHandler(ILogger<StorageAccountHandler> logger, QueueClient queueClient, IConfiguration configuration, BlobServiceClient blobServiceClient)
        {
            _logger = logger;
            _queueClient = queueClient;
            _configuration = configuration;
            _blobServiceClient = blobServiceClient;
        }

        public async Task EnqueueSubmission(string investorId, string caseNumber, string pdfName, string submissionId)
        {
            try
            {
                var payload = new
                {
                    InvestorId = investorId,
                    CaseNumber = caseNumber,
                    PdfName = pdfName,
                    SumbissionId = submissionId
                };

                string message = System.Text.Json.JsonSerializer.Serialize(payload);
                await _queueClient.SendMessageAsync(message);

                _logger.LogInformation("Enqueued submission: {Payload}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue submission for CaseNumber: {CaseNumber}, InvestorId: {InvestorId}", caseNumber, investorId);
            }
        }
        public async Task<string> UploadPdf(IMYCaseRequestModel model, string submissionId)
        {
            try
            {
                var containerName = _configuration["Values:BlobContainerName"];
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var folderName = $"{model.InvestorId}_{model.CaseNumber}";
                var fileName = $"{Path.GetFileNameWithoutExtension(model.PdfFile.FileName)}_{submissionId}.pdf";
                var blobName = $"{folderName}/{fileName}";
                var blobClient = containerClient.GetBlobClient(blobName);

                using var stream = model.PdfFile.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: false);

                _logger.LogInformation("PDF uploaded to blob storage: {BlobUri}", blobClient.Uri);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload PDF for CaseNumber: {CaseNumber}, InvestorId: {InvestorId}", model.CaseNumber, model.InvestorId);
                throw;
            }
        }
        public async Task<byte[]> DownloadFileFromBlob(string filePath)
        {
            var parts = filePath.Split('/', 2);
            var containerName = parts[0];
            var blobPath = parts[1];

            try
            {
                var blobServiceClient = new BlobServiceClient(_configuration["AzureWebJobsStorage"]);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(Uri.UnescapeDataString(blobPath));

                var blobStream = await blobClient.OpenReadAsync();
                using (var memoryStream = new MemoryStream())
                {
                    await blobStream.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading blob from path: {FilePath}", filePath);
                throw;
            }
        }

    }
}
