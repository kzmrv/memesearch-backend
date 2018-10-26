using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Elasticsearch.Net;

using Nest;

namespace Shared
{
    public class EsClient
    {
        public const string MemesIndex = "memes-idx";
        public const string Url = "http://localhost:9200";
        readonly ElasticClient client;

        public EsClient(string index, params string[] nodes)
        {
            var uris = nodes.Select(x => new Uri(x)).ToArray();
            var pool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(pool);
            settings.DefaultIndex(index);

            client = new ElasticClient(settings);
        }


        public static ElasticClient Configure(string[] nodes, string index)
        {
            var uris = nodes.Select(x => new Uri(x)).ToArray();
            var pool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(pool);
            settings.DefaultIndex(index);

            return new ElasticClient(settings);
        }

        public async Task Save(IEnumerable<UnifiedMeme> memes)
        {
            await client.IndexManyAsync(memes);
        }

        public async Task<List<UnifiedMeme>> Get()
        {
            var search = await client.SearchAsync<UnifiedMeme>();
            return search.Documents.ToList();
        }

        public async Task DeleteIndex(string index)
        {
            await client.DeleteIndexAsync(index);
        }

        const string scrollTime = "1h";
        public async Task<(string scrollId, List<UnifiedMeme> memes)> Scroll(string pattern = null, string scrollId = null)
        {
            var search = scrollId == null
                ? await SearchByPattern(pattern, scrollTime)
                : await client.ScrollAsync<UnifiedMeme>(scrollTime, scrollId);

            var memes = search.Documents.ToList();
            Decode(memes);
            return (search.ScrollId, memes);
        }

        const int searchSize = 10;
        public async Task<List<UnifiedMeme>> Search(string pattern)
        {
            var resp = await SearchByPattern(pattern);
            var memes = resp.Documents.ToList();
            Decode(memes);
            return memes;
        }

        static void Decode(List<UnifiedMeme> memes)
        {
            foreach (var meme in memes)
            {
                meme.Title = HttpUtility.HtmlDecode(meme.Title);
                meme.Tags = meme.Tags.Select(HttpUtility.HtmlDecode).ToArray();
            }
        }


        Task<ISearchResponse<UnifiedMeme>> SearchByPattern(string pattern, string scrollTime = null)
        {
            var bq = new BoolQuery
            {
                Must = new List<QueryContainer>
                {
                    new QueryContainer(new MatchQuery
                    {
                        Field = "originalType",
                        Query = "Photo",
                    })
                },
            };

            if (!string.IsNullOrWhiteSpace(pattern))
            {
                bq.Filter = new List<QueryContainer>
                {
                    new QueryContainer(
                        new MatchQuery
                        {
                            Field = "title",
                            Query = pattern
                        })
                };
            }


            return client.SearchAsync<UnifiedMeme>(s => s
                .Scroll(scrollTime ?? Time.Zero)
                .AllTypes()
                .From(0)
                .Take(searchSize)
                .Query(descriptor => bq
                //qry => qry
                //.Bool(g=>g
                //    .Must(m => m
                //        .Match(mm => mm
                //            .Field("originalType")
                //            .Query("Photo")))
                //    .Filter(m =>
                //        m.Match(mm => mm
                //            .Field(um => um.Title)
                //            .Query(pattern)))
                //)
                ));
        }
    }
}
