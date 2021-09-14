using System.Threading.Tasks;
using azure_boards_pbi_autorule.Services.Interfaces;
using azure_boards_pbi_autorule.Utils;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost("state")]
        public async Task<IActionResult> State([FromBody] JObject payload)
        {
            var vm = AzureUtils.BuildUpdatedPayloadViewModel(payload);

            Log.Debug("Received work item with id '{id}'", vm.workItemId);

            if (vm.eventType != "workitem.updated")
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", "Event type is not workitem.updated");
                return Ok("Event type is not workitem.updated");
            }

            if (!_rulesApplierService.HasStateRuleForType(vm.workItemType))
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", $"No rule is configured for type {vm.workItemType}");
                return Ok($"No rule is configured for type {vm.workItemType}");
            }

            var result = await _rulesApplierService.ApplyStateRules(vm);

            if (!result.HasError)
            {
                Response.Headers.Add("x-autorule-info", $"{result.Data.Target} updated with {result.Data.TargetRule}");
                Response.Headers.Add("x-autorule-match", JsonConvert.SerializeObject(result.Data));
                Log.Information($"{result.Data.Target} updated with {result.Data.TargetRule}");
                return Ok($"{result.Data.Target} updated with {result.Data.TargetRule}");
            }

            Log.Information(result.Error);
            Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
            Response.Headers.Add("x-autorule-info", result.Error);
            return Ok(result.Error);
        }

        [HttpPost("area")]
        public async Task<IActionResult> Area([FromBody] JObject payload)
        {
            var vm = AzureUtils.BuildCreatedPayloadViewModel(payload);
            
            if (vm.eventType != "workitem.created")
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", "Event type is not workitem.craeted");
                return Ok("Event type is not workitem.created");
            }
            
            if (!_rulesApplierService.HasAreaRuleForType(vm.workItemType))
            {
                Response.Headers.Add("Warning", "No work done, check logs or x-autorule-info header for more info");
                Response.Headers.Add("x-autorule-info", $"No rule is configured for type {vm.workItemType}");
                return Ok($"No rule is configured for type {vm.workItemType}");
            }
            
            return Ok();
        }
    }
}