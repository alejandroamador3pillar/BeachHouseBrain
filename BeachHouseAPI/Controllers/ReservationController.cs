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
using Microsoft.EntityFrameworkCore;
using Microsoft.Web.Administration;
using System.Configuration;

namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly BeachHouseDBContext _context;
        private string apiKeySendGridA = ConfigurationManager.AppSettings["SendGridKeyA"];
        private string apiKeySendGridB = ConfigurationManager.AppSettings["SendGridKeyB"];

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
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            Users user;
            Reservations res;
            user = GetUser(user_id);

            if (user == null)
            {
                return Unauthorized("You have no permission to perform this action");
            }
            else  if (user.Active == false) 
            {
                return Unauthorized("Your user has been deactivated by admin");
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
                    await SendReservationEmailAsync(res);
                    return Ok();
                }
                else
                {
                    _context.Remove(res);
                    await _context.SaveChangesAsync();
                    return BadRequest("Date range was invalid at the moment of creation. Check your dates.");
                }
            }
        }

        [HttpPost("/reservation/cancel")]
        public async Task<ActionResult> Cancel()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            string res_id = Request.Headers.FirstOrDefault(header => header.Key == "res_id").Value;

            Users user;
            Reservations res;
            user = GetUser(user_id);
            res = GetReservation(res_id);

            if (user == null)
            {
                return Unauthorized("You have no permission to perform this action");
            }
            else if (user.Active == false)
            {
                return Unauthorized("Your user has been deactivated by admin");
            }
            else if (user.Id != res.UserId && user.Role == 0) 
            {
                return Unauthorized("You have no permission to perform this action");
            }
            else if (res == null || res.Active == false)
            {
                return BadRequest("Invalid Reservation");
            }
            else
            {
                foreach (var det in res.ReservationDetails)
                {
                    _context.Remove(det);
                }

                res.Active = false;
                await _context.SaveChangesAsync();
                await SendCancelEmailAsync(res);
                return Ok();
            }
        }

        private Users GetUser(string id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
        }

        private Reservations GetReservation(string id)
        {
            Reservations res;
            //res = _context.Reservations.FirstOrDefault(e => e.Id.ToString() == id);
            res = _context.Reservations
                    .Include(p => p.ReservationDetails)
                    .Where(p => p.Id.ToString() == id).FirstOrDefault();

            return res;
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

        private async Task SendReservationEmailAsync(Reservations res)
        {
            var apiKey = apiKeySendGridA + apiKeySendGridB; 
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("beachhousealerts@outlook.com", "Beach House Alerts");
            var subject = " Beach House Reservation ID:" + res.Id + " has been created!";
            var to = new EmailAddress("alejandro.amador@3pillarglobal.com", "Example User Mail");
            var plainTextContent = "Reservation ID:" + res.Id + " date: " + res.Date.ToShortDateString() + " nights: " + res.ReservationDetails.Count();
            var htmlContent = "<strong>" + "Reservation " + res.Id + " has been created on: " + res.Date.ToShortDateString() + " <br> nights: " + res.ReservationDetails.Count() + " <br> From:" + res.ReservationDetails.FirstOrDefault().Date.ToShortDateString() + " to: " + res.ReservationDetails.LastOrDefault().Date.ToShortDateString() + " <br> Total: $" + res.ReservationDetails.Sum(x => x.Rate) + " </strong>" ;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

        private async Task SendCancelEmailAsync(Reservations res)
        {
            var apiKey = apiKeySendGridA + apiKeySendGridB;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("beachhousealerts@outlook.com", "Beach House Alerts");
            var subject = " Beach House Reservation ID:" + res.Id + " has been canceled!";
            var to = new EmailAddress("alejandro.amador@3pillarglobal.com", "Example User Mail");
            var plainTextContent = "Reservation ID:" + res.Id + " date: " + res.Date.ToShortDateString() + " nights: " + res.ReservationDetails.Count();
            var htmlContent = "<strong>" + "Reservation " + res.Id + " has been cancelled. </strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

    }
}
