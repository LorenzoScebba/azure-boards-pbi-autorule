using Microsoft.AspNetCore.Mvc;

namespace azure_boards_pbi_autorule.Controllers
{
    [Route("/api")]
    public class IndexController : ControllerBase
    {
        [HttpPost("receive")]
        public IActionResult Index()
        {
            return Ok();
        }
    }
}