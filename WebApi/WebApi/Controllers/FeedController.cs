using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/feed")]
    public class FeedController : Controller
    {
        FeedHandlers handlers;
        public FeedController(FeedHandlers handlers)
        {
            this.handlers = handlers;
        }

        [HttpGet, Route("search")]
        public async Task<IActionResult> Search([FromQuery]GetSearchFeed req)
        {
            var searchResult = await handlers.SearchFeed(req);
            return Ok(searchResult);
        }
    }
}
