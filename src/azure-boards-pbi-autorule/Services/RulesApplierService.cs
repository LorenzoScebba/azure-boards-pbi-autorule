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
        private readonly IEnumerable<StateRuleConfiguration> _stateRules;
        private readonly IEnumerable<AreaRuleConfiguration> _areaRules;

        public RulesApplierService(IWorkItemsService client, IEnumerable<StateRuleConfiguration> stateRules, IEnumerable<AreaRuleConfiguration> areaRules)
        {
            _client = client;
            _stateRules = stateRules;
            _areaRules = areaRules;
        }

        public bool HasStateRuleForType(string type)
        {
            return _stateRules.Any(r => r.Type.Equals(type));
        }

        public bool HasAreaRuleForType(string type)
        {
            return _areaRules.Any(r => r.Type.Contains(type));
        }

        public async Task<Result<StateRule, string>> ApplyStateRules(AzureWebHookModel vm)
        {
            Log.Information(
                "{typeChanged} '#{idChanged}' state has changed to {stateChanged}, applying rules...",
                vm.workItemType,
                vm.workItemId,
                vm.state
            );

            foreach (var ruleConfig in _stateRules)
            {
                if (!ruleConfig.Type.Equals(vm.workItemType))
                    continue;

                foreach (var rule in ruleConfig.Rules)
                {
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
            }

            return Result<StateRule, string>.Fail("No rule matched");
        }

        public async Task<Result<AreaRule, string>> ApplyAreaRules(AzureWebHookModel vm)
        {
            Log.Information(
                "{typeChanged} '#{idChanged}' was created, applying rules...",
                vm.workItemType,
                vm.workItemId
                );
            
            foreach (var ruleConfig in _areaRules)
            {
                if (!ruleConfig.Type.Contains(vm.workItemType))
                    continue;

                return await ApplyAreaRule(vm, ruleConfig.Rule);
            }
            
            return Result<AreaRule, string>.Fail("No rule matched");
        }

        private async Task<Result<AreaRule, string>> ApplyAreaRule(AzureWebHookModel vm, AreaRule areaRule)
        {
            var workItem = await _client.GetWorkItemAsync(vm.workItemId);
                
            try
            {
                await _client.UpdateWorkItemArea(workItem, areaRule.SetAreaPathTo);
                return Result<AreaRule, string>.Ok(areaRule);
            }
            catch (RuleValidationException e)
            {
                return Result<AreaRule, string>.Fail(
                    $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
            }
            
        }

        private async Task<Result<StateRule, string>> ApplyRulesToParent(AzureWebHookModel vm, StateRule stateRule)
        {
            var parentWorkItem = await _client.GetWorkItemAsync(vm.parentId, null, null, WorkItemExpand.Relations);

            if (parentWorkItem == null) return Result<StateRule, string>.Fail("No parent is available.");

            var childWorkItems = (await _client.ListChildWorkItemsForParent(parentWorkItem)).ToList();

            // We're updating the parent of the changed work item 

            if (stateRule.IfState.Equals(vm.state))
            {
                if (stateRule.SetParentStateTo.Equals(parentWorkItem.GetWorkItemField("System.State")))
                    return Result<StateRule, string>.Fail(
                        $"Parent state is already '{stateRule.SetParentStateTo}', skipping!");

                if (stateRule.All)
                {
                    // check to see if any of the child items are not closed, if so, we will get a count > 0
                    var count = childWorkItems
                        .Where(x => !x.GetWorkItemField("System.State").Equals(stateRule.IfState)).ToList()
                        .Count;

                    if (count.Equals(0))
                    {
                        Log.Information("Updating {type} '#{id}' with {state}",
                            parentWorkItem.GetWorkItemField("System.WorkItemType"),
                            parentWorkItem.Id,
                            stateRule.SetParentStateTo);

                        try
                        {
                            await _client.UpdateWorkItemState(parentWorkItem, stateRule.SetParentStateTo);
                            return Result<StateRule, string>.Ok(stateRule);
                        }
                        catch (RuleValidationException e)
                        {
                            return Result<StateRule, string>.Fail(
                                $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                        }
                    }
                }
                else
                {
                    if (!stateRule.NotParentStates.Contains(parentWorkItem.GetWorkItemField("System.State")))
                    {
                        Log.Information("Updating {type} '#{id}' with {state}",
                            parentWorkItem.GetWorkItemField("System.WorkItemType"),
                            parentWorkItem.Id,
                            stateRule.SetParentStateTo);

                        try
                        {
                            await _client.UpdateWorkItemState(parentWorkItem, stateRule.SetParentStateTo);
                            return Result<StateRule, string>.Ok(stateRule);
                        }
                        catch (RuleValidationException e)
                        {
                            return Result<StateRule, string>.Fail(
                                $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                        }
                    }
                }
            }

            return null;
        }

        private async Task<Result<StateRule, string>> ApplyRulesToChildrens(AzureWebHookModel vm, StateRule stateRule)
        {
            var workItem =
                await _client.GetWorkItemAsync(vm.workItemId, null, null, WorkItemExpand.Relations);

            if (workItem == null) return Result<StateRule, string>.Fail("No parent is available.");

            var childWorkItems = (await _client.ListChildWorkItemsForParent(workItem)).ToList();

            // We're updating the childrens of the changed work item 
            if (stateRule.IfState.Equals(vm.state))
            {
                foreach (var childWorkItem in childWorkItems)
                    try
                    {
                        if(!childWorkItem.GetWorkItemField("System.State").Equals(stateRule.SetChildrenStateTo)){
                            Log.Information("Updating {type} '#{id}' with {state}",
                                childWorkItem.GetWorkItemField("System.WorkItemType"),
                                childWorkItem.Id,
                                stateRule.SetChildrenStateTo);
                            
                            await _client.UpdateWorkItemState(childWorkItem, stateRule.SetChildrenStateTo);
                        }
                    }
                    catch (RuleValidationException e)
                    {
                        return Result<StateRule, string>.Fail(
                            $"A rule validation exception occurred, please review the rule. Error was {e.Message}");
                    }

                return Result<StateRule, string>.Ok(stateRule);
            }

            return null;
        }
    }
}