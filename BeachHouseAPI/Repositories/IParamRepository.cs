using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public interface IParamRepository
    {
        IEnumerable<Params> GetParams();
        Task UpdateParam(string user_id, ParamDTO value);
        int ValidateParam(string user_id, ParamDTO value);
    }
}
