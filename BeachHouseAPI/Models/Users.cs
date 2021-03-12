using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class Users
    {
        public long Id { get; set; }
        public int Role { get; set; }
        public bool? Active { get; set; }

    }
}
