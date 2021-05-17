using AutoMapper;
using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using BeachHouseAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _repository;

        public object Summaries { get; private set; }

        public UserController(IUserRepository repository)
        {
            _repository = repository;
            
        }

        [HttpGet("/user")]
        public async Task<ActionResult<IEnumerable<UserEditDTO>>> GetUsers()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            var valid = _repository.ValidateUser(user_id);
            
            
            if (valid == 200)
            {
                var l =await _repository.GetUsers(user_id);
                
                return Ok(l.ToList());
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }
        }

        [HttpPost("/user/sign_in")]
        public async Task<ActionResult> SignIn([FromBody] UserDTO value)
        {
            string header;
            string user_id;
            header = Request.Headers.First(header => header.Key == "user_id").Value.FirstOrDefault();

            user_id = header.ToString();

            var resul = await _repository.SignIn(user_id, value);

            if(resul == 200)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet("/user/{id}")]
        public ActionResult<Users> GetUser()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            string id = Url.ActionContext.RouteData.Values["id"].ToString();

            var valid = _repository.ValidateUser2(user_id, id);

            if(valid == 200)
            {
                return Ok(_repository.GetUser(id));
            }else if(valid == 401)
            {
                return Unauthorized("You have no permission to perform this action");
            }
            else
            {
                return NotFound("User not exist");
            }

        }


        //**

        [HttpPut("/user")]
        public async Task<ActionResult> UpdateUser([FromBody] UserEditDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            var resul = await _repository.UpdateUser(user_id,value);

            if(resul == 200)
            {
                return Ok();
            }else if(resul == 401)
            {
                return Unauthorized("You have no permission to perform this action");
            }
            else
            {
                return NotFound("User not exist");
            }

        }
        
        [HttpGet("/isAdmin")]
        public IActionResult isAdmin()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;
            if (_repository.isAdmin(user_id)){
                return Ok(200);
            }
            else
            {
                return Ok(401);
            }

        }
        //**
    }
}