using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Serializers
{
    public partial class AvailableDatesSerializer
    {
        public DateTime Date { get; set; }
        public bool Available { get; set; }
        public long Rate { get; set; }
    }
}
