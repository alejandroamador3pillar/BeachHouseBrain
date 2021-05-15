using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public class ParamRepository: IParamRepository
    {
        private readonly BeachHouseDBContext _context;
        public ParamRepository(BeachHouseDBContext context)
        {
            _context = context;
        }
        public IEnumerable<Params> GetParams()
        {
            return this._context.Params.ToList();
        }

        public async Task UpdateParam(string user_id, ParamDTO value)
        {
            Users user;
            user = GetUser(user_id);
            Params param;
            param = GetParam(value.Id);
            param.Value = value.Value;
            param.Description = value.Description;
            param.StartDate = value.StartDate;
            param.EndDate = value.EndDate;
            param.LastModified = DateTime.UtcNow;
            param.UpdatedBy = user.Id;
            _context.Update(param);
            await _context.SaveChangesAsync();
        }

        public int ValidateParam(string user_id, ParamDTO value)
        {
            Users user;
            user = GetUser(user_id);

            if (user == null)
            {
                return 401;
            }
            else
            {
                Params param;
                param = GetParam(value.Id);

                if (param == null)
                {
                    return 404;
                }
                else
                {
                    if (user.Active == true && user.Role == 1) //Role 1 = Admin
                    {
                        
                        return 200;
                    }
                    else
                    {
                        return 401;
                    }
                }
            }
        }

        private Params GetParam(long id)
        {
            Params param;
            param = _context.Params.FirstOrDefault(e => e.Id == id);

            return param;
        }

        private Users GetUser(string id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
        }
    }
}
