using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using JsonPatchDocument = Microsoft.VisualStudio.Services.WebApi.Patch.Json.JsonPatchDocument;

namespace azure_boards_pbi_autorule.Services
{
    /// <summary>
    /// A simple wrapper around the WorkItemTrackingHttpClient for testing porpouses
    /// </summary>
    public class WorkItemTrackingHttpClientService : IWorkItemTrackingHttpClientService
    {
        private readonly WorkItemTrackingHttpClient _client;

        public WorkItemTrackingHttpClientService(WorkItemTrackingHttpClient client)
        {
            _client = client;
        }

        public async Task<WorkItem> GetWorkItemAsync(
            int id,
            IEnumerable<string> fields = null,
            DateTime? asOf = null,
            WorkItemExpand? expand = null)
        {
            return await _client.GetWorkItemAsync(id, fields, asOf, expand);
        }
        
        public async Task<IEnumerable<WorkItem>> GetWorkItemsAsync(
            IEnumerable<int> ids,
            IEnumerable<string> fields = null)
        {
            return await _client.GetWorkItemsAsync(ids, fields);
        }
        
        public async Task<WorkItem> UpdateWorkItemAsync(
            JsonPatchDocument document, 
            int id)
        {
            return await _client.UpdateWorkItemAsync(document, id);
        }
    }
}