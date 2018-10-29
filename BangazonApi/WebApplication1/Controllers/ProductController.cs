using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace BangazonApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ProductController(IConfiguration config)
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

                // GET api/students?q=Taco
        [HttpGet]
        public async Task<IActionResult> Get(string q)
        {
            string sql = @"
            SELECT
                p.Id,
                p.Title,
                p.Price,
                p.Description,
                p.Quantity,
                c.Id,
                c.FirstName,
                c.LastName,
                t.Id,
                t.Name
            FROM Product p
            JOIN Customer c ON p.CustomerId = c.Id
            JOIN ProductType t ON p.ProductTypeId = t.Id
            WHERE 1=1
            ";

            Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Product> products = await conn.QueryAsync<Product, Customer, ProductType, Product>(
                    sql,
                    (product, customer, productType) =>
                    {
                        product.Customer = customer;
                        product.ProductType = productType;
                        return product;
                    }
                );
                return Ok(products);
            }
        }

        // GET api/students/5
        [HttpGet("{id}", Name = "GetProduct")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                p.Id,
                p.Title,
                p.Price,
                p.Description,
                p.Quantity,
                c.Id,
                c.FirstName,
                c.LastName,
                t.Id,
                t.Name
            FROM Product p
            JOIN Customer c ON p.CustomerId = c.Id
            JOIN ProductType t ON p.ProductTypeId = t.Id
           WHERE p.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Product> products = await conn.QueryAsync<Product, Customer, ProductType, Product>(
                    sql,
                    (product, customer, productType) =>
                    {
                        product.Customer = customer;
                        product.ProductType = productType;
                        return product;
                    }
                );
                return Ok(products.Single());
            }
        }

        // Add new one
        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            string sql = $@"INSERT INTO Product
            (Title, Price, Description, Quantity, CustomerId, ProductTypeId)
            VALUES
            (
                '{product.Title}'
                ,'{product.Price}'
                ,'{product.Description}'
                ,'{product.Quantity}'
                ,'{product.CustomerId}'
                ,'{product.ProductTypeId}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                product.Id = newId;
                return CreatedAtRoute("GetProduct", new { id = newId }, product);
            }
        }
        //edit
        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Product product)
        {
            string sql = $@"
            UPDATE Product
            SET 
                Title = '{product.Title}',
                Price = {product.Price},
                Description = '{product.Description}',
                Quantity = {product.Quantity},
                CustomerId = {product.CustomerId},
                ProductTypeId = {product.ProductTypeId}
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
                if (!ProductExists(id))
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
            string sql = $@"DELETE FROM Product WHERE Id = {id}";

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

        private bool ProductExists(int id)
        {
            string sql = $"SELECT Id FROM Product WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Product>(sql).Count() > 0;
            }
        }



        }
    }





 
