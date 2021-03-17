using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class Locations
    {
        public Locations()
        {
            Reservations = new HashSet<Reservations>();
        }

        public long Id { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Reservations> Reservations { get; set; }
    }
}
