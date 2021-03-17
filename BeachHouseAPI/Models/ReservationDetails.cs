using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class ReservationDetails
    {
        public long Id { get; set; }
        public long ReservationId { get; set; }
        public DateTime Date { get; set; }
        public long? Rate { get; set; }

        public virtual Reservations Reservation { get; set; }
    }
}
