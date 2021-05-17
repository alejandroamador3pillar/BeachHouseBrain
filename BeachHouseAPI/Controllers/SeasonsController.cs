using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using BeachHouseAPI.Repositories;
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
        private readonly ISeasonsRepository _repository;

        public object Summaries { get; private set; }

        public SeasonController(ISeasonsRepository repository)
        {
            _repository = repository;
        }

        //listado
        [HttpGet("/season")]
        public async Task<ActionResult<IEnumerable<Seasons>>> GetSeasonsAsync()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            if (_repository.ValidateUser(user_id, 3))
            {
                var seasons = await _repository.GetSeasons(); ;
                return Ok(seasons.ToList());

            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }
        }

        //actualizar
        [HttpPut("/season")]
        public async Task<ActionResult> UpdateSeason([FromBody] SeasonDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            if (_repository.ValidateUser(user_id, 1))
            {
                var resul = await _repository.UpdateSeason(value);
                if (resul == 200)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Season doesnt exist");
                }
            }
            else
            {
                return Unauthorized("You dont have permision to perform this action");
            }
        }
        
        //insert
        [HttpPost("/season/insert")]
        public async Task<ActionResult> Insert([FromBody] SeasonDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            if (_repository.ValidateUser(user_id, 1))
            {
                var resul = await _repository.Insert(value);
                if (resul == 200)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Already exists");
                }
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }
            
            
        }

    }
}
