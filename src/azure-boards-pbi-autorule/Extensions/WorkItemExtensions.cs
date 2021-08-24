using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace azure_boards_pbi_autorule.Extensions
{
    public static class WorkItemExtensions
    {
        public static string GetWorkItemField(this WorkItem workItem, string field)
        {
            return workItem.Fields.ContainsKey(field)
                ? workItem.Fields[field].ToString()
                : string.Empty;
        }
    }
}