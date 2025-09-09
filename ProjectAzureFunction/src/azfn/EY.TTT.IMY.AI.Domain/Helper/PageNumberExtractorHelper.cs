using EY.TTT.IMY.AI.Domain.APIConstants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace EY.TTT.IMY.AI.Domain.Helper
{
    public static class PageNumberExtractor
    {
        public static HashSet<int> ExtractFilePageNumbers(JsonObject jsonObject)
        {
            var pageNumbers = new HashSet<int>();

            if (jsonObject.TryGetPropertyValue(FileExtractionConstants.JSONObjForm, out JsonNode? formSpecificsNode) &&
                formSpecificsNode is JsonArray formSpecificsArray)
            {
                foreach (var item in formSpecificsArray)
                {
                    if (item is JsonObject obj &&
                        obj.TryGetPropertyValue(FileExtractionConstants.FilePageNumberIndicator, out JsonNode? pageNode) &&
                        pageNode is JsonValue val &&
                        val.TryGetValue<int>(out int page))
                    {
                        pageNumbers.Add(page);
                    }
                }
            }
            return pageNumbers;
        }
    }
}
