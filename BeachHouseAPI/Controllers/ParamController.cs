using BeachHouseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeachHouseAPI.DTOs;

using System.Web.Http.Cors;

namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParamController : ControllerBase
    {
        private readonly BeachHouseDBContext _context;

        public object Summaries { get; private set; }

        public ParamController(BeachHouseDBContext context)
        {
            _context = context;
        }

        [HttpGet("/params")]
        public async Task<ActionResult<IEnumerable<Params>>> GetParams()
        {
            return await _context.Params.ToListAsync();
        }

        [HttpPost("/params")]
        public async Task<ActionResult> UpdateParam([FromBody] ParamDTO value)
        {
            Params param;
            param = GetParam(value.Id);

            if (param == null)
            {
                return NotFound();
            }
            else
            {
                param.Value = value.Value;
                param.StartDate = value.StartDate;
                param.EndDate = value.EndDate;
                await _context.SaveChangesAsync();
                return Ok();
            }
        }

        private Params GetParam(long id)
        {
            Params param;
            param = _context.Params.FirstOrDefault(e => e.Id == id);

            return param;
        }


    }
}
