using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Extensions;
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
        private readonly IEnumerable<RuleConfiguration> _rules;

        public RulesApplierService(IWorkItemsService client, IEnumerable<RuleConfiguration> rules)
        {
            _client = client;
            _rules = rules;
        }

        public bool HasRuleForType(string type)
        {
            return _rules.Any(r => r.Type.Equals(type));
        }

        public async Task<Result<Rule, string>> ApplyRules(AzureWebHookModel vm, WorkItem parentWorkItem)
        {
            Log.Information("{typeChanged} '#{idChanged}' state has changed to {stateChanged}, applying rules on {typeParent}", 
                vm.workItemType,
                vm.workItemId,
                vm.state,
                parentWorkItem.GetWorkItemField("System.WorkItemType")
                );

            foreach (var ruleConfig in _rules)
            {
                if (!ruleConfig.Type.Equals(vm.workItemType))
                    continue;

                foreach (var rule in ruleConfig.Rules)
                {
                    var childWorkItems = (await _client.ListChildWorkItemsForParent(parentWorkItem)).ToList();

                    if (rule.IfChildState.Equals(vm.state))
                    {
                        if (!rule.AllChildren)
                        {
                            if (!rule.NotParentStates.Contains(parentWorkItem.GetWorkItemField("System.State")))
                            {
                                Log.Information("Updating {type} '#{id}' with {state}",
                                    parentWorkItem.GetWorkItemField("System.WorkItemType"),
                                    parentWorkItem.Id,
                                    rule.SetParentStateTo);

                                try
                                {
                                    await _client.UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                                    return Result<Rule, string>.Ok(rule);
                                }
                                catch (RuleValidationException e)
                                {
                                    return Result<Rule, string>.Fail(
                                        $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                                }
                            }
                        }
                        else
                        {
                            // check to see if any of the child items are not closed, if so, we will get a count > 0
                            var count = childWorkItems
                                .Where(x => !x.Fields["System.State"].ToString().Equals(rule.IfChildState)).ToList()
                                .Count;

                            if (count.Equals(0))
                            {
                                Log.Information("Updating {type} '#{id}' with {state}", 
                                    parentWorkItem.GetWorkItemField("System.WorkItemType"),
                                    parentWorkItem.Id,
                                    rule.SetParentStateTo);

                                try
                                {
                                    await _client.UpdateWorkItemState(parentWorkItem, rule.SetParentStateTo);
                                    return Result<Rule, string>.Ok(rule);
                                }
                                catch (RuleValidationException e)
                                {
                                    return Result<Rule, string>.Fail(
                                        $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                                }
                            }
                        }
                    }
                }
            }

            return Result<Rule, string>.Fail("No rule matched");
        }
    }
}