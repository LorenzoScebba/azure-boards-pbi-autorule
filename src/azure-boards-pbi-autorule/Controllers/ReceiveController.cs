using System.Threading.Tasks;
using azure_boards_pbi_autorule.Services.Interfaces;
using azure_boards_pbi_autorule.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace azure_boards_pbi_autorule.Controllers
{
    [Route("/api")]
    public class ReceiveController : ControllerBase
    {
        private readonly IWorkItemsService _client;
        private readonly IRulesApplierService _rulesApplierService;

        public ReceiveController(IWorkItemsService client, IRulesApplierService rulesApplierService)
        {
            _client = client;
            _rulesApplierService = rulesApplierService;
        }
        
        [HttpPost("receive")]
        public async Task<IActionResult> Index([FromBody] JObject payload)
        {
            var vm = AzureUtils.BuildPayloadViewModel(payload);
            
            Log.Debug("Received work item with id '{id}'", vm.workItemId);

            if (vm.eventType != "workitem.updated")
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", "Event type is not workitem.updated");
                return Ok("Event type is not workitem.updated");
            }

            var parentWorkItem = await _client.GetWorkItemAsync(vm.parentId, null, null, WorkItemExpand.Relations);

            if (parentWorkItem == null)
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", $"Parent work item with id '{vm.parentId}' not found");
                return Ok($"Parent work item with id '{vm.parentId}' not found");
            }

            Log.Debug("Found parent work item with id '{id}'", parentWorkItem.Id);

            var result = await _rulesApplierService.ApplyRules(vm, parentWorkItem);

            if (result.Modified)
            {
                Response.Headers.Add("x-autorule-info", result.Message);
                Response.Headers.Add("x-autorule-match", JsonConvert.SerializeObject(result.MatchedRule));
                return Ok(result.Message);
            }
            
            Log.Information("No rule matched for parent '{id}'", vm.parentId);
            
            Response.Headers.Add("x-autorule-info", result.Message);
            return Ok(result.Message);
        }
    }
}