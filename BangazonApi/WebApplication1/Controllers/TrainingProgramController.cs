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
                tp.MaxAttendees,
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

        //GET Single Training Program

        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                tp.Id,
                tp.MaxAttendees,
                tp.StartDate,
                tp.EndDate
                from TrainingProgram tp
            WHERE tp.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<TrainingProgram> tps = await conn.QueryAsync<TrainingProgram>(sql);
                return Ok(tps);
            }

        }


        //Post a new training program
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram tp)
        {
            string sql = $@"INSERT INTO TrainingProgram 
            (MaxAttendees, StartDate, EndDate)
            VALUES
            (
                '{tp.MaxAttendees}',
                '{tp.StartDate}',
                '{tp.EndDate}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                tp.Id = newId;
                return CreatedAtRoute("GetTrainingProgram", new { id = newId }, tp);
            }
        }

        //EDIT TrainingProgram

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TrainingProgram tp)
        {
            string sql = $@"
            UPDATE TrainingProgram
            SET MaxAttendees = '{tp.MaxAttendees}',
                StartDate = '{tp.StartDate}',
                EndDate = '{tp.EndDate}'
            WHERE Id = {id}";

            try
            {
                using (IDbConnection conn = Connection)
                {
                    int rowsAffected = await conn.ExecuteAsync(sql);
                    if (rowsAffected > 0)
                    {
                        return new StatusCodeResult(StatusCodes.Status204NoContent);
                    }
                    throw new Exception("No rows affected");
                }
            }
            catch (Exception)
            {
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

        }



        private bool TrainingProgramExists(int id)
        {
            string sql = $"SELECT Id FROM TrainingProgram WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<TrainingProgram>(sql).Count() > 0;
            }
        }


    }
}
