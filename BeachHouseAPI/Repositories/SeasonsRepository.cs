using BeachHouseAPI.DTOs;
using BeachHouseAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeachHouseAPI.Repositories
{
    public class SeasonsRepository: ISeasonsRepository
    {
        private readonly BeachHouseDBContext _context;
        public SeasonsRepository(BeachHouseDBContext context)
        {
            _context = context;
        }
        public Seasons GetSeason(int id)
        {
            Seasons season;
            season = _context.Seasons.FirstOrDefault(e => e.Id == id);

            return season;
        }

        public async Task<IEnumerable<Seasons>> GetSeasons()
        {
            return await _context.Seasons.ToListAsync();
        }

        public Users GetUser(string id)
        {
            Users user;
            user = _context.Users.FirstOrDefault(e => e.Id == id);

            return user;
        }

        public async Task<int>  Insert(SeasonDTO value)
        {
            Seasons season;
            season = GetSeason(value.Id);

            if (season == null)
            {
                season = new Seasons();
                season.Active = true;
                season.Typeseason = 1;
                season.Startdate = (DateTime)value.StartDate;
                season.Enddate = (DateTime)value.EndDate;

                _context.Seasons.Add(season);
                await _context.SaveChangesAsync();
                return 200;


            }
            else
            {
                return 400;
            }
        }

        public async Task<int> UpdateSeason(SeasonDTO value)
        {
            Seasons season;
            season = GetSeason(value.Id);
            if (season != null)
            {
                season.DescriptionSeason = value.descriptionSeason;
                season.Startdate = (DateTime)value.StartDate;
                season.Enddate = (DateTime)value.EndDate;
                _context.Update(season);
                await _context.SaveChangesAsync();
                return 200;
            }
            else
            {
                return 400;
            }
            
                
            
        }

        public bool ValidateUser(string id, int tvalid)
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
    }
}
