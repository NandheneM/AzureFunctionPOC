using EY.TTT.IMY.AI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Infrastructure.AzureHelper
{
    public interface IStorageAccountHandler
    {
        Task<string> UploadPdf(IMYCaseRequestModel model, string submissionId);
        Task<byte[]> DownloadFileFromBlob(string filePath);
        Task EnqueueSubmission(string investorId, string caseNumber, string pdfName, string submissionId);
    }
}
