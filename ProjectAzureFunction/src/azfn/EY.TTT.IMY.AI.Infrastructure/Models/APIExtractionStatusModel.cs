using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Infrastructure.Models
{
    public class APIExtractionStatusModel
    {
        public string registeredAt { get; set; }
        public string updatedAt { get; set; }
        public string status { get; set; }
        public List<OutputFilesInfo> outputFiles { get; set; }
        public string statusMessage { get; set; }
    }
    public class OutputFilesInfo
    {
        public string fileType { get; set; }
        public string fileName { get; set; }
    }
}
