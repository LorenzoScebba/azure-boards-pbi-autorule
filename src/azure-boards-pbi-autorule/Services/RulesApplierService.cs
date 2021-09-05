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

        public async Task<Result<Rule, string>> ApplyRules(AzureWebHookModel vm)
        {
            Log.Information(
                "{typeChanged} '#{idChanged}' state has changed to {stateChanged}, applying rules...",
                vm.workItemType,
                vm.workItemId,
                vm.state
            );

            foreach (var ruleConfig in _rules)
            {
                if (!ruleConfig.Type.Equals(vm.workItemType))
                    continue;

                foreach (var rule in ruleConfig.Rules)
                    if (!string.IsNullOrWhiteSpace(rule.SetParentStateTo))
                    {
                        var result = await ApplyRulesToParent(vm, rule);

                        if (result != null)
                            return result;
                    }
                    else if (!string.IsNullOrWhiteSpace(rule.SetChildrenStateTo))
                    {
                        var result = await ApplyRulesToChildrens(vm, rule);

                        if (result != null)
                            return result;
                    }
            }

            return Result<Rule, string>.Fail("No rule matched");
        }

        private async Task<Result<Rule, string>> ApplyRulesToParent(AzureWebHookModel vm, Rule rule)
        {
            var parentWorkItem = await _client.GetWorkItemAsync(vm.parentId, null, null, WorkItemExpand.Relations);

            if (parentWorkItem == null) return Result<Rule, string>.Fail("No parent is available.");

            var childWorkItems = (await _client.ListChildWorkItemsForParent(parentWorkItem)).ToList();

            // We're updating the parent of the changed work item 

            if (rule.IfState.Equals(vm.state))
            {
                if (rule.SetParentStateTo.Equals(parentWorkItem.GetWorkItemField("System.State")))
                    return Result<Rule, string>.Fail(
                        $"Parent state is already '{rule.SetParentStateTo}', skipping!");

                if (rule.All)
                {
                    // check to see if any of the child items are not closed, if so, we will get a count > 0
                    var count = childWorkItems
                        .Where(x => !x.GetWorkItemField("System.State").Equals(rule.IfState)).ToList()
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
                else
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
            }

            return null;
        }

        private async Task<Result<Rule, string>> ApplyRulesToChildrens(AzureWebHookModel vm, Rule rule)
        {
            var workItem =
                await _client.GetWorkItemAsync(vm.workItemId, null, null, WorkItemExpand.Relations);

            if (workItem == null) return Result<Rule, string>.Fail("No parent is available.");

            var childWorkItems = (await _client.ListChildWorkItemsForParent(workItem)).ToList();

            // We're updating the childrens of the changed work item 
            if (rule.IfState.Equals(vm.state))
            {
                foreach (var childWorkItem in childWorkItems)
                    try
                    {
                        Log.Information("Updating {type} '#{id}' with {state}",
                            childWorkItem.GetWorkItemField("System.WorkItemType"),
                            childWorkItem.Id,
                            rule.SetChildrenStateTo);

                        await _client.UpdateWorkItemState(childWorkItem, rule.SetChildrenStateTo);
                    }
                    catch (RuleValidationException e)
                    {
                        return Result<Rule, string>.Fail(
                            $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                    }

                return Result<Rule, string>.Ok(rule);
            }

            return null;
        }
    }
}