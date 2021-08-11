using System;
using azure_boards_pbi_autorule.Models;
using Newtonsoft.Json.Linq;

namespace azure_boards_pbi_autorule.Utils
{
    public static class AzureUtils
    {
        public static PayloadViewModel BuildPayloadViewModel(JObject body)
        {
            var vm = new PayloadViewModel();

            var url = body["resource"]["url"] == null ? null : body["resource"]["url"].ToString();
            var org = GetOrganization(url);

            vm.workItemId = body["resource"]["workItemId"] == null
                ? -1
                : Convert.ToInt32(body["resource"]["workItemId"].ToString());
            vm.workItemType = body["resource"]["revision"]["fields"]["System.WorkItemType"] == null
                ? null
                : body["resource"]["revision"]["fields"]["System.WorkItemType"].ToString();
            vm.eventType = body["eventType"] == null ? null : body["eventType"].ToString();
            vm.rev = body["resource"]["rev"] == null ? -1 : Convert.ToInt32(body["resource"]["rev"].ToString());
            vm.url = body["resource"]["url"] == null ? null : body["resource"]["url"].ToString();
            vm.organization = org;
            vm.teamProject = body["resource"]["fields"]["System.AreaPath"] == null
                ? null
                : body["resource"]["fields"]["System.AreaPath"].ToString();
            vm.state = body["resource"]["fields"]["System.State"]["newValue"] == null
                ? null
                : body["resource"]["fields"]["System.State"]["newValue"].ToString();

            return vm;
        }

        private static string GetOrganization(string url)
        {
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);

            if (url.Contains("visualstudio.com"))
            {
                var split = url.Split('.');
                return split[0];
            }

            if (url.Contains("dev.azure.com"))
            {
                url = url.Replace("dev.azure.com/", string.Empty);
                var split = url.Split('/');
                return split[0];
            }

            return string.Empty;
        }
        
        public static int GetWorkItemIdFromUrl(string url)
        {
            var lastIndexOf = url.LastIndexOf("/");
            var size = url.Length - (lastIndexOf + 1);

            var value = url.Substring(lastIndexOf + 1, size);

            return Convert.ToInt32(value);
        }
    }
}