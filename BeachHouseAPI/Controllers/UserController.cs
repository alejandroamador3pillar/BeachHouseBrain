using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
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
        private readonly BeachHouseDBContext _context;

        public object Summaries { get; private set; }

        public UserController(BeachHouseDBContext context)
        {
            _context = context;
        }

        [HttpGet("/user")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            if (ValidateUser(user_id,1))
            {
                return await _context.Users.ToListAsync();
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

            Users user;
            user = GetUser(user_id);

            if (user == null)
            {
                user = new Users();
                user.Id = user_id;
                user.Role = 0;
                user.Active = true;
                user.Email = value.Email;
                user.FirstName = value.FirstName;
                user.LastName = value.LastName;
                user.Phone = value.Phone;
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

        [HttpGet("/user/{id}")] 
        public ActionResult<Users> GetUser()
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value; 
            
            string id = Url.ActionContext.RouteData.Values["id"].ToString();

            if (ValidateUser(user_id, 1))
            {
                if (ValidateUser(id, 3))
                {

                    Users user;
                    user = GetUser(id);

                    if (ValidateUser(user.Id, 3))
                    {
                        return user;
                    }
                    else
                    {
                        return NotFound("User not exist");
                    }
                }
                else
                {
                    return NotFound("User not exist");
                }
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }

        }


        //**

        [HttpPut("/user")]
        public async Task<ActionResult> UpdateUser([FromBody] UserEditDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            if (ValidateUser(user_id,1))
            {
                Users user;
                user = GetUser(value.Id);

                if (user_id != value.Id)
                {
                    if (ValidateUser(user_id, 2)!=true)
                    {
                        return Unauthorized("You have no permission to perform this action");
                    }
                }

                if (user != null)

                    if (ValidateUser(user.Id, 3))
                    {
                        user.Role = value.Role;
                        user.Active = value.Active;
                        user.Email = value.Email;
                        user.FirstName = value.FirstName;
                        user.LastName = value.LastName;
                        user.Phone = value.Phone;
                        await _context.SaveChangesAsync();
                        return Ok();
                    }
                    else
                    {
                        return NotFound("User not exist");
                    }
                else
                {
                    return NotFound("User not exist");
                }
            }
            else
            {
                return Unauthorized("You have no permission to perform this action");
            }

        }
            //**

        private Users GetUser(string id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
        }

        private bool ValidateUser(string id,int tvalid)
        {
            Users user;
            user = GetUser(id);

            if (id!=null  && user != null)
            { 
                bool flag=false;
          
                switch (tvalid)
                {
                    case 1:
                        if (user.Active == true) 
                            flag=true; break;
                    case 2:
                        if (user.Active == true  && user.Role!=0)
                            flag=true; break;
                    case 3:
                        if (user != null )
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
