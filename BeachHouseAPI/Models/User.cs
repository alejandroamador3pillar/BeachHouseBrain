using System;
using System.Collections.Generic;

#nullable disable

namespace BeachHouseAPI.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public int Role { get; set; }
        public bool? Active { get; set; }
    }
}
