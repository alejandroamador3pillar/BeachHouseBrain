using BeachHouseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

using System.Web.Http.Cors;
using BeachHouseAPI.DTOs;
using BeachHouseAPI.Serializers;

namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly BeachHouseDBContext _context;

        public object Summaries { get; private set; }

        public ReservationController(BeachHouseDBContext context)
        {
            _context = context;
        }


        [HttpGet("/reservation")]
        public ActionResult<IEnumerable<AvailableDatesSerializer>> GetAvailableDates([FromBody] AvailableDatesDTO value)
        {
            if (value == null)
            {
                return NotFound();
            }
            else
            {
                var dates = new List<AvailableDatesSerializer>();

                // Loop from the first day of the month until we hit the next month, moving forward a day at a time
                for (var date = new DateTime(value.Year, value.Month, 1); date.Month == value.Month; date = date.AddDays(1))
                {
                    var availableDate = new AvailableDatesSerializer
                    {
                        Date = date,
                        Available = IsAvailableDate(date),
                        Rate = 500
                    };
                    dates.Add(availableDate);
                }
                return dates;
            }
        }

        private bool IsAvailableDate(DateTime date)
        {

            var res = _context.ReservationDetails.FirstOrDefault(s => s.Date == date);
            if (res == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
