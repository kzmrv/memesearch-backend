using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Serilog;

using Shared;

namespace MemeScrapper
{
    class Program
    {
        static readonly ILogger log = Logging.Configure(nameof(MemeScrapper));
        static void Main(string[] args)
        {
            log.Information("App started");
            var posts = CollectNineGagRecent().Result;
            SaveToFiles(posts);
            log.Information("App finished");
            Console.ReadKey();
        }

        public static async Task<List<NineGagApi.Post>> CollectNineGagRecent()
        {
            var feed = NineGagApi.Fresh;
            var api = new NineGagApi();
            var cursor = await api.GetCursor();
            var posts = new List<NineGagApi.Post>();
            // limit ~ 6k posts
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    var response = await api.CollectPage(cursor, feed);
                    posts.AddRange(response.data.posts);
                    var chars = response.data.nextCursor.TrimStart("after=").TakeWhile(c => c != '&').ToArray();
                    cursor = WebUtility.UrlDecode(new string(chars));
                    log.Debug("Success for {PageNumber} page", i);
                }
                catch (Exception ex)
                {
                    log.Information(ex, "Stopped at {PageNumber} page");
                    break;
                }
            }

            var distinctPosts = posts
                .Distinct(x => x.id)
                .OrderByDescending(x => x.creationTs)
                .ToList();
            return distinctPosts;
        }

        const string dataFolder = "c:\\\\temp\\9gag\\data\\";
        static void SaveToFiles(List<NineGagApi.Post> posts)
        {
            var alternativePath = Environment.GetEnvironmentVariable("MSEARCH_DATA");
            var path = alternativePath ?? dataFolder;
            log.Information("Writing to folder {Directory}", path);
            var converted = posts.Select(NineGagApi.ToMeme).ToList();
            var currentDate = DateTimeOffset.UtcNow.ToString("yy-MM-dd-hh");
            DirectoryExtensions.SaveJson($"{path}raw{currentDate}.json", posts);
            DirectoryExtensions.SaveJson($"{path}converted{currentDate}.json", converted);
        }

        public static void RemoveIndexHandle()
        {
            var es = GetClient();
            es.DeleteIndex(EsClient.MemesIndex).Wait();
        }

        static EsClient GetClient() => new EsClient(EsClient.MemesIndex, EsClient.LocalUrl);

        public static void SaveToEs()
        {
            var date = DateTimeOffset.UtcNow.ToString("yy-MM-dd-hh");
            var path = $"{dataFolder}converted{date}.json";
            var str = File.ReadAllText(path);
            var memes = JsonConvert.DeserializeObject<List<UnifiedMeme>>(str);
            GetClient().Save(memes).GetAwaiter().GetResult();
            log.Information("Successfully saved to Elastic cluster!");
        }
    }
}
