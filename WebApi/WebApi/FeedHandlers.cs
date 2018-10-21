using System.Collections.Generic;
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

        public async Task<List<UnifiedMeme>> GetFeed()
        {
            var memes = await client.Get();
            return memes;
        }

        public async Task<List<UnifiedMeme>> SearchFeed(GetSearchFeed query)
        {
            var memes = await client.Search(query.query);
            return memes;
        }
    }
}
