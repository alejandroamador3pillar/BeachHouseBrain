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
using BeachHouseAPI.Repositories;

namespace BeachHouseAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ParamController : ControllerBase
    {
        private readonly IParamRepository _repository;
        public object Summaries { get; private set; }

        public ParamController(IParamRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("/params")]
        public IActionResult GetParams()
        {
            return Ok(this._repository.GetParams());
        }

        [HttpPut("/params")]
        public async Task<ActionResult> UpdateParam([FromBody] ParamDTO value)
        {
            string user_id = Request.Headers.FirstOrDefault(header => header.Key == "user_id").Value;

            var valid = _repository.ValidateParam(user_id,value);

            if (valid == 200)
            {
                await _repository.UpdateParam(user_id, value);
                return Ok();
                
            }else if (valid == 401)
            {
                return Unauthorized();
            }
            else
            {
                
                return NotFound();
            }
        }

    }
}
