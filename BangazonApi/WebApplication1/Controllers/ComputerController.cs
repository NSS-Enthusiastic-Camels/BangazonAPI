using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BangazonAPI;
using Microsoft.AspNetCore.Http;

namespace BangazonApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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



        // GET api/ProductType?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                c.Id,
                c.PurchaseDate,
                c.DecomissionDate

               
            FROM Computer c
            ";

            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Computer> computers = await conn.QueryAsync<Computer>(sql);
                return Ok(computers);
            }
        }

        // GET api/students/5
        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.PurchaseDate,
                c.DecomissionDate
            FROM Computer c
            WHERE c.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Computer> computers = await conn.QueryAsync<Computer>(
                    sql
                );
                return Ok(computers.Single());
            }
        }

        //Post a new Computer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            string sql = $@"INSERT INTO Computer
            (PurchaseDate, DecomissionDate)
            VALUES
            (
                '{computer.PurchaseDate}'
                ,'{computer.DecomissionDate}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                computer.Id = newId;
                return CreatedAtRoute("GetCustomer", new { id = newId }, computer);
            }
        }

        //edit
        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Computer computer)
        {
            string sql = $@"
            UPDATE Computer
            SET 
                PurchaseDate = '{computer.PurchaseDate}',
                DecomissionDate = '{computer.DecomissionDate}'
            WHERE Id = {id}";

            // Console.WriteLine(sql);
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
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE api/students/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Computer WHERE Id = {id}";

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

        private bool ComputerExists(int id)
        {
            string sql = $"SELECT Id FROM Computer WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Computer>(sql).Count() > 0;
            }
        }


    }
}