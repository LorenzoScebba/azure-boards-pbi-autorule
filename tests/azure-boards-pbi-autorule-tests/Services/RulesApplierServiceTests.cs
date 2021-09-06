using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq;
using NUnit.Framework;

namespace azure_boards_pbi_autorule_tests.Services
{
    [TestFixture]
    public class RulesApplierServiceTests
    {
        [TestFixture]
        public class Parent
        {
            [Test]
            public async Task ApplyRules_ToNew()
            {
                // Related tasks
                var relatedTasks = new List<WorkItem>
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
                // Parent exists
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });

                // Childrens defined up top
                workItemsService.Setup(x => x.ListChildWorkItemsForParent(It.IsAny<WorkItem>()))
                    .ReturnsAsync(relatedTasks);

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleTaskRules });

                // If a child goes to to do and all task are in to do
                var vm = new AzureWebHookModel
                {
                    state = "To Do",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Task"
                };

                var result = await service.ApplyRules(vm);

                // rule for parent in new should be applied
                Assert.IsFalse(result.HasError);
                Assert.AreEqual(TestUtils.SampleTaskRules.Rules[0], result.Data);
            }

            [Test]
            public async Task ApplyRules_ToCommitted()
            {
                var workItemsService = new Mock<IWorkItemsService>();
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleTaskRules });

                var vm = new AzureWebHookModel
                {
                    state = "In Progress",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Task"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsFalse(result.HasError);
                Assert.AreEqual(TestUtils.SampleTaskRules.Rules[1], result.Data);
            }

            [Test]
            public async Task ApplyRules_MultipleRules_ToCommittedTask()
            {
                var workItemsService = new Mock<IWorkItemsService>();
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleProductBacklogItemRules, TestUtils.SampleTaskRules });

                var vm = new AzureWebHookModel
                {
                    state = "In Progress",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Task"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsFalse(result.HasError);
                Assert.AreEqual(TestUtils.SampleTaskRules.Rules[1], result.Data);
            }

            [Test]
            public async Task ApplyRules_MultipleRules_ToCommittedPBI()
            {
                var workItemsService = new Mock<IWorkItemsService>();
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleProductBacklogItemRules, TestUtils.SampleTaskRules });

                var vm = new AzureWebHookModel
                {
                    state = "Committed",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Product Backlog Item"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsFalse(result.HasError);
                Assert.AreEqual(TestUtils.SampleProductBacklogItemRules.Rules[1], result.Data);
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
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });
                workItemsService.Setup(x => x.ListChildWorkItemsForParent(It.IsAny<WorkItem>()))
                    .ReturnsAsync(workItemsRelated);

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleTaskRules });

                var vm = new AzureWebHookModel
                {
                    state = "To Do",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Task"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsTrue(result.HasError);
            }

            [Test]
            public async Task ApplyRules_WrongConfiguration_AnyChild()
            {
                var workItemsService = new Mock<IWorkItemsService>();
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });
                workItemsService.Setup(x => x.UpdateWorkItemState(It.IsAny<WorkItem>(), It.IsAny<string>()))
                    .ThrowsAsync(new RuleValidationException("Sample Rule Validation Exception Message", null));

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleInvalidTaskRules });

                var vm = new AzureWebHookModel
                {
                    state = "In Progress",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Task"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsTrue(result.HasError);
                StringAssert.Contains("Sample Rule Validation Exception Message", result.Error);
            }

            [Test]
            public async Task ApplyRules_WrongConfiguration_AllChild()
            {
                var workItemsService = new Mock<IWorkItemsService>();
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });
                workItemsService.Setup(x => x.UpdateWorkItemState(It.IsAny<WorkItem>(), It.IsAny<string>()))
                    .ThrowsAsync(new RuleValidationException("Sample Rule Validation Exception Message", null));

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleInvalidTaskRules });

                var vm = new AzureWebHookModel
                {
                    state = "To Do",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Task"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsTrue(result.HasError);
                StringAssert.Contains("Sample Rule Validation Exception Message", result.Error);
            }
        }
        
        [TestFixture]
        public class Childrens
        {
            [Test]
            public async Task ApplyRules_ChildrensToDone()
            {
                var relatedTasks = new List<WorkItem>
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
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });

                workItemsService.Setup(x => x.ListChildWorkItemsForParent(It.IsAny<WorkItem>()))
                    .ReturnsAsync(relatedTasks);

                workItemsService.Setup(wis => wis.UpdateWorkItemState(It.IsAny<WorkItem>(), It.IsAny<string>()))
                    .ReturnsAsync(new WorkItem());

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleChildrensRules });

                var vm = new AzureWebHookModel
                {
                    state = "Done",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Product Backlog Item"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsFalse(result.HasError);
                Assert.AreEqual(TestUtils.SampleChildrensRules.Rules[0], result.Data);
            }
            
            [Test]
            public async Task ApplyRules_ChildrensValidationException()
            {
                var relatedTasks = new List<WorkItem>
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
                workItemsService.Setup(wis =>
                        wis.GetWorkItemAsync(It.IsAny<int>(), null, null, It.IsAny<WorkItemExpand>()))
                    .ReturnsAsync(new WorkItem { Id = 1 });

                workItemsService.Setup(x => x.ListChildWorkItemsForParent(It.IsAny<WorkItem>()))
                    .ReturnsAsync(relatedTasks);

                workItemsService.Setup(wis => wis.UpdateWorkItemState(It.IsAny<WorkItem>(), It.IsAny<string>()))
                    .ThrowsAsync(new RuleValidationException("'Rule validation error message'", new Exception()));

                var service = new RulesApplierService(workItemsService.Object,
                    new List<RuleConfiguration> { TestUtils.SampleChildrensRules });

                var vm = new AzureWebHookModel
                {
                    state = "Done",
                    eventType = "workitem.updated",
                    parentId = 1,
                    workItemId = 2,
                    workItemType = "Product Backlog Item"
                };

                var result = await service.ApplyRules(vm);

                Assert.IsTrue(result.HasError);
                StringAssert.Contains("A rule validation exception occurred", result.Error);
            }
            
        }
    }
}