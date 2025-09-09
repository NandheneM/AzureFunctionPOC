using EY.TTT.IMY.AI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Interfaces.Services
{
    public interface ICaseSubmissionService
    {
        Task<(bool isSuccess, string message)> CaseSubmission(IMYCaseRequestModel model);
    }
}
