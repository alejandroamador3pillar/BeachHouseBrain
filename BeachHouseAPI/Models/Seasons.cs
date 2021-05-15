using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class Seasons
    {
        public int Id { get; set; }
        public string DescriptionSeason { get; set; }
        public int Typeseason { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime Enddate { get; set; }
        public bool Active { get; set; }
    }
}
