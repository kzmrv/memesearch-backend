using System;
using System.Linq;
using System.Threading.Tasks;

using Flurl;
using Flurl.Http;

using Shared;

namespace MemeScrapper
{
    public class NineGagApi
    {
        const string Host = "https://9gag.com";

        public async Task<string> GetCursor()
        {
            var html = await Host.GetStringAsync();
            var marker = "\"after=";
            var iof = html.IndexOf(marker, StringComparison.Ordinal);
            var cursor = html
                .Substring(iof + marker.Length)
                .TakeWhile(c => c != '&')
                .ToArray();

            return new string(cursor);
        }

        public const string Hot = "hot";
        public const string Fresh = "fresh";
        public async Task<Response> CollectPage(string cursor, string type)
        {
            var response = await Host
                .AppendPathSegments("v1", "group-posts", "group", "default", "type", type)
                .SetQueryParam("after", cursor)
                .SetQueryParam("c", 50)
                .GetJsonAsync<Response>();
            return response;
        }

        const string source = "9gag.com";
        public static UnifiedMeme ToMeme(Post post) => new UnifiedMeme
        {
            Source = source,
            CommentsCount = post.commentsCount,
            Likes = post.upVoteCount,
            Url = post.url,
            Published = DateTimeOffset.FromUnixTimeSeconds(post.creationTs),
            OriginalId = post.id,
            Tags = post.tags.Select(x => x.key).ToArray(),
            Title = post.title,
            OriginalType = post.type,
            Media = Media.From(new Media
            {
                Url = post.images?.image700?.url ?? post.images?.image460?.url
            }),
            Permalink = post.url,
        };

        public class Response
        {
            public Meta meta { get; set; }
            public Data data { get; set; }
        }

        public class Meta
        {
            public int timestamp { get; set; }
            public string status { get; set; }
            public string sid { get; set; }
        }

        public class Data
        {
            public Post[] posts { get; set; }
            public string nextCursor { get; set; }
        }

        public class Post
        {
            public string id { get; set; }
            public string url { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public int nsfw { get; set; }
            public int upVoteCount { get; set; }
            public int creationTs { get; set; }
            public int promoted { get; set; }
            public int isVoteMasked { get; set; }
            public int hasLongPostCover { get; set; }
            public Images images { get; set; }
            public string sourceDomain { get; set; }
            public string sourceUrl { get; set; }
            public int commentsCount { get; set; }
            public Postsection postSection { get; set; }
            public Tag[] tags { get; set; }
            public string descriptionHtml { get; set; }
            public Article article { get; set; }
        }

        public class Images
        {
            public Image700 image700 { get; set; }
            public Image460 image460 { get; set; }
            public Image460sv image460sv { get; set; }
            public Image460svwm image460svwm { get; set; }
        }

        public class Image700
        {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
            public string webpUrl { get; set; }
        }

        public class Image460
        {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
            public string webpUrl { get; set; }
        }

        public class Image460sv
        {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
            public int hasAudio { get; set; }
            public int duration { get; set; }
            public string h265Url { get; set; }
            public string vp9Url { get; set; }
        }

        public class Image460svwm
        {
            public int width { get; set; }
            public int height { get; set; }
            public string url { get; set; }
            public int hasAudio { get; set; }
            public int duration { get; set; }
        }

        public class Postsection
        {
            public string name { get; set; }
            public string url { get; set; }
            public string imageUrl { get; set; }
        }

        public class Article
        {
            public Block[] blocks { get; set; }
            public object medias; //TODO do this
        }

        public class Block
        {
            public string type { get; set; }
            public string mediaId { get; set; }
            public string caption { get; set; }
        }

        public class Tag
        {
            public string key { get; set; }
            public string url { get; set; }
        }
    }
}
