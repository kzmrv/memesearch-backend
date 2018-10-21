using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Shared;

namespace MemeScrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            var posts = CollectNineGagRecent().Result;
            SaveToFiles(posts);
            Console.WriteLine("Completed");
            Console.ReadKey();
        }


        public static async Task<List<NineGagApi.Post>> CollectNineGagRecent()
        {
            var type = NineGagApi.Fresh;
            var api = new NineGagApi();
            var cursor = await api.GetCursor();
            var posts = new List<NineGagApi.Post>();
            // limit ~ 6k posts
            for (int i = 0; i < 1000; i++)
            {
                try
                {
                    var response = await api.CollectPage(cursor, type);
                    posts.AddRange(response.data.posts);
                    var chars = response.data.nextCursor.TrimStart("after=").TakeWhile(c => c != '&').ToArray();
                    cursor = WebUtility.UrlDecode(new string(chars));
                    Console.WriteLine($"Success for {i} page");
                }
                catch (Exception ex)
                {
                    break;
                }
            }

            var distinct = posts.Distinct(x => x.id).OrderByDescending(x => x.creationTs).ToList();
            return distinct.ToList();
        }

        const string dataFolder = "c:\\\\temp\\9gag\\data\\";
        static void SaveToFiles(List<NineGagApi.Post> posts)
        {
            var alternativePath = Environment.GetEnvironmentVariable("MSEARCH_DATA");
            var path = alternativePath ?? dataFolder;
            var converted = posts.Select(NineGagApi.ToMeme).ToList();
            var date = DateTimeOffset.UtcNow.ToString("yy-MM-dd-hh");
            TextExtensions.SaveJson($"{path}raw{date}.json", posts);
            TextExtensions.SaveJson($"{path}converted{date}.json", converted);
        }

        public static void DeleteAll()
        {
            var es = GetClient();
            es.DeleteIndex(EsClient.MemesIndex).Wait();
        }

        static EsClient GetClient() => new EsClient(EsClient.MemesIndex, EsClient.Url);

        public static void SaveToEs()
        {
            var date = DateTimeOffset.UtcNow.ToString("yy-MM-dd-hh");
            var path = $"{dataFolder}converted{date}.json";
            var str = File.ReadAllText(path);
            var memes = JsonConvert.DeserializeObject<List<UnifiedMeme>>(str);
            GetClient().Save(memes).GetAwaiter().GetResult();
            Console.WriteLine("Successfully saved to Elastic cluster!");
        }
    }
}
