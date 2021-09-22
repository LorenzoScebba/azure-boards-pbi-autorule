using System.Threading.Tasks;
using azure_boards_pbi_autorule.Controllers;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace azure_boards_pbi_autorule_tests.Controllers
{
    public class AreaReceiveControllerTests
    {
        [Test]
        public async Task Area_SampleFlow()
        {
            var rulesApplierService = new Mock<IRulesApplierService>();

            rulesApplierService.Setup(x => x.HasAreaRuleForType(It.IsAny<string>())).Returns(true);
            rulesApplierService.Setup(x => x.ApplyAreaRules(It.IsAny<AzureWebHookModel>()))
                .ReturnsAsync(Result<AreaRule, string>.Ok(new AreaRule()));

            var controller = new ReceiveController(rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Area(JObject.FromObject(TestUtils.SampleAreaJObject));

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.True(string.IsNullOrWhiteSpace(controller.Response.Headers["Warning"]));
        }
        
        [Test]
        public async Task Area_NoRule()
        {
            var rulesApplierService = new Mock<IRulesApplierService>();

            rulesApplierService.Setup(x => x.HasAreaRuleForType(It.IsAny<string>())).Returns(false);

            var controller = new ReceiveController(rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Area(JObject.FromObject(TestUtils.SampleAreaJObject));

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("No rule is configured for type Task",
                controller.Response.Headers["x-autorule-info"].ToString());
        }
        
        [Test]
        public async Task Area_WrongEventType()
        {
            var rulesApplierService = new Mock<IRulesApplierService>();

            rulesApplierService.Setup(x => x.HasAreaRuleForType(It.IsAny<string>())).Returns(true);

            var controller = new ReceiveController(rulesApplierService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            var result = await controller.Area(JObject.FromObject(TestUtils.SampleJObjectWithWrongEventType));

            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Event type is not workitem.craeted",
                controller.Response.Headers["x-autorule-info"].ToString());
        }
    }
}