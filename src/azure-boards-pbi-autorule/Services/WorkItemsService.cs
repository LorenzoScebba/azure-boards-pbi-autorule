using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Services.Interfaces;
using azure_boards_pbi_autorule.Utils;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace azure_boards_pbi_autorule.Services
{
    public class WorkItemsService : IWorkItemsService
    {
        private readonly IWorkItemTrackingHttpClientService _client;

        public WorkItemsService(IWorkItemTrackingHttpClientService client)
        {
            _client = client;
        }

        public async Task<WorkItem> GetWorkItemAsync(
            int id,
            IEnumerable<string> fields = null,
            DateTime? asOf = null,
            WorkItemExpand? expand = null)
        {
            try
            {
                return await _client.GetWorkItemAsync(id, fields, asOf, expand);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<WorkItem>> ListChildWorkItemsForParent(WorkItem parentWorkItem)
        {
            try
            {
                var children =
                    parentWorkItem.Relations.Where(x =>
                        x.Rel.Equals("System.LinkTypes.Hierarchy-Forward"));

                IList<int> ids = children.Select(child => AzureUtils.GetWorkItemIdFromUrl(child.Url)).ToList();

                return await _client.GetWorkItemsAsync(ids, new[] { "System.State" });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<WorkItem> UpdateWorkItemState(WorkItem workItem, string state)
        {
            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Test,
                    Path = "/rev",
                    Value = workItem.Rev.ToString()
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = state
                }
            };

            return await _client.UpdateWorkItemAsync(patchDocument, Convert.ToInt32(workItem.Id));
        }
    }
}