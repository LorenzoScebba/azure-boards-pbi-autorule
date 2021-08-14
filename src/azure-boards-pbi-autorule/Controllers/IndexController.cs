using System.Linq;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services.Interfaces;
using azure_boards_pbi_autorule.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace azure_boards_pbi_autorule.Controllers
{
    [Route("/api")]
    public class IndexController : ControllerBase
    {
        private readonly IWorkItemsService _client;
        private readonly IRulesApplierService _rulesApplierService;
        private readonly ILogger<IndexController> _logger;

        public IndexController(IWorkItemsService client, IRulesApplierService rulesApplierService, ILogger<IndexController> logger)
        {
            _client = client;
            _rulesApplierService = rulesApplierService;
            _logger = logger;
        }
        
        [HttpPost("receive")]
        public async Task<IActionResult> Index([FromBody] JObject payload)
        {
            var vm = AzureUtils.BuildPayloadViewModel(payload);
            
            _logger.Log(LogLevel.Information, $"Received work item with id {vm.workItemId}");

            if (vm.eventType != "workitem.updated")
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", "Event type is not workitem.updated");
                return Ok();
            }

            var workItem = await _client.GetWorkItemAsync(vm.workItemId, null, null, WorkItemExpand.Relations);

            if (workItem == null)
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", $"Workitem with id '{vm.workItemId}' not found");
                return Ok();
            }

            var parentRelation = workItem.Relations.FirstOrDefault(x => x.Rel.Equals("System.LinkTypes.Hierarchy-Reverse"));

            if (parentRelation == null)
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", $"No parent found for work item '{vm.workItemId}'");
                return Ok();
            }

            var parentId = AzureUtils.GetWorkItemIdFromUrl(parentRelation.Url);
            
            var parentWorkItem = await _client.GetWorkItemAsync(parentId, null, null, WorkItemExpand.Relations);

            if (parentWorkItem == null)
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", $"Parent work item with id '{parentId}' not found");
                return Ok($"Parent work item with id {parentId} not found");
            }

            _logger.Log(LogLevel.Information, $"Found parent work item with id {parentWorkItem.Id}");

            var result = await _rulesApplierService.ApplyRules(vm, parentWorkItem);

            if (result.Modified)
            {
                Response.Headers.Add("x-autorule-info", result.Message);
                Response.Headers.Add("x-autorule-match", JsonConvert.SerializeObject(result.MatchedRule));
                return Ok();
            }
            
            _logger.Log(LogLevel.Information, $"No rule matched for parent '{parentId}'");
            
            Response.Headers.Add("x-autorule-info", result.Message);
            return Ok();
        }
    }
}