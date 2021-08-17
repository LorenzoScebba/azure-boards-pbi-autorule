using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace azure_boards_pbi_autorule.Services.Interfaces
{
    public interface IWorkItemTrackingHttpClientService
    {
        Task<WorkItem> GetWorkItemAsync(
            int id,
            IEnumerable<string> fields = null,
            DateTime? asOf = null,
            WorkItemExpand? expand = null);

        Task<IEnumerable<WorkItem>> GetWorkItemsAsync(
            IEnumerable<int> ids,
            IEnumerable<string> fields = null);

        Task<WorkItem> UpdateWorkItemAsync(
            JsonPatchDocument document, 
            int id);
    }
}