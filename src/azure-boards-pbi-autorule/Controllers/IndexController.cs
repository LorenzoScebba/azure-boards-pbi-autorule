using Flurl;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace azure_boards_pbi_autorule.Controllers
{
    [Route("")]
    public class IndexController: ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            var url = Request.GetDisplayUrl().AppendPathSegment("api").AppendPathSegment("receive");
            
            Response.Headers.Add("x-autorule-info", $"The endpoint that needs to be configured on Azure Devops is {url}");
            return Ok("azure-boards-pbi-autorule");
        }
    }
}