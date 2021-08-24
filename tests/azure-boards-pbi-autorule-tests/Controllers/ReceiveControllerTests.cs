using System.Threading.Tasks;
using azure_boards_pbi_autorule.Controllers;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace azure_boards_pbi_autorule_tests.Controllers
{
    public class ReceiveControllerTests
    {
        [Test]
        public async Task Index_SampleFlow()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var rulesApplierService = new Mock<IRulesApplierService>();

            workItemsService.Setup(x => x.GetWorkItemAsync(1, null, null, WorkItemExpand.Relations))
                .ReturnsAsync(new WorkItem
                {
                    Id = 1,
                    // other parameters omitted for testing
                });

            rulesApplierService.Setup(x => x.HasRuleForType(It.IsAny<string>())).Returns(true);
            rulesApplierService.Setup(x => x.ApplyRules(It.IsAny<AzureWebHookModel>(), It.IsAny<WorkItem>()))
                .ReturnsAsync(Result<Rule, string>.Ok(TestUtils.SampleTaskRules.Rules[1]));

            var controller = new ReceiveController(workItemsService.Object, rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Index(JObject.FromObject(TestUtils.SampleJObject));

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.True(string.IsNullOrWhiteSpace(controller.Response.Headers["Warning"]));
        }
        
        [Test]
        public async Task Index_WrongEvent()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var rulesApplierService = new Mock<IRulesApplierService>();

            var controller = new ReceiveController(workItemsService.Object, rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Index(JObject.FromObject(TestUtils.SampleJObjectWithWrongEventType));

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Event type is not workitem.updated", controller.Response.Headers["x-autorule-info"].ToString());
        }
        
        [Test]
        public async Task Index_NoParent()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var rulesApplierService = new Mock<IRulesApplierService>();

            rulesApplierService.Setup(x => x.HasRuleForType(It.IsAny<string>())).Returns(true);
            workItemsService.Setup(x => x.GetWorkItemAsync(1, null, null, WorkItemExpand.Relations))
                .ReturnsAsync((WorkItem)null);
            
            var controller = new ReceiveController(workItemsService.Object, rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Index(JObject.FromObject(TestUtils.SampleJObject));

            Assert.IsInstanceOf<OkObjectResult>(result);
            StringAssert.Contains("Parent work item with id", controller.Response.Headers["x-autorule-info"].ToString());
        }

        [Test]
        public async Task Index_NoRuleMatch()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var rulesApplierService = new Mock<IRulesApplierService>();

            var controller = new ReceiveController(workItemsService.Object, rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Index(JObject.FromObject(TestUtils.SampleJObject));

            Assert.IsInstanceOf<OkObjectResult>(result);
            StringAssert.Contains("No rule is configured for type", controller.Response.Headers["x-autorule-info"].ToString());
        }
        
        [Test]
        public async Task Index_NoWork()
        {
            var workItemsService = new Mock<IWorkItemsService>();
            var rulesApplierService = new Mock<IRulesApplierService>();

            rulesApplierService.Setup(x => x.HasRuleForType(It.IsAny<string>())).Returns(true);
            workItemsService.Setup(x => x.GetWorkItemAsync(1, null, null, WorkItemExpand.Relations))
                .ReturnsAsync(new WorkItem
                {
                    Id = 1,
                    // other parameters omitted for testing
                });
            
            rulesApplierService.Setup(x => x.ApplyRules(It.IsAny<AzureWebHookModel>(), It.IsAny<WorkItem>()))
                .ReturnsAsync(Result<Rule, string>.Fail("No rule matched"));

            var controller = new ReceiveController(workItemsService.Object, rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Index(JObject.FromObject(TestUtils.SampleJObject));

            Assert.IsInstanceOf<OkObjectResult>(result);
            StringAssert.Contains("No rule matched", controller.Response.Headers["x-autorule-info"].ToString());
        }
    }
}