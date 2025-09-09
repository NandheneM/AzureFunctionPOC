using EY.TTT.IMY.AI.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Interfaces.Helper
{
    public interface IAPIHelper
    {
        Task<string> GetAccessToken();
        Task<string> RequestExtraction(string token, string fileName, byte[] file);
        Task<List<OutputFilesInfo>> GetExtractionStatus(string token, string extractionId);
        List<string> GetWithholdingStatementFileExtractNames(List<OutputFilesInfo> outputFiles);
        Task<JsonObject> GetExtractionResult(string token, string extractionId, string fileExtractNameList);
    }
}
