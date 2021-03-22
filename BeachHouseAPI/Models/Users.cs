using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class Users
    {
        public Users()
        {
            Params = new HashSet<Params>();
            Reservations = new HashSet<Reservations>();
        }

        public string Id { get; set; }
        public int Role { get; set; }
        public bool? Active { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Phone { get; set; }

        public virtual ICollection<Params> Params { get; set; }
        public virtual ICollection<Reservations> Reservations { get; set; }
    }
}
