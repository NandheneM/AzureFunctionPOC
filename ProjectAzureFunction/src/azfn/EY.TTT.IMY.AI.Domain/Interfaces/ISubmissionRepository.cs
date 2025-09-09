using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Interfaces
{
    public interface ISubmissionRepository
    {
        Task InsertSubmissionAsync(string submissionId, string caseNumber, string investorId, string filePath);
        Task<string> GenerateSubmissionIdAsync();
    }
}
