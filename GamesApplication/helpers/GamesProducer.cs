using GamesApplication.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using System.Xml.Linq;

namespace GamesApplication.helpers
{
    public class GamesProducer
    {
        public static string site_url = "https://futbolme.com/";

        private static GamesApplicationContext db = new GamesApplicationContext();

        public static  async Task ProduceAsync(BufferBlock<string> queue)
        {
            WebClient client = new WebClient();
            string html = await client.DownloadStringTaskAsync(new Uri(site_url));
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var references = doc.DocumentNode.SelectNodes("//a[@href]");
            foreach (HtmlNode node in references)
            {
                string link = node.Attributes["href"].Value;
                if (link.Contains("resultados-directo/partido/"))
                {
                    string url = site_url + link;
                    queue.Post(url);
                }
               
            }
            queue.Complete();
        }

        

    
    }
}