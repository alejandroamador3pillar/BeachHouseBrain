using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserEditDTO>> GetUsers(string user_id);
        int ValidateUser(string user_id);
        Task<int> SignIn(string user_id, UserDTO value);
        int ValidateUser2(string user_id, string id);
        Users GetUser(string id);
        Task<int> UpdateUser(string user_id, UserEditDTO value);
        bool isAdmin(string user_id);
    }
}
