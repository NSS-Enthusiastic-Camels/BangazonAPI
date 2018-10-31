using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BangazonApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]


    public class TrainingProgramController : Controller
    {
        private readonly IConfiguration _config;

        public TrainingProgramController(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //GET All Training Programs

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string sql = @" select
                tp.Id,
                tp.Title,
                tp.Capacity,
                tp.StartDate,
                tp.EndDate
                from TrainingProgram tp
                where 1 = 1
            ";

            using (IDbConnection conn = Connection)
            {

                IEnumerable<TrainingProgram> tps = await conn.QueryAsync<TrainingProgram>(sql);
                return Ok(tps);
            }

        }






    }
}
