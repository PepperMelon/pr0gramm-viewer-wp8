using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pr0gramm.Logic
{
    using System.Net;
    using Newtonsoft.Json;

    public static class AzureService
    {
        private const string newsFeedUrl = "myNewsFeed.json"; //hier gehört eine url hin, die ein physikalisches json file verlinkt

        public static Task<List<NewsFeed>> GetNewsFeed()
        {
            var tcs = new TaskCompletionSource<List<NewsFeed>>();
            var client = new WebClient();
            client.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null)
                {
                    tcs.SetException(e.Error);
                    return;
                }

                var newsFeeds = JsonConvert.DeserializeObject<List<NewsFeed>>(e.Result);
                tcs.SetResult(newsFeeds);
            };

            client.DownloadStringAsync(new Uri(newsFeedUrl));
            return tcs.Task;
        }

        
    }

    public class NewsFeed
    {
        public string Date { get; set; }

        public string Text { get; set; }
    }
}
