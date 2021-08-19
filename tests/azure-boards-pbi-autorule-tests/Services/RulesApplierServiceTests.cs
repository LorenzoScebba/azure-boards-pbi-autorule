using System.Collections.Generic;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq;
using NUnit.Framework;

namespace azure_boards_pbi_autorule_tests.Services
{
    public class RulesApplierServiceTests
    {
        [Test]
        public async Task ApplyRules_ToCommitted()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var service = new RulesApplierService(workItemsService.Object, TestUtils.SampleRules);

            var vm = new AzureWebHookModel
            {
                state = "In Progress",
                eventType = "wm.updated",
                parentId = 1,
                workItemId = 2,
                workItemType = "Task"
            };

            var parent = new WorkItem
            {
                Id = 1,
                Fields = new Dictionary<string, object>
                {
                    { "System.State", "New" }
                }
            };

            var result = await service.ApplyRules(vm, parent);
            
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(TestUtils.SampleRules.Rules[1], result.Data);
        }
        
        [Test]
        public async Task ApplyRules_ToNew()
        {
            var workItemsRelated = new List<WorkItem>
            {
                new WorkItem
                {
                    Fields = new Dictionary<string, object>
                    {
                        { "System.State", "To Do" }
                    }
                },
                new WorkItem
                {
                    Fields = new Dictionary<string, object>
                    {
                        { "System.State", "To Do" }
                    }
                }
            };
            
            var workItemsService = new Mock<IWorkItemsService>();
            workItemsService.Setup(x => x.ListChildWorkItemsForParent(It.IsAny<WorkItem>()))
                .ReturnsAsync(workItemsRelated);
                
            var service = new RulesApplierService(workItemsService.Object, TestUtils.SampleRules);

            var vm = new AzureWebHookModel
            {
                state = "To Do",
                eventType = "wm.updated",
                parentId = 1,
                workItemId = 2,
                workItemType = "Task"
            };

            var parent = new WorkItem
            {
                Id = 1,
                Fields = new Dictionary<string, object>
                {
                    { "System.State", "Committed" }
                }
            };

            var result = await service.ApplyRules(vm, parent);
            
            Assert.IsFalse(result.HasError);
            Assert.AreEqual(TestUtils.SampleRules.Rules[0], result.Data);
        }
        
        [Test]
        public async Task ApplyRules_ToNew_OneStillInCommitted()
        {
            var workItemsRelated = new List<WorkItem>
            {
                new WorkItem
                {
                    Fields = new Dictionary<string, object>
                    {
                        { "System.State", "To Do" }
                    }
                },
                new WorkItem
                {
                    Fields = new Dictionary<string, object>
                    {
                        { "System.State", "In Progress" }
                    }
                }
            };
            
            var workItemsService = new Mock<IWorkItemsService>();
            workItemsService.Setup(x => x.ListChildWorkItemsForParent(It.IsAny<WorkItem>()))
                .ReturnsAsync(workItemsRelated);
                
            var service = new RulesApplierService(workItemsService.Object, TestUtils.SampleRules);

            var vm = new AzureWebHookModel
            {
                state = "To Do",
                eventType = "wm.updated",
                parentId = 1,
                workItemId = 2,
                workItemType = "Task"
            };

            var parent = new WorkItem
            {
                Id = 1,
                Fields = new Dictionary<string, object>
                {
                    { "System.State", "Committed" }
                }
            };

            var result = await service.ApplyRules(vm, parent);
            
            Assert.IsTrue(result.HasError);
        }
        
        [Test]
        public async Task ApplyRules_WrongConfiguration_AnyChild()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            workItemsService.Setup(x => x.UpdateWorkItemState(It.IsAny<WorkItem>(), It.IsAny<string>()))
                .ThrowsAsync(new RuleValidationException("Sample Rule Validation Exception Message", null));
            
            var service = new RulesApplierService(workItemsService.Object, TestUtils.SampleRulesWithWrongSetParentStateTo);

            var vm = new AzureWebHookModel
            {
                state = "In Progress",
                eventType = "wm.updated",
                parentId = 1,
                workItemId = 2,
                workItemType = "Task"
            };

            var parent = new WorkItem
            {
                Id = 1,
                Fields = new Dictionary<string, object>
                {
                    { "System.State", "New" }
                }
            };

            var result = await service.ApplyRules(vm, parent);
            
            Assert.IsTrue(result.HasError);
            StringAssert.Contains("Sample Rule Validation Exception Message", result.Error);
        }
        
        [Test]
        public async Task ApplyRules_WrongConfiguration_AllChild()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            workItemsService.Setup(x => x.UpdateWorkItemState(It.IsAny<WorkItem>(), It.IsAny<string>()))
                .ThrowsAsync(new RuleValidationException("Sample Rule Validation Exception Message", null));
            
            var service = new RulesApplierService(workItemsService.Object, TestUtils.SampleRulesWithWrongSetParentStateTo);

            var vm = new AzureWebHookModel
            {
                state = "To Do",
                eventType = "wm.updated",
                parentId = 1,
                workItemId = 2,
                workItemType = "Task"
            };

            var parent = new WorkItem
            {
                Id = 1,
                Fields = new Dictionary<string, object>
                {
                    { "System.State", "Committed" }
                }
            };

            var result = await service.ApplyRules(vm, parent);
            
            Assert.IsTrue(result.HasError);
            StringAssert.Contains("Sample Rule Validation Exception Message", result.Error);
        }
    }
}