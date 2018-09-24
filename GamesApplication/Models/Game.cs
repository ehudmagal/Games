using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamesApplication.Models
{
    public class Game
    {
        public int ID { get; set; }
        public string CompetitionName { get; set; }
        public DateTime StartTime { get; set; }
        public string TeamName1 { get; set; }
        public string TeamName2 { get; set; }
        public string SportType { get; set; }
    }
}