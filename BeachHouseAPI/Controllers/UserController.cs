using BeachHouseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.User.ToListAsync();
        }

        // POST: api/Movies
        [HttpPost("/user")]
        public async Task<ActionResult<User>> SingIn(User user)
        {
            if (UserExists(user.Id) == false)
            {
                _context.User.Add(user);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
