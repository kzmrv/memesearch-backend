using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<List<UnifiedMeme>> Search(string pattern)
        {
            var resp = await client.SearchAsync<UnifiedMeme>(s => s
                .AllTypes()
                .From(0)
                .Take(10)
                .Query(qry => qry
                    .Bool(b => b
                        .Must(m => m
                            .QueryString(qs => qs
                                .Query(pattern))))));

            return resp.Documents.ToList();
        }
    }
}
