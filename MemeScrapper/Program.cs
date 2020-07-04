using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

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
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();
            var pages = int.Parse(config["pages"]);
            var posts = CollectNineGagRecent(pages).Result;

            //var savePath = config["savePath"];
            //SaveToFiles(posts, savePath);

            SaveToEs();
            log.Information("App finished");
            Console.ReadKey();
        }

        public static async Task<List<NineGagApi.Post>> CollectNineGagRecent(int pages)
        {
            var feed = NineGagApi.Fresh;
            var api = new NineGagApi();
            var cursor = await api.GetCursor();
            var posts = new List<NineGagApi.Post>();
            // limit ~ 6k posts
            for (int i = 0; i < pages; i++)
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
        static void SaveToFiles(List<NineGagApi.Post> posts, string folder = dataFolder)
        {
            var envPath = Environment.GetEnvironmentVariable("MSEARCH_DATA");
            var currentDate = DateTimeOffset.Now.ToString("yy-MM-dd-HH");
            var path = envPath ?? folder;
            log.Information("Writing to folder {Directory}", path);
            DirectoryExtensions.SaveJson(Path.Combine(path, $"raw{currentDate}.json"), posts);

            var converted = posts.Select(NineGagApi.ToMeme).ToList();
            DirectoryExtensions.SaveJson(Path.Combine(path, $"converted{currentDate}.json"), converted);
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
