using EY.TTT.IMY.AI.Domain.Helper;
using EY.TTT.IMY.AI.Domain.Interfaces.Helper;
using EY.TTT.IMY.AI.Domain.Interfaces.Repositories;
using EY.TTT.IMY.AI.Domain.Interfaces.Services;
using EY.TTT.IMY.AI.Infrastructure.AzureHelper;
using EY.TTT.IMY.AI.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EY.TTT.IMY.AI.Domain.Services
{
    public class CaseFileExtractionService:ICaseFileExtractionService
    {
        private readonly ILogger<CaseFileExtractionService> _logger;
        private readonly ICaseFileStorageRepository _dbrepository;
        private readonly IStorageAccountHandler _accHandler;
        private readonly IAPIHelper _apiHelper;
        public CaseFileExtractionService(ILogger<CaseFileExtractionService> logger, ICaseFileStorageRepository dbrepository, IStorageAccountHandler accHandler, IAPIHelper apiHelper)
        {
            _logger = logger;
            _dbrepository = dbrepository;
            _apiHelper = apiHelper;
            _accHandler = accHandler;
        }

        public async Task<(bool isSuccess, object result)> FileExtractionProcess(string QueueMessage)
        {
            try
            {
                var token = await _apiHelper.GetAccessToken();
                if (token == null)
                {
                    _logger.LogError("Invalid token.");
                    throw new InvalidOperationException("Invalid token.");
                }

                QueueMessageModel fileDetails = JsonSerializer.Deserialize<QueueMessageModel>(QueueMessage);

                if (fileDetails == null)
                {
                    _logger.LogError("Invalid queue message format.");
                    throw new InvalidOperationException("Invalid queue message format.");
                }

                QueueTriggerResponseModel QueueResponse = new QueueTriggerResponseModel();
                QueueResponse.CaseNumber = fileDetails.CaseNumber;

                var FilePath = await _dbrepository.GetFilePath(fileDetails.SumbissionId, fileDetails.CaseNumber, fileDetails.InvestorId);
                QueueResponse.FilePath = FilePath;

                var File = await _accHandler.DownloadFileFromBlob(FilePath);

                if (File == null || File.Length == 0 || !PdfFileValidator.IsValidPdf(File))
                {
                    _logger.LogError("Invalid file.");
                    throw new InvalidOperationException("Invalid file.");
                }

                var extractionId = await _apiHelper.RequestExtraction(token, fileDetails.PdfName, File);

                List<OutputFilesInfo> outputFiles = await _apiHelper.GetExtractionStatus(token, extractionId);

                List<string> withholdingStatementFileExtractNameList = _apiHelper.GetWithholdingStatementFileExtractNames(outputFiles);

                FormsExtractionModel formsExtraction = new FormsExtractionModel();
                List<FormsModel> withholdingStatementsList = new List<FormsModel>();

                foreach (var fileExtractName in withholdingStatementFileExtractNameList)
                {
                    FormsModel withholdingStatement = new FormsModel();
                    withholdingStatement.FormName = fileExtractName;

                    var extractionResult = await _apiHelper.GetExtractionResult(token, extractionId, fileExtractName);
                    if (extractionResult == null)
                    {
                        _logger.LogError("Invalid file extraction result.");
                        throw new InvalidOperationException("Invalid file extraction result.");
                    }
                    HashSet<int> PageNumbers = PageNumberExtractor.ExtractFilePageNumbers(extractionResult);

                    withholdingStatement.FormPages = PageNumbers.ToArray(); 
                    withholdingStatementsList.Add(withholdingStatement);
                }

                formsExtraction.FormList = withholdingStatementsList;
                QueueResponse.FormExtractionDetails = formsExtraction;

                return (true, QueueResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during FileExtractionProcess");
                return (false, $"Error: {ex.Message}");
            }
        }

    }
}
