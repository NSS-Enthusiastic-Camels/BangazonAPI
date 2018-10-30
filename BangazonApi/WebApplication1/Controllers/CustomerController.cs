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
    
        public class CustomerController : ControllerBase
    {

        private readonly IConfiguration _config;

        public  CustomerController(IConfiguration config)
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


        // GET api/Customer/5?_include=products
        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get([FromRoute]int id, string _include)
        {
            string sql = $@"
            SELECT
                c.Id,
                c.FirstName,
                c.LastName
            FROM Customer c
            WHERE c.Id = {id}
            ";

            if (_include != null)
            {
                if (_include == "products")
                {
                    Dictionary<int, Customer> report = new Dictionary<int, Customer>();

                    IEnumerable<Customer> custAndProd = Connection.Query<Customer, Product, Customer>(
                       $@"
                    SELECT c.Id,
                        c.FirstName,
                        c.LastName,
                        p.Id,
                        p.Title,
                        p.Price,
                        p.Quantity,
                        p.Description,
                        p.ProductTypeId,
                        p.CustomerId
                    FROM Customer c
                    JOIN Product p ON c.Id = p.CustomerId
                    WHERE c.Id = {id};
                ",
                        (generatedCustomer, generatedProduct) => {
                            if (!report.ContainsKey(generatedCustomer.Id))
                            {
                                report[generatedCustomer.Id] = generatedCustomer;
                            }

                            report[generatedCustomer.Id].products.Add(generatedProduct);

                            return generatedCustomer;
                        }
                    );

                    return Ok(report);
                }

                if (_include == "payments")
                {
                   // Dictionary<int, Payment> report = new Dictionary<int, Customer>();


                }
            }

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Customer> customer = await conn.QueryAsync<Customer>(sql);
                return Ok(customer);
            }
        }

        //Post a new customer
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer cust)
        {
            string sql = $@"INSERT INTO Customer 
            (FirstName, LastName)
            VALUES
            (
                '{cust.FirstName}'
                ,'{cust.LastName}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                cust.Id = newId;
                return CreatedAtRoute("GetCustomer", new { id = newId }, cust);
            }
        }

        //Put customer - edit
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Customer cust)
        {
            string sql = $@"
            UPDATE Customer
            SET FirstName = '{cust.FirstName}',
                LastName = '{cust.LastName}'
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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

        }



            private bool CustomerExists(int id)
            {
                string sql = $"SELECT Id FROM Customer WHERE Id = {id}";
                using (IDbConnection conn = Connection)
                {
                    return conn.Query<Customer>(sql).Count() > 0;
                }
            }

    }
}
