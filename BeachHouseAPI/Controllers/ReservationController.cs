using BeachHouseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeachHouseAPI.DTOs;
using BeachHouseAPI.Serializers;
using System.Net.Mail;
using System.Net.Mime;
using SendGrid;
using SendGrid.Helpers.Mail;

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
            string user_id;
            header = Request.Headers.First(header => header.Key == "user_id").Value.FirstOrDefault();

             user_id = header.ToString();

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
                if (ValidDates(value.StartDate, value.Nights))
                {
                    CreateDetailLines(res.Id, value.StartDate, value.Nights);
                    await _context.SaveChangesAsync();
                    SendReservationEmailAsync();
                    return Ok();
                }
                else
                {
                    _context.Remove(res);
                    await _context.SaveChangesAsync();
                    return BadRequest();
                }
            }
        }

        private Users GetUser(string id)
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

        private bool ValidDates(DateTime startDate, int nights)
        {
            bool valid = true;
            for (DateTime day = startDate; day < startDate.AddDays(nights); day = day.AddDays(1))
            {
                if (IsAvailableDate(day) == false)
                {
                    valid = false;
                }
            }
            return valid;
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
        }

        private async Task SendReservationEmailAsync()
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("aleamadorq@gmail.com", "Mail Example User");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("alejandro.amador@3pillarglobal.com", "Example User Mail");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

    }
}
