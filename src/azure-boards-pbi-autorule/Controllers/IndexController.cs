using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json.Linq;

namespace azure_boards_pbi_autorule.Controllers
{
    [Route("/api")]
    public class IndexController : ControllerBase
    {
        private readonly WorkItemTrackingHttpClient _client;
        private readonly ILogger<IndexController> _logger;
        private readonly RuleConfiguration _rules;

        public IndexController(WorkItemTrackingHttpClient client, ILogger<IndexController> logger, RuleConfiguration rules)
        {
            _client = client;
            _logger = logger;
            _rules = rules;
        }
        
        [HttpPost("receive")]
        public async Task<IActionResult> Index([FromBody] JObject payload)
        {
            var vm = AzureUtils.BuildPayloadViewModel(payload);
            
            _logger.Log(LogLevel.Information, $"Received work item with id {vm.workItemId}");

            if (vm.eventType != "workitem.updated")
                return UnprocessableEntity("Event type is not workitem.updated");
            
            var workItem = await _client.GetWorkItemAsync(vm.workItemId, null, null, WorkItemExpand.Relations);

            if (workItem == null)
                return NotFound($"Workitem with id {vm.workItemId} not found");
            
            var parentRelation = workItem.Relations.FirstOrDefault(x => x.Rel.Equals("System.LinkTypes.Hierarchy-Reverse"));

            if (parentRelation == null)
                return Ok("No parent found, workitem is not a task");
            
            var parentId = AzureUtils.GetWorkItemIdFromUrl(parentRelation.Url);
            
            var parentWorkItem = await _client.GetWorkItemAsync(parentId, null, null, WorkItemExpand.Relations);
            
            if (parentWorkItem == null)
                return NotFound($"Parent work item with id {parentId} not found");
            
            _logger.Log(LogLevel.Information, $"Found parent work item with id {parentId}");

            var parentState = parentWorkItem.Fields["System.State"] == null
                ? string.Empty
                : parentWorkItem.Fields["System.State"].ToString();

            foreach (var rule in _rules.Rules)
            {
                if (rule.IfChildState.Equals(vm.state))
                {
                    if (!rule.AllChildren)
                    {
                        if (!rule.NotParentStates.Contains(parentState))
                        {
                            _logger.Log(LogLevel.Information, $"Updating '{parentId}' with {rule.SetParentStateTo}");

                            await UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                            return Ok($"Parent updated with {rule.SetParentStateTo}");
                        }
                    }
                    else
                    {
                        // get a list of all the child items to see if they are all closed or not
                        var childWorkItems =
                            await ListChildWorkItemsForParent(parentWorkItem);

                        // check to see if any of the child items are not closed, if so, we will get a count > 0
                        var count = childWorkItems
                            .Where(x => !x.Fields["System.State"].ToString().Equals(rule.IfChildState)).ToList().Count;

                        if (count.Equals(0))
                        {
                            _logger.Log(LogLevel.Information, $"Updating '{parentId}' with {rule.SetParentStateTo}");

                            await UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                            return Ok($"Parent updated with {rule.SetParentStateTo}");
                        }


                        return Ok("Nothing to do");
                    }
                }
            }

            _logger.Log(LogLevel.Information, $"No rule matched for parent '{parentId}'");

            return Ok("what the hell are we doing here");
        }
        
        private async Task<WorkItem> UpdateWorkItemState(WorkItem workItem, string state)
        {
            var patchDocument = new JsonPatchDocument();

            patchDocument.Add(
                new JsonPatchOperation
                {
                    Operation = Operation.Test,
                    Path = "/rev",
                    Value = workItem.Rev.ToString()
                }
            );

            patchDocument.Add(
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = state
                }
            );

            return await _client.UpdateWorkItemAsync(patchDocument, Convert.ToInt32(workItem.Id));
        }

        private async Task<List<WorkItem>> ListChildWorkItemsForParent(WorkItem parentWorkItem)
        {
            // get all the related child work item links
            var children =
                parentWorkItem.Relations.Where(x =>
                    x.Rel.Equals("System.LinkTypes.Hierarchy-Forward"));
            IList<int> ids = new List<int>();

            // loop through children and extract the id's the from the url
            foreach (var child in children) ids.Add(AzureUtils.GetWorkItemIdFromUrl(child.Url));

            // in this case we only care about the state of the child work items
            string[] fields = { "System.State" };

            // go get the full list of child work items with the desired fields
            return await _client.GetWorkItemsAsync(ids, fields);
        }
    }
}