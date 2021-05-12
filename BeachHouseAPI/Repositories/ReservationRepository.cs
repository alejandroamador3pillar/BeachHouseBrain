using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using BeachHouseAPI.Serializers;
using Microsoft.EntityFrameworkCore;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly BeachHouseDBContext _context;
        private string apiKeySendGridA;
        private string apiKeySendGridB;

        public object Summaries { get; private set; }
        public ReservationRepository(BeachHouseDBContext context)
        {
            _context = context;
            apiKeySendGridA = ConfigurationManager.AppSettings.Get("SendGridKeyA"); //PLEASE request sendgrid keys if you need to test email 
            apiKeySendGridB = ConfigurationManager.AppSettings.Get("SendGridKeyB");
        }

        public IEnumerable<AvailableDatesSerializer> GetAvailableDates(AvailableDatesDTO value)
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

        public async Task<int> Reserve(string user_id, string requestor, ReservationDTO value)
        {
            Users user, req;
            Reservations res;
            user = GetUser(user_id);
            req = GetUser(requestor);

            if (user == null || user == null)
            {
                return 401;
            }
            else if (user.Id != req.Id && req.Role == 0)
            {
                return 401;
            }
            else if (user.Active == false)
            {
                return 4011;            }
            else
            {
                if (value.Nights > 6)
                {
                    return 500;
                }
                else
                {
                   var diff = DaysLeft(requestor);
                    if ( diff == -1) {
                        res = new Reservations();
                        res.Date = DateTime.UtcNow;
                        res.LocationId = 1;
                        res.UserId = user.Id;
                        res.Active = true;
                        res.Notified = false;
                        res.User = user;
                        _context.Reservations.Add(res);

                        await _context.SaveChangesAsync();
                        if (ValidDates(value.StartDate, value.Nights))
                        {
                            CreateDetailLines(res.Id, value.StartDate, value.Nights);
                            await _context.SaveChangesAsync();
                            //await CreateAuditRecord(res.Id, req.Id, "Reserve");
                            SendReservationEmail(res);
                            return 200;
                        }
                        else
                        {
                            _context.Remove(res);
                            await _context.SaveChangesAsync();
                            return 5001;
                        }
                    }
                    else
                    {
                        return diff;
                    }

                    
                }


            }

        }

        public async Task<int> Cancel(string user_id, string res_id)
        {
            Users user;
            Reservations res;
            user = GetUser(user_id);
            res = GetReservation(res_id);

            if (user == null)
            {
                return 501;
            }
            if (res == null)
            {
                return 404;
            }
            else if (user.Active == false)
            {
                return 4011;
            }
            else if (user.Id != res.UserId && user.Role == 0)
            {
                return 401;
            }
            else if (res == null || res.Active == false)
            {
                return 500;
            }
            else
            {
                foreach (var det in res.ReservationDetails)
                {
                    _context.Remove(det);
                }

                res.Active = false;
                await _context.SaveChangesAsync();
                SendCancelEmail(res);
                //await CreateAuditRecord(res.Id, user.Id, "Cancel");
                return 200;
            }
        }

        public int EmailReminder(string key)
        {
            if (key == apiKeySendGridB)
            {
                var email_list = GetLastCancellationDayReservations();

                foreach (Reservations res in email_list)
                {
                    SendReminderEmail(res);
                }
                return 200;
            }
            else
            {
                return 401;
            }
        }

        public int ValidateReport(string user_id, string startDate, string endDate, string mode)
        {
            Users user = GetUser(user_id);
            if (user == null)
            {
                return 401;
            }
            else if (IsDate(startDate) == false || IsDate(endDate) == false)
            {
                return 500;
            }
            else if (user.Role == 1)
            {
                return 200;
            }
            else
            {
                return 401;
            }
        }

        public IEnumerable<ReservationsReportDTO> ReservationsReport(string user_id, string startDate, string endDate, string mode)
        {
            var list = new List<ReservationsReportDTO>();
            var res = GetReservations(DateTime.Parse(startDate), DateTime.Parse(endDate), mode);

            foreach (Reservations r in res)
            {
                var record = new ReservationsReportDTO();

                record.Id = r.Id;
                record.Date = r.Date;
                record.UserId = r.UserId;
                record.UserName = r.User.FirstName.Trim() + " " + r.User.LastName.Trim();
                record.Nights = r.ReservationDetails.Count();
                record.TotalRate = r.ReservationDetails.Sum(x => x.Rate);
                record.Status = r.Active;

                list.Add(record);
            }

            return list;
        }

        public int ValidateUserReservation(string user_id, string requestor, string mode)
        {
            Users user = GetUser(user_id);
            Users req = GetUser(requestor);
            if (user.Id != requestor && req.Role == 0)
            {
                return 401;
            }
            else
            {
                return 200;
            }
        }
        public IEnumerable<ReservationsReportDTO> UserReservations(string user_id, string requestor, string mode)
        {
            Users user = GetUser(user_id);
            Users req = GetUser(requestor);

            var list = new List<ReservationsReportDTO>();
            var res = GetUserReservations(user_id, mode);

            foreach (Reservations r in res)
            {
                var record = new ReservationsReportDTO();

                record.Id = r.Id;
                record.Date = r.Date;
                record.UserId = r.UserId;
                record.UserName = r.User.FirstName.Trim() + " " + r.User.LastName.Trim();
                record.Nights = r.ReservationDetails.Count();
                record.TotalRate = r.ReservationDetails.Sum(x => x.Rate);
                record.Status = r.Active;

                list.Add(record);
            }

            return list;
        }



        //Private methods


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

        private IEnumerable<Reservations> GetReservations(DateTime startDate, DateTime endDate, string mode)
        {
            //modes: 0=All Reservations
            //       1=Active
            //       2=Cancelled

            var res = _context.Reservations
                    .Include(p => p.ReservationDetails)
                    .Include(p => p.User);

            var filter = res.Where(p => p.ReservationDetails.FirstOrDefault().Date.Date >= startDate && p.ReservationDetails.FirstOrDefault().Date.Date <= endDate);


            IQueryable<Reservations> filter1 = filter;
            if (mode == "1")
            {
                filter1 = filter.Where(p => p.Active == true);
            }
            else if (mode == "2")
            {
                filter1 = res.Where(p => p.Active == false && p.Date >= startDate && p.Date <= endDate);
            }

            return filter1.ToList();
        }

        private IEnumerable<Reservations> GetUserReservations(string user_id, string mode)
        {
            //modes: 0=All Reservations
            //       1=Active
            //       2=Cancelled

            var res = _context.Reservations
                    .Include(p => p.ReservationDetails)
                    .Include(p => p.User)
                    .Where(p => p.UserId == user_id);


            IQueryable<Reservations> filter1 = res;
            if (mode == "1")
            {
                filter1 = res.Where(p => p.Active == true);
            }
            else if (mode == "2")
            {
                filter1 = res.Where(p => p.Active == false);
            }

            return filter1.ToList();
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

            string htmltag = "<BR> Check in: " + res.Date.ToShortDateString() + " " + checkin.Value + " Check Out: " + res.Date.AddDays(res.ReservationDetails.Count).ToShortDateString() + " " + checkout.Value + "<BR>";

            return htmltag;
        }

        private void SendReservationEmail(Reservations res)
        {
            var subject = " Beach House Reservation ID:" + res.Id + " has been created!";
            var to = new EmailAddress(res.User.Email.Trim(), res.User.FirstName.Trim() + " " + res.User.LastName.Trim());
            var toAdmin = GetAdminEmailAddress();

            var plainTextContent = "Reservation ID:" + res.Id + " date: " + res.Date.ToShortDateString() + " nights: " + res.ReservationDetails.Count();

            var htmlContent = "<p>Hello <strong> " + res.User.FirstName.Trim() + " " + res.User.LastName.Trim() + "</strong>,</p><p> Thanks for reserving the ASEITHSMUS Bejuco Beach House.This is your reservation summary:<p>" +
               getCheckInCheckOut(res) + "<br>" + "<strong> Total Nights:</strong> " + res.ReservationDetails.Count() + " <strong> Total Charge:</strong> $" + res.ReservationDetails.Sum(x => x.Rate) + " (USD) <br/> " +
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

        public static bool IsDate(string tempDate)
        {
            DateTime fromDateValue;
            var formats = new[] { "dd/MM/yyyy", "yyyy-MM-dd", "MM-dd-yyyy" };
            if (DateTime.TryParseExact(tempDate, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fromDateValue))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ValidateUser(string id, int tvalid)
        {
            Users user;
            user = GetUser(id);


            if (id != null && user != null)
            {
                bool flag = false;


                switch (tvalid)
                {
                    case 1:
                        if (user.Active == true)
                            flag = true;
                        break;
                    case 2:
                        if (user.Active == true && user != null && user.Role != 0)
                            flag = true;
                        break;
                    case 3:
                        if (user != null) //&& user !=null)
                            flag = true;
                        break;
                }

                return flag;
            }
            else
            {
                return false;
            }

        }

        private async Task CreateAuditRecord(long res_id, string user_id, string operation)
        {
            //Operation: 0 = Create Reserve, 1 = Cancel 

            var log = new ReservationLog();
            log.ReservationId = res_id;
            log.UserId = user_id;
            log.Operation = operation;
            log.Date = DateTime.Now;
            _context.ReservationLog.Add(log);
            await _context.SaveChangesAsync();
        }

        public int DaysLeft(string requestor)
        {
            var days = Convert.ToInt16(_context.Params.FirstOrDefault(r => r.Id == 5).Value);
            var today = DateTime.Now;
            var r = _context.Reservations.Where(r => (r.UserId == requestor)).OrderByDescending(r => r.Date).FirstOrDefault();


            if (r != null)
            {
                TimeSpan dif = (today - r.Date);
                var diff = dif.Days;
                if (diff < days)
                {
                    Console.WriteLine(r.Date);
                    Console.WriteLine(today);
                    Console.WriteLine(diff);
                    return (int)(today -r.Date).Days;
                }

            }

            return -1;
        }
    }
}
