using System;
using System.Collections.Generic;

#nullable disable

namespace BeachHouseAPI.Models
{
    public partial class Param
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
