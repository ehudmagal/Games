using GamesApplication.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GamesApplication.helpers
{
    public class GamesConsumer
    {
        public static async Task ConsumeAsync(BufferBlock<string> queue)
        {
            object lockObject = new object();
            double hours_threshold = 2.0;
            GamesApplicationContext db = new GamesApplicationContext();
            WebClient client = new WebClient();
            while (await queue.OutputAvailableAsync())
            {
                string url = await queue.ReceiveAsync();
                string html = await client.DownloadStringTaskAsync(new Uri(url));
                var doc = new HtmlDocument();
                doc.LoadHtml(html);               
                string[] teams = find_teams(doc);                
                Game game = new Game();
                game.CompetitionName = find_contest_name(doc);
                game.StartTime = time(doc);
                game.TeamName1 = teams[0];
                game.TeamName2 = teams[1];
                game.SportType = get_sport_type();
                lock (lockObject)
                {
                    if (validate_game(game))
                    {
                        db.Games.Add(game);
                        db.SaveChanges();
                    }
                }
                
            }

            bool validate_game(Game game)
            {
                DateTime t = game.StartTime.AddHours(-2);
                var list =  db.Games.Where(g => g.CompetitionName == game.CompetitionName &&
                              g.SportType == game.SportType &&
                              g.TeamName1 == game.TeamName1 &&
                              g.TeamName2 == game.TeamName2 &&         
                              g.StartTime.Year == game.StartTime.Year &&
                              g.StartTime.Month == game.StartTime.Month &&
                              g.StartTime.Day == game.StartTime.Day &&
                              t.Hour <= g.StartTime.Hour).ToList();
              
                return list.Count == 0;
            }


            string get_sport_type()
            {
                return "Football";
            }

         
            DateTime time(HtmlDocument doc)
            {
                var node = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'row greenbox nomargin')]");
                string t = node.SelectSingleNode("//span[@class='pull-right']").InnerHtml.Trim();
                string[] time_arr = t.Split(' ');
                string timestr =  time_arr[time_arr.Length - 1];
                DateTime date = DateTime.Today;
                int hour = Int32.Parse(timestr.Split(':')[0]);
                int minute = Int32.Parse(timestr.Split(':')[1]);
                TimeSpan ts = new TimeSpan(hour, minute, 0);
                return date.Date + ts;
            }

            string[] find_teams(HtmlDocument doc)
            {
              
                string[] data = description(doc).Split(new string[] { "::" }, StringSplitOptions.None);
                string[] teams = data[0].Split('-');
                return teams;
            }

            string find_contest_name(HtmlDocument doc)
            {
                string[] data = description(doc).Split(new string[] { "::" }, StringSplitOptions.None);
                return data[1];
            }

            string description(HtmlDocument doc){
                var description_node = doc.DocumentNode.SelectSingleNode("//meta[contains(@name, 'description')]");
                return description_node.Attributes["content"].Value;
            }
           
        }
    }
}