using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class Reservations
    {
        public Reservations()
        {
            ReservationDetails = new HashSet<ReservationDetails>();
        }

        public long Id { get; set; }
        public DateTime Date { get; set; }
        public long UserId { get; set; }
        public long LocationId { get; set; }

        public virtual Locations Location { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<ReservationDetails> ReservationDetails { get; set; }
    }
}
