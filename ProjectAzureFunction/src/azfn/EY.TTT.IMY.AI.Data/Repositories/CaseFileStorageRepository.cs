using EY.TTT.IMY.AI.Data.DBHelpers;
using EY.TTT.IMY.AI.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Data;

namespace EY.TTT.IMY.AI.Data.Repositories
{
    public class CaseFileStorageRepository:ICaseFileStorageRepository
    {
        private readonly IDBHelper _dbHelper;
        private readonly ILogger<CaseFileStorageRepository> _logger;

        public CaseFileStorageRepository(IDBHelper dbHelper, ILogger<CaseFileStorageRepository> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        public async Task InsertSubmission(string submissionId, string caseNumber, string investorId, string filePath)
        {
            const string sql = @"
                INSERT INTO dbo.IMYAI_CaseSubmissionHistory (SubmissionId, CaseNumber, InvestorId, FilePath)
                VALUES (@SubmissionId, @CaseNumber, @InvestorId, @FilePath)";

            var parameters = new
            {
                SubmissionId = submissionId,
                CaseNumber = caseNumber,
                InvestorId = investorId,
                FilePath = filePath
            };

            await _dbHelper.ExecuteAsyncWithRetry(sql, parameters, CommandType.Text);
            _logger.LogInformation("Inserted submission: {SubmissionId}", submissionId);
        }
        public async Task<string> GenerateSubmissionId()
        {
            try
            {
                const string sql = "SELECT NEXT VALUE FOR SubmissionSequence";
                var uniqueNumber = await _dbHelper.ExecuteScalarAsyncWithRetry<int>(sql);

                var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
                var submissionId = $"IMYAI{datePart}{uniqueNumber:D5}";

                _logger.LogInformation("Generated SubmissionId: {SubmissionId}", submissionId);

                return submissionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate SubmissionId from SQL SEQUENCE.");

                // Fallback: Use timestamp-based ID
                var fallbackId = $"IMYAI{DateTime.UtcNow:yyyyMMdd}{DateTime.UtcNow:HHmmssfff}";
                _logger.LogWarning("Using fallback SubmissionId: {FallbackId}", fallbackId);

                return fallbackId;
            }
        }
        public async Task<string> GetFilePath(string submissionId, string caseNumber, string investorId)
        {
            _logger.LogInformation("Obtaining FilePath for: {SubmissionId}", submissionId);

            const string sql = @"SELECT FilePath FROM dbo.IMYAI_CaseSubmissionHistory
            WHERE SubmissionId = @SubmissionId AND CaseNumber = @CaseNumber AND InvestorId = @InvestorId";

            var parameters = new
            {
                SubmissionId = submissionId,
                CaseNumber = caseNumber,
                InvestorId = investorId
            };

            return await _dbHelper.QuerySingleOrDefaultAsyncWithRetry<string>(sql, parameters, CommandType.Text);
        }
    }
}
