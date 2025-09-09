using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Interfaces.Repositories
{
    public interface ICaseFileStorageRepository
    {
        Task<string> GetFilePath(string submissionId, string caseNumber, string investorId);
        Task InsertSubmission(string submissionId, string caseNumber, string investorId, string filePath);
        Task<string> GenerateSubmissionId();
    }
}
