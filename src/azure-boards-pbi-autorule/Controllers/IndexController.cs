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