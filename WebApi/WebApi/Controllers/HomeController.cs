using Microsoft.AspNetCore.Mvc;

using Shared;

namespace WebApi.Controllers
{
    [Route("api")]
    public class HomeController : Controller
    {
        [HttpGet, Route("health")]
        public IActionResult Health() => Ok("Ok");
    }
}
