using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class Params
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Value { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long UpdatedBy { get; set; }
        public DateTime LastModified { get; set; }

    }
}
