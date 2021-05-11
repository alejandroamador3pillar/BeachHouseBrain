using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly BeachHouseDBContext _context;
        public UserRepository(BeachHouseDBContext context)
        {
            _context = context;

        }

        public async Task<IEnumerable<UserEditDTO>> GetUsers(string user_id)
        {
             var list = await _context.Users.ToListAsync();
             var list2 = new List<UserEditDTO>();
             for (int i = 0; i < list.Count; i++)
             {
               var n = new UserEditDTO();
               var c = list.ElementAt(i);
               n.Id = c.Id;
               n.FirstName = c.FirstName;
               n.LastName = c.LastName;
               n.Active = c.Active;
               n.Email = c.Email;
               n.Role = c.Role;
               n.Phone = c.Phone;

               list2.Add(n);

             }
             return list2;
            
        }

        public async Task<int> SignIn(string user_id, UserDTO value)
        {
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
                return 200;
            }
            else if (user.Active == false)
            {
                return 401;
            }
            else
            {
                return 200;
            }
        }

        public int ValidateUser(string user_id)
        {
            if (ValidateUser(user_id, 1))
            {
                return 200;
            }
            else
            {
                return 500;
            }
        }

        public int ValidateUser2(string user_id, string id)
        {
            if (ValidateUser(user_id, 1))
            {
                if (ValidateUser(id, 3))
                {

                    Users user;
                    user = GetUser(id);

                    if (ValidateUser(user.Id, 3))
                    {
                        return 200;
                    }
                    else
                    {
                        return 404;
                    }
                }
                else
                {
                    return 404;
                }
            }
            else
            {
                return 401;
            }
        }

        public Users GetUser(string id)
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

        public async Task<int> UpdateUser(string user_id, UserEditDTO value)
        {
            if (ValidateUser(user_id, 1))
            {
                Users user;
                user = GetUser(value.Id);

                if (user_id != value.Id)
                {
                    if (ValidateUser(user_id, 2) != true)
                    {
                        return 401;
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
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        return 200;
                    }
                    else
                    {
                        return 404;
                    }
                else
                {
                    return 404;
                }
            }
            else
            {
                return 401;
            }
        }
    }
}
