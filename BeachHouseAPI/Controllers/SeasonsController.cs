using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;




namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SeasonController : ControllerBase
    {
        private readonly BeachHouseDBContext _context;

        public object Summaries { get; private set; }

        public SeasonController(BeachHouseDBContext context)
        {
            _context = context;
        }

        //listado
        [HttpGet("/season")]
        public async Task<ActionResult<IEnumerable<Seasons>>> GetUsers()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            if (ValidateUser(user_id, 1))
            {
                return await _context.Seasons.ToListAsync();

            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }
        }




        //actualizar
        [HttpPut("/season")]
        public async Task<ActionResult> UpdateUser([FromBody] SeasonDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;


            Seasons season;
            season = GetSeason(value.Id);


            
            season.DescriptionSeason = value.descriptionSeason;
            season.Startdate = (DateTime)value.StartDate;
            season.Enddate = (DateTime)value.EndDate;


            _context.Update(season);
            await _context.SaveChangesAsync();
            return Ok();

        }

        
        //insert
        [HttpPost("/seson/insert")]
        public async Task<ActionResult> Insert([FromBody] SeasonDTO value)

        {      
            Seasons season;
            
            season = GetSeason(value.Id);

            if (season == null)
            {
                season = new Seasons();
                season.Active = true;
                season.Typeseason = 1;
                season.Startdate = (DateTime)value.StartDate;
                season.Enddate = (DateTime)value.EndDate;

                _context.Seasons.Add(season);
                await _context.SaveChangesAsync();

                return Ok();
            }
            return BadRequest("Already exists");
        }


       



        //**

        private Seasons GetSeason(int id)
        {
            Seasons season;
            season = _context.Seasons.FirstOrDefault(e => e.Id == id);

            return season;
        }

        private Users GetUser(string id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
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
                            flag = true; break;
                    case 2:
                        if (user.Active == true && user.Role != 0)
                            flag = true; break;
                    case 3:
                        if (user != null)
                            flag = true; break;
                }
                return flag;
            }
            else
            {
                return false;
            }
        }








    }
}
