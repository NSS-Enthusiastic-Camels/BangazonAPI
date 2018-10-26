using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BangazonApi.Controllers
{
        [Route("api/[controller]")]
        [ApiController]
    
        public class CustomersController : ControllerBase
    {

        private readonly IConfiguration _config;

        public  CustomersController(IConfiguration config)
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

        // GET api/customers
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string sql = @"
            SELECT
                c.Id,
                c.FirstName,
                c.LastName
            FROM Customer c
            WHERE 1=1
            ";

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Customer> customers = await conn.QueryAsync<Customer>(sql);
                return Ok(customers);
            }
        }





    }
}
