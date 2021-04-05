using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.DTOs
{
    public class UserEditDTO:UserDTO
    {
        public string Id { get; set; }
        public int Role { get; set; }
        public bool Active { get; set; }

    }
}
