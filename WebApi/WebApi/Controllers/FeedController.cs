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

        [HttpGet]
        public async Task<IActionResult> GetFeed()
        {
            var feed = await handlers.GetFeed();
            return Ok(new GetFeedResponse
            {
                Memes = feed
            });
        }

        [HttpGet, Route("search")]
        public async Task<IActionResult> Search([FromQuery]GetSearchFeed req)
        {
            var feed = await handlers.SearchFeed(req);
            return Ok(new GetSearchFeedResponse
            {
                Memes = feed
            });
        }
    }
}
