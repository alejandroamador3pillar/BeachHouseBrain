using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.DTOs
{
    public class ReservationDTO
    {
        public DateTime StartDate { get; set; }
        public int LocationId { get; set; }
        public int Nights { get; set; }
        public string CreatedBy { get; set; }
        


    }
}
