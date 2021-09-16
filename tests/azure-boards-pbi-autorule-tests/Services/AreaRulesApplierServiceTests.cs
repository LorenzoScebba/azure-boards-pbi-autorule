using System.Collections.Generic;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq;
using NUnit.Framework;

namespace azure_boards_pbi_autorule_tests.Services
{
    [TestFixture]
    public class AreaRulesApplierServiceTests
    {
        [Test]
        public async Task ApplyAreaRules_WithUnknownItemType_ShouldFail()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var areaRuleConfigurations = new List<AreaRuleConfiguration>
            {
                new AreaRuleConfiguration
                {
                    Type = new []{"Custom Task"},
                    Rule = new AreaRule
                    {
                        SetAreaPathTo = "NewAreaPath"
                    }
                }
            };

            workItemsService.Setup(w => w.GetWorkItemAsync(It.IsAny<int>(), null ,null ,null)).ReturnsAsync(new WorkItem());
            workItemsService.Setup(w => w.UpdateWorkItemArea(It.IsAny<WorkItem>(), It.IsAny<string>()))
                .ReturnsAsync(new WorkItem());
            
            var service = new RulesApplierService(workItemsService.Object,
                new List<StateRuleConfiguration>(),
                areaRuleConfigurations);

            var vm = new AzureWebHookModel
            {
                eventType = "workitem.created",
                workItemId = 1,
                workItemType = "Task"
            };

            var result = await service.ApplyAreaRules(vm);

            Assert.IsTrue(result.HasError);
            Assert.AreEqual("No rule matched", result.Error);
        }

        [Test]
        public async Task ApplyAreaRules_WithKnownItemType()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var areaRuleConfigurations = new List<AreaRuleConfiguration>
            {
                new AreaRuleConfiguration
                {
                    Type = new []{"Custom Task"},
                    Rule = new AreaRule
                    {
                        SetAreaPathTo = "NewAreaPath"
                    }
                }
            };

            workItemsService.Setup(w => w.GetWorkItemAsync(It.IsAny<int>(), null ,null ,null)).ReturnsAsync(new WorkItem());
            workItemsService.Setup(w => w.UpdateWorkItemArea(It.IsAny<WorkItem>(), It.IsAny<string>()))
                .ReturnsAsync(new WorkItem());
            
            var service = new RulesApplierService(workItemsService.Object,
                new List<StateRuleConfiguration>(),
                areaRuleConfigurations);

            var vm = new AzureWebHookModel
            {
                eventType = "workitem.created",
                workItemId = 1,
                workItemType = "Custom Task"
            };

            var result = await service.ApplyAreaRules(vm);

            Assert.IsFalse(result.HasError);
            Assert.AreEqual(areaRuleConfigurations[0].Rule, result.Data);
        }
    }
}