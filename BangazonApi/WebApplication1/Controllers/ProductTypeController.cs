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
	public class ProductTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductTypeController(IConfiguration config)
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
                p.Id,
                p.Name
               
            FROM ProductType p
            ";

            if (q != null)
            {
                string isQ = $@"
                    AND p.Name LIKE '%{q}%'
              
                ";
                sql = $"{sql} {isQ}";
            }

            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<ProductType> producttypes = await conn.QueryAsync<ProductType>(sql);
                return Ok(producttypes);
            }
        }

        // GET api/cohorts/5
        [HttpGet("{id}", Name = "GetProductType")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                p.Id,
                p.Name
           from ProductType p
            where p.Id= {id}
               
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<ProductType> producttypes = await conn.QueryAsync<ProductType>(sql);
                return Ok(producttypes);
            }
        }



        // POST api/cohorts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductType producttype)
        {
            string sql = $@"INSERT INTO ProductType 
            (Name)
            VALUES
            (
                '{producttype.Name}'
                
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                producttype.Id = newId;
                return CreatedAtRoute("GetProductType", new { id = newId }, producttype);
            }
        }
        // PUT api/ProductType/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ProductType producttype)
        {
            string sql = $@"
            UPDATE ProductType
            SET Name = '{producttype.Name}'
              
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
                if (!ProductTypeExists(id))
                {
                    throw;
                }
                else
                {
                    return NotFound();
                }
            }
        }
        // DELETE api/ProductType/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM ProductType WHERE Id = {id}";

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

        private bool ProductTypeExists(int id)
        {
            string sql = $"SELECT Id FROM ProductType WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<ProductType>(sql).Count() > 0;
            }
        }
    }
}
