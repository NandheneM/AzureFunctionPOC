using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Interfaces.Services
{
    public interface ICaseFileExtractionService
    {
        Task<(bool isSuccess, object result)> FileExtractionProcess(string QueueMessage);
    }
}
