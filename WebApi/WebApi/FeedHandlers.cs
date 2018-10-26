using System.Threading.Tasks;

using Shared;

namespace WebApi
{
    public class FeedHandlers
    {
        readonly EsClient client;
        public FeedHandlers(EsClient client)
        {
            this.client = client;
        }

        public async Task<GetSearchFeedResponse> SearchFeed(GetSearchFeed query)
        {
            var scroll = await client.Scroll(query.query, query.scrollId);

            return new GetSearchFeedResponse
            {
                scrollId = scroll.scrollId,
                memeses = scroll.memes
            };
        }
    }
}
