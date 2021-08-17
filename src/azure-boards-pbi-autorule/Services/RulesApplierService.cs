using System.Linq;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.Extensions.Logging;
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

        public async Task<RuleResult> ApplyRules(AzureWebHookModel vm, WorkItem parentWorkItem)
        {
            var parentState = parentWorkItem.Fields["System.State"] == null
                ? string.Empty
                : parentWorkItem.Fields["System.State"].ToString();

            foreach (var rule in _rules.Rules)
            {
                var childWorkItems =
                    await _client.ListChildWorkItemsForParent(parentWorkItem);

                if (rule.IfChildState.Equals(vm.state))
                {
                    if (!rule.AllChildren)
                    {
                        if (!rule.NotParentStates.Contains(parentState))
                        {
                            Log.Information("Updating '{id}' with {state}", parentWorkItem.Id, rule.SetParentStateTo);

                            await _client.UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                            return new RuleResult
                            {
                                Message = $"Parent updated with {rule.SetParentStateTo}",
                                Modified = true,
                                MatchedRule = rule
                            };
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

                            await _client.UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                            return new RuleResult
                            {
                                Message = $"Parent updated with {rule.SetParentStateTo}",
                                Modified = true,
                                MatchedRule = rule
                            };
                        }
                    }
                }
            }

            return new RuleResult
            {
                Message = "No rule matched",
                Modified = false,
            };
        }
    }
}