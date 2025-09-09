using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace EY.TTT.IMY.AI.Infrastructure.Models
{
    public class IMYCaseRequestModel
    {
        [Required]
        public required string InvestorId { get; set; }

        [Required]
        public required string CaseNumber { get; set; }

        [Required]
        public required IFormFile PdfFile { get; set; }
    }
    

}
