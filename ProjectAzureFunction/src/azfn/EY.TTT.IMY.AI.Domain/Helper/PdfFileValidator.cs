using EY.TTT.IMY.AI.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Helper
{
    public class PdfFileValidator
    {
        private const long MaxPdfSizeInBytes = 5 * 1024 * 1024; // 5 MB
        public List<ValidationResult> Validate(IMYCaseRequestModel model)
        {
            var errors = new List<ValidationResult>();

            if (model.PdfFile == null || model.PdfFile.Length == 0)
            {
                errors.Add(new ValidationResult("PDF file is missing or empty."));
            }
            else
            {
                if (model.PdfFile.ContentType != "application/pdf")
                {
                    errors.Add(new ValidationResult("Uploaded file must be a PDF."));
                }

                if (model.PdfFile.Length > MaxPdfSizeInBytes)
                {
                    errors.Add(new ValidationResult($"PDF file size must not exceed {MaxPdfSizeInBytes / (1024 * 1024)} MB."));
                }
            }

            return errors;
        }
        public static bool IsValidPdf(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length < 5)
                return false;

            try
            {
                string header = Encoding.ASCII.GetString(fileBytes, 0, 5);
                return header == "%PDF-";  
            }
            catch
            {
                return false;
            }
        }
    }
}
