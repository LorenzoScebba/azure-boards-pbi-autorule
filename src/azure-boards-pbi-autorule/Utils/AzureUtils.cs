using System;
using azure_boards_pbi_autorule.Models;
using Newtonsoft.Json.Linq;

namespace azure_boards_pbi_autorule.Utils
{
    public static class AzureUtils
    {
        public static AzureWebHookModel BuildPayloadViewModel(JObject body)
        {
            return new AzureWebHookModel
            {
                eventType = body["eventType"]?.ToString(),
                
                workItemId = body["resource"]["workItemId"] == null
                    ? -1
                    : Convert.ToInt32(body["resource"]["workItemId"].ToString()),
                parentId = body["resource"]["revision"]?["fields"]?["System.Parent"] == null
                    ? -1
                    : Convert.ToInt32(body["resource"]["revision"]?["fields"]?["System.Parent"]),
                
                workItemType = body["resource"]["revision"]?["fields"]?["System.WorkItemType"]?.ToString(),
                state = body["resource"]["fields"]?["System.State"]?["newValue"]?.ToString()
            };
        }

        public static int GetWorkItemIdFromUrl(string url)
        {
            var lastIndexOf = url.LastIndexOf("/", StringComparison.Ordinal);
            var size = url.Length - (lastIndexOf + 1);

            var value = url.Substring(lastIndexOf + 1, size);

            return Convert.ToInt32(value);
        }
    }
}