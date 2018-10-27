using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Flurl.Http;

using Newtonsoft.Json;

using Shared;

namespace TextDetectorPoller
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new EsClient(EsClient.MemesIndex, EsClient.Url);
            var hasMore = true;
            while (hasMore)
            {
                try
                {
                    hasMore = ProcessBatch(client).Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error");
                }
            }

            Console.WriteLine("Hello World!");
        }

        const string textDetectorUrl = "http://172.20.10.10:8889/predict";

        static async Task<bool> ProcessBatch(EsClient client)
        {
            var memes = await client.GetUnprocessed();
            if (!memes.Any()) return false;
            var response = await DetectText(memes);
            foreach (var resp in response)
            {
                var found = memes.First(x => x.Id == resp.Id);
                found.DetectedText = resp.Text;
            }
            Console.WriteLine($"Detected for {response.Length} of {memes.Count}");

            foreach (var meme in memes)
            {
                meme.DetectedText = meme.DetectedText ?? "";
            }
            await client.Save(memes);
            return true;
        }

        static async Task<DetectorResponse[]> DetectText(List<UnifiedMeme> memes)
        {
            var urls = memes.Select(x => new
            {
                Id = x.Id,
                Url = x.Media.FirstOrDefault()?.Url
            });

            var processed = await textDetectorUrl.PostJsonAsync(urls.Where(x => x.Url != null).ToList()).ReceiveString();
            var deserialised = JsonConvert.DeserializeObject<DetectorResponse[]>(processed);
            return deserialised;
        }

        class DetectorResponse
        {
            public string Id;
            public string Text;
        }
    }
}
