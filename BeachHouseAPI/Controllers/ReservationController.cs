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
using BeachHouseAPI.Repositories;

namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationRepository _repository;
        private readonly BeachHouseDBContext _context;

        public object Summaries { get; private set; }

        public ReservationController(IReservationRepository repository, BeachHouseDBContext context)
        {
            _repository = repository;
        }



        [HttpGet("/reservation/price/{datetime}/{num_days}")]
        public IActionResult GetPrice(DateTime datetime,int num_days)
        {
            var sum = _repository.GetPrice(datetime, num_days);
            return Ok(sum);
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
                return Ok( _repository.GetAvailableDates(value));
            }
        }

        [HttpPost("/reservation")]
        public async Task<ActionResult> Reserve([FromBody] ReservationDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            string requestor = Request.Headers.FirstOrDefault(header => header.Key == "requestor").Value;

            var resul = await _repository.Reserve(user_id, requestor, value);

            if(resul == 200)
            {
                
                    return Ok();

            }else if (resul == 401)
            {
                return Unauthorized("You have no permission to perform this action");
            }else if (resul == 4011)
            {
                return Unauthorized("Your user has been deactivated by admin");
            }
            else if (resul == 500)
            {
                return BadRequest("Reservations longer than 6 days are not allowed");
            }
            else if(resul == 5001)
            {
                return BadRequest("Date range was invalid at the moment of creation. Check your dates.");
            }
            else
            {
                return BadRequest("You must wait "+ resul +" days before making another reservation");
            }
        }


        [HttpPut("/reservation/cancel")]
        public async Task<ActionResult> Cancel()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            string res_id = Request.Headers.FirstOrDefault(header => header.Key == "res_id").Value;

            var resul = await _repository.Cancel(user_id, res_id);
            if(resul == 200)
            {
                return Ok();
            }
            else if(resul == 404)
            {
                return NotFound("Reservation does not exist");
            }else if (resul == 401)
            {
                return Unauthorized("You have no permission to perform this action");
            }else if (resul == 4011)
            {
                return Unauthorized("Your user has been deactivated by admin");
            }
            else
            {
                return BadRequest("Invalid Reservation");
            }
        }

        [HttpPost("/reservation/reminder")]
        public ActionResult EmailReminder()
        {
            string key = Request.Headers.FirstOrDefault(header => header.Key == "auth").Value;
            var resul = _repository.EmailReminder(key);

            if(resul == 200)
            {
                return Ok();
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }

        }

        [HttpGet("/reservation/reservation_report")]
        public ActionResult<IEnumerable<ReservationsReportDTO>> ReservationsReport()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            string startDate = Request.Headers.FirstOrDefault(header => header.Key == "start_date").Value;
            string endDate = Request.Headers.FirstOrDefault(header => header.Key == "end_date").Value;
            string mode = Request.Headers.FirstOrDefault(header => header.Key == "mode").Value;



            var resul = _repository.ValidateReport(user_id, startDate, endDate, mode);
            if (resul == 200)
            {
                return Ok(_repository.ReservationsReport(user_id, startDate, endDate, mode));
            }
            else if (resul==401)
            {
                return Unauthorized("You have no permission to perform this action");
            }
            else if (resul==500)
            {
                return BadRequest("Date Range is invalid");
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }
        }

        [HttpGet("/reservation/user_reservations")]
        public ActionResult<IEnumerable<ReservationsReportDTO>> UserReservations()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            string requestor = Request.Headers.FirstOrDefault(header => header.Key == "requestor").Value;

            string mode = Request.Headers.FirstOrDefault(header => header.Key == "mode").Value;
            var valid = _repository.ValidateUserReservation(user_id, requestor, mode);

            if (valid == 200)
            {
                return Ok(_repository.UserReservations(user_id, requestor, mode));
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }
            
        }

        

    
    }
}
