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
        private string apiKeySendGridA;
        private string apiKeySendGridB;

        public object Summaries { get; private set; }

        public ReservationController(BeachHouseDBContext context)
        {
            _context = context;
            apiKeySendGridA = ConfigurationManager.AppSettings.Get("SendGridKeyA"); //PLEASE request sendgrid keys if you need to test email 
            apiKeySendGridB = ConfigurationManager.AppSettings.Get("SendGridKeyB");
        }

        [HttpPost("/reservation/available_dates")]
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
            else if (user.Active == false)
            {
                return Unauthorized("Your user has been deactivated by admin");
            }
            else
            {
                if (value.Nights > 6)
                {
                    return BadRequest("Reservations longer than 6 days are not allowed");
                }
                else
                {
                    res = new Reservations();
                    res.Date = DateTime.UtcNow;
                    res.LocationId = 1;
                    res.UserId = user_id;
                    res.Active = true;
                    res.Notified = false;
                    _context.Reservations.Add(res);

                    await _context.SaveChangesAsync();
                    if (ValidDates(value.StartDate, value.Nights))
                    {
                        CreateDetailLines(res.Id, value.StartDate, value.Nights);
                        await _context.SaveChangesAsync();
                        SendReservationEmail(res);
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
        }

        [HttpPut("/reservation/cancel")]
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
            if (res == null)
            {
                return NotFound("Reservation does not exist");
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
                res.CreatedBy = user.Id;
                res.Active = false;
                await _context.SaveChangesAsync();
                SendCancelEmail(res);
                return Ok();
            }
        }

        [HttpPost("/reservation/reminder")]
        public ActionResult EmailReminder()
        {
            string key = Request.Headers.FirstOrDefault(header => header.Key == "auth").Value;
            if (key == apiKeySendGridB)
            {
                var email_list = GetLastCancellationDayReservations();

                foreach (Reservations res in email_list)
                {
                    SendReminderEmail(res);
                }
                return Ok();
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
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
                    .Include(p => p.User)
                    .Where(p => p.Id.ToString() == id).FirstOrDefault();

            return res;
        }

        private IEnumerable<Reservations> GetLastCancellationDayReservations()
        {
            var res = _context.Reservations
                    .Include(p => p.ReservationDetails)
                    .Include(p => p.User)
                    .Where(p => p.ReservationDetails.FirstOrDefault().Date.Date == DateTime.Now.AddDays(6).Date)
                    .Where(p => p.Active == true)
                    .Where(p => p.Notified == false); 

            return res.ToList();
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

        private EmailAddress GetAdminEmailAddress()
        {
            Params param = _context.Params.FirstOrDefault(e => e.Id.ToString() == ConfigurationManager.AppSettings.Get("NotificationParamId"));
            return new EmailAddress(param.Value, "BeachHouseAPI Admin");
        }

        private string getTAndCUrl()
        {
            Params param = _context.Params.FirstOrDefault(e => e.Id.ToString() == ConfigurationManager.AppSettings.Get("TermsConditionsParamId"));
            return param.Value;
        }

        private string getCheckInCheckOut(Reservations res)
        {
            Params checkin = _context.Params.FirstOrDefault(e => e.Id.ToString() == ConfigurationManager.AppSettings.Get("CheckInParamId"));
            Params checkout = _context.Params.FirstOrDefault(e => e.Id.ToString() == ConfigurationManager.AppSettings.Get("CheckOutParamId"));

            string htmltag = "<BR> Check in: " + res.Date.ToShortDateString() + " "+ checkin.Value + " Check Out: " + res.Date.AddDays(res.ReservationDetails.Count).ToShortDateString() + " " + checkout.Value + "<BR>";

            return htmltag;
        }

        private void SendReservationEmail(Reservations res)
        {
            var subject = " Beach House Reservation ID:" + res.Id + " has been created!";
            var to = new EmailAddress(res.User.Email.Trim(), res.User.FirstName.Trim() + " " + res.User.LastName.Trim());
            var toAdmin = GetAdminEmailAddress();

            var plainTextContent = "Reservation ID:" + res.Id + " date: " + res.Date.ToShortDateString() + " nights: " + res.ReservationDetails.Count();

             var htmlContent = "<p>Hello <strong> " + res.User.FirstName.Trim() + " " + res.User.LastName.Trim() + "</strong>,</p><p> Thanks for reserving the ASEITHSMUS Bejuco Beach House.This is your reservation summary:<p>" +
                getCheckInCheckOut(res) + "<br>" + "<strong> Total Nights:</strong> " + res.ReservationDetails.Count() + " <strong> Total Charge:</strong> $" + res.ReservationDetails.Sum(x => x.Rate) +" (USD) <br/> " +
                "<a href = " + getTAndCUrl() + "> Terms and Conditions</a></p><p> Please remember that cancellations are allowed until one week before the Check In date, right after that date no cancellations can be made.</p> " +
                "<p> Bejuco Beach House Google Maps link <a href =" + getTAndCUrl() + "> here </a></p><p> Regards,<br/><strong> Bejuco Beach House Administration Team</strong></p>";

            var emailToUser = SendEmailAsync(subject, to, plainTextContent, htmlContent);
            var emailToAdmin = SendEmailAsync(subject, toAdmin, plainTextContent, htmlContent);
        }

        private void SendReminderEmail(Reservations res)
        {
            var subject = " Reminder of your upcoming reservation. ID:" + res.Id + ".";
            var to = new EmailAddress(res.User.Email.Trim(), res.User.FirstName.Trim() + " " + res.User.LastName.Trim());

            var plainTextContent = "Reservation ID:" + res.Id + " date: " + res.Date.ToShortDateString() + " nights: " + res.ReservationDetails.Count();
            var htmlContent = "<p> Hello <strong> " + res.User.FirstName.Trim() + " " + res.User.LastName.Trim() + "/ <strong>,</p><br> <p> This is a friendly reminder of your upcoming reservation.</p>" +
                getCheckInCheckOut(res) + "<br> Total Nights:</strong> " + res.ReservationDetails.Count() + "<strong> Total Charge: </strong> $" + res.ReservationDetails.Sum(x => x.Rate) + " (USD) <br/></p><p><a href =" + getTAndCUrl() + "> Terms and Conditions</a></p> " +
                "<p> If you want to cancel the reservation please click <a href= " + getTAndCUrl() + "> here </a> to be redirected to the system for cancellation." +
                "<p> Bejuco Beach House Google Maps link <a href=" + getTAndCUrl() + " > here </a></p>" +
                "<p> Regards,<br/><strong> Bejuco Beach House Administration Team </strong></p> ";

            var emailToUser = SendEmailAsync(subject, to, plainTextContent, htmlContent);
            res.Notified = true;
            _context.SaveChanges();

        }

        private void SendCancelEmail(Reservations res)
        {
            var subject = " Beach House Reservation ID:" + res.Id + " has been canceled!";
            var to = new EmailAddress(res.User.Email.Trim(), res.User.FirstName.Trim() + " " + res.User.LastName.Trim());
            var toAdmin = GetAdminEmailAddress();

            var plainTextContent = "Reservation " + res.Id + " has been cancelled.";
            var htmlContent = "<strong>" + "Your reservation " + res.Id + " has been cancelled. <br>If required, we'll call you to:" + res.User.Phone + "  <br> <a href = " + getTAndCUrl() + " > Terms And Conditions </ a > </strong>"; ;

            var emailToUser = SendEmailAsync(subject, to, plainTextContent, htmlContent);
            var emailToAdmin = SendEmailAsync(subject, toAdmin, plainTextContent, htmlContent);
        }

        private async Task SendEmailAsync(string subject, EmailAddress to, string plainTextContent, string htmlContent)
        {
            var apiKey = apiKeySendGridA + apiKeySendGridB;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("alertsbeachhouse@outlook.com", "Beach House Alerts");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }

    }
}
