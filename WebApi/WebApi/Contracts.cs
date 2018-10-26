using System.Collections.Generic;

using Shared;

namespace WebApi
{
    public class GetSearchFeed
    {
        public string query { get; set; }
        public string scrollId { get; set; }
    }

    public class GetSearchFeedResponse
    {
        public List<UnifiedMeme> memeses;
        public string scrollId;
    }
}
