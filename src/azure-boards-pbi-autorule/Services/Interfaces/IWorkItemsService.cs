using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace azure_boards_pbi_autorule.Services.Interfaces
{
    public interface IWorkItemsService
    {
        Task<WorkItem> GetWorkItemAsync(int id,
            IEnumerable<string> fields = null,
            DateTime? asOf = null,
            WorkItemExpand? expand = null);

        Task<IEnumerable<WorkItem>> ListChildWorkItemsForParent(WorkItem parentWorkItem);
        Task<WorkItem> UpdateWorkItemState(WorkItem workItem, string state);
    }
}