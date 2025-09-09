using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Infrastructure.Models
{
    public class QueueTriggerResponseModel
    {
        [Required]
        [JsonProperty("case_id")]
        public string CaseNumber { get; set; }
        [Required]
        [JsonProperty("pdf_blob_path")]
        public string FilePath { get; set; }
        [Required]
        [JsonProperty("omniform_extraction")]
        public FormsExtractionModel FormExtractionDetails { get; set; }
    }
    public class FormsExtractionModel
    {
        [JsonProperty("forms")]
        public List<FormsModel> FormList {  get; set; } = new List<FormsModel>();
    }
    public class FormsModel
    {
        [JsonProperty("form_name")]
        public string FormName { get; set; }
        [JsonProperty("form_pages")]
        public int[] FormPages { get; set; }
    }
}
