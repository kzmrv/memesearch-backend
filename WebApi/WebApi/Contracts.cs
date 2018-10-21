using System.Collections.Generic;

using Shared;

namespace WebApi
{
    public class GetFeed
    {
    }

    public class GetFeedResponse
    {
        public List<UnifiedMeme> Memes;
    }

    public class GetSearchFeed
    {
        public string query { get; set; }
    }

    public class GetSearchFeedResponse
    {
        public List<UnifiedMeme> Memes;
    }
}
