using System;
using System.Collections.Generic;

namespace BeachHouseAPI.Models
{
    public partial class ReservationLog
    {
        public int Id { get; set; }
        public long ReservationId { get; set; }
        public string UserId { get; set; }
        public string Operation { get; set; }
        public DateTime? Date { get; set; }

        public virtual Reservations Reservation { get; set; }
        public virtual Users User { get; set; }
    }
}
