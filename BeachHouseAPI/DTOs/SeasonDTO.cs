using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.DTOs
{
    public class SeasonDTO
    {
        public int Id { get; set; }
        public string descriptionSeason { get; set; }
        public int typeseason { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Boolean? active { get; set; }
    }
}
