using EY.TTT.IMY.AI.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EY.TTT.IMY.AI.Domain.Helper
{
    public static class IMYCaseValidationRequestBinder
    {
        public static async Task<(IMYCaseRequestModel model, 
            List<ValidationResult> errors)> BindAsync(HttpRequest req)
        {
            var form = await req.ReadFormAsync();

            var model = new IMYCaseRequestModel
            {
                InvestorId = form["InvestorId"],
                CaseNumber = form["CaseNumber"],
                PdfFile = form.Files["PdfFile"]
            };

            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);

            var fileValidator = new PdfFileValidator();
            var domainErrors = fileValidator.Validate(model);
            results.AddRange(domainErrors);

            return (model, results);
        }
    }
}
