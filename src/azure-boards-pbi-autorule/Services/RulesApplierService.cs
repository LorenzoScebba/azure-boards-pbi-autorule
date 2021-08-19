using System.Linq;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Serilog;

namespace azure_boards_pbi_autorule.Services
{
    public class RulesApplierService : IRulesApplierService
    {
        private readonly IWorkItemsService _client;
        private readonly RuleConfiguration _rules;

        public RulesApplierService(IWorkItemsService client, RuleConfiguration rules)
        {
            _client = client;
            _rules = rules;
        }

        public async Task<Result<Rule, string>> ApplyRules(AzureWebHookModel vm, WorkItem parentWorkItem)
        {
            var parentState = parentWorkItem.Fields["System.State"] == null
                ? string.Empty
                : parentWorkItem.Fields["System.State"].ToString();

            var childWorkItems = await _client.ListChildWorkItemsForParent(parentWorkItem);
            
            foreach (var rule in _rules.Rules)
            {
                if (rule.IfChildState.Equals(vm.state))
                {
                    if (!rule.AllChildren)
                    {
                        if (!rule.NotParentStates.Contains(parentState))
                        {
                            Log.Information("Updating '{id}' with {state}", parentWorkItem.Id, rule.SetParentStateTo);

                            try
                            {
                                await _client.UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                                return Result<Rule, string>.Ok(rule);
                            }
                            catch (RuleValidationException e)
                            {
                                return Result<Rule, string>.Fail($"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                            }
                        }
                    }
                    else
                    {
                        // check to see if any of the child items are not closed, if so, we will get a count > 0
                        var count = childWorkItems
                            .Where(x => !x.Fields["System.State"].ToString().Equals(rule.IfChildState)).ToList().Count;

                        if (count.Equals(0))
                        {
                            Log.Information("Updating '{id}' with {state}", parentWorkItem.Id, rule.SetParentStateTo);

                            try
                            {
                                await _client.UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                                return Result<Rule, string>.Ok(rule);
                            }
                            catch (RuleValidationException e)
                            {
                                return Result<Rule, string>.Fail($"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                            }
                        }
                    }
                }
            }

            return Result<Rule, string>.Fail("No rule matched");
        }
    }
}