using BeachHouseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Web.Http.Cors;

namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly BeachHouseDBContext _context;

        public object Summaries { get; private set; }

        public UserController(BeachHouseDBContext context)
        {
            _context = context;
        }

 
        [HttpGet("/user")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpPost("/user/sign_in/test")]
        public async Task<ActionResult> SignIn()
        {
            string header;
            long user_id;
            header = Request.Headers.First(header => header.Key == "user_id").Value.FirstOrDefault();

            user_id = long.Parse(header.ToString());

            Users user;
            user = GetUser(user_id);

            if (user == null)
            {
                user = new Users();
                user.Id = user_id;
                user.Role = 0;
                user.Active = true;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else if (user.Active == false)
            {
                return Unauthorized();
            }
            else 
            {
                return Ok();
            }
        }

        private Users GetUser(long id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
        }

        private Users GetIdFromHeader(long id)
        {

            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
        }
    }
}
