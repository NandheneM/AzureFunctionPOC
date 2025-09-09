using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using EY.TTT.IMY.AI.Domain.Interfaces;
using EY.TTT.IMY.AI.Domain.Interfaces.Repositories;
using EY.TTT.IMY.AI.Domain.Interfaces.Services;
using EY.TTT.IMY.AI.Infrastructure.AzureHelper;
using EY.TTT.IMY.AI.Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace EY.TTT.IMY.AI.Domain.Services
{
    public class CaseSubmissionService : ICaseSubmissionService
    {
        private readonly ICaseFileStorageRepository _dbrepository;
        private readonly IStorageAccountHandler _accHandler;
        private readonly ILogger<CaseSubmissionService> _logger;

        public CaseSubmissionService(ICaseFileStorageRepository dbrepository, ILogger<CaseSubmissionService> logger, IStorageAccountHandler accHandler)
        {
            _dbrepository = dbrepository;
            _logger = logger;
            _accHandler = accHandler;
        }

        public async Task<(bool isSuccess, string message)> CaseSubmission(IMYCaseRequestModel model)
        {

            string submissionId = string.Empty;
            string filePath = string.Empty;

            try
            {
                var pdfName = model.PdfFile.FileName;

                submissionId = await _dbrepository.GenerateSubmissionId();

                var uploadedPath = await _accHandler.UploadPdf(model, submissionId); 
                filePath = GetTrimmedFilePath(uploadedPath);

                await _dbrepository.InsertSubmission(submissionId, model.CaseNumber, model.InvestorId, filePath);

                await _accHandler.EnqueueSubmission(model.InvestorId, model.CaseNumber, pdfName, submissionId); 

                return (true, submissionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during case submission for CaseNumber: {CaseNumber}, InvestorId: {InvestorId}", model.CaseNumber, model.InvestorId);
                return (false, $"Error: {ex.Message}");
            }
        }

        public string GetTrimmedFilePath(string filePath)
        {
            Uri uri = new Uri(filePath);
            string blobPath = uri.AbsolutePath.TrimStart('/');
            return blobPath;
        }
    }
}
