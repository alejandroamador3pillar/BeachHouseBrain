using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public interface ISeasonsRepository
    {
        Seasons GetSeason(int id);
        Users GetUser(string id);
        bool ValidateUser(string id, int tvalid);
        Task<IEnumerable<Seasons>> GetSeasons();
        Task<int> UpdateSeason(SeasonDTO value);
        Task<int> Insert(SeasonDTO value);

    }
}
