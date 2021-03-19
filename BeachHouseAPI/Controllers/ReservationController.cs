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

        [HttpGet("/reservation/available_dates")]
        public ActionResult<IEnumerable<AvailableDatesSerializer>> GetAvailableDates([FromBody] AvailableDatesDTO value)
        {
            if (value == null)
            {
                return NotFound(); ////// revisar
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

        [HttpPost("/reservation")]
        public async Task<ActionResult> Reserve([FromBody] ReservationDTO value)
        {
            string header;
            long user_id;
            header = Request.Headers.First(header => header.Key == "user_id").Value.FirstOrDefault();

             user_id = long.Parse(header.ToString());

            Users user;
            Reservations res;
            user = GetUser(user_id);

            if (user == null)
            {
                return Unauthorized();
            }
            else  if (user.Active == false) 
            {
                return Unauthorized();
            }
            else
            {
                res = new Reservations();
                res.Date = DateTime.UtcNow;
                res.LocationId = 1;
                res.UserId = user_id;
                res.Active = true;
                _context.Reservations.Add(res);                
                
                await _context.SaveChangesAsync();
                CreateDetailLines(res.Id, value.StartDate, value.Nights);
                return Ok();
            }
        }

        private Users GetUser(long id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
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

        private void CreateDetailLines(long res_id, DateTime startDate, int nights) 
        {

            ReservationDetails line;
            for (DateTime day = startDate; day < startDate.AddDays(nights); day = day.AddDays(1))
            {
                line = new ReservationDetails();
                line.ReservationId = res_id;
                line.Rate = 999;
                line.Date = day;
                _context.ReservationDetails.Add(line);
            }
            _context.SaveChangesAsync();
        }

    }
}
