using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IConfiguration _config;
        public EmployeeController(IConfiguration config)
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

        // GET api/Employee?q=Taco
        public async Task<IActionResult> Get()
        {
            string sql = @"
            select
                e.Id,
                e.FirstName,
                e.LastName,
                e.IsSuperVisor,
                e.DepartmentId,
                c.Id, 
                c.PurchaseDate,
                d.Id,
                d.Name,
                d.Budget
               FROM Employee e
            join Department d on e.DepartmentId = d.Id
            left join ComputerEmployee ce on ce.EmployeeId = e.Id
            left join Computer c on ce.ComputerId = c.Id";
            using (IDbConnection conn = Connection)
            {
                var employeeQuerySet = await conn.QueryAsync<Employee,Computer,Department, Employee>(
                    sql,
                    (employee,computer,department) =>
                    {
                        employee.DepartmentName = department.Name;
                        employee.Computer = computer;
                        return employee;
                    }
                );
                return Ok(employeeQuerySet);
            }
           
        }

        // GET api/Employee/5
        [HttpGet("{id}", Name = "GetEmployee")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
           select 
            e.Id, e.FirstName, e.LastName, e.DepartmentId,d.Id,
            d.Name, c.Id, c.PurchaseDate
            FROM Employee e 
            join Department d on e.DepartmentId = d.Id
            left join ComputerEmployee ce on ce.EmployeeId = e.Id
            left join Computer c on ce.ComputerId = c.Id
            WHERE e.Id = {id}";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Employee> employees = await conn.QueryAsync<Employee, Department, Computer, Employee >(
                    sql,
                    (employee, department, computer) =>
                    {
                        employee.DepartmentName = department.Name;
                        employee.Computer = computer;
                        return employee;
                    }
                );
                return Ok(employees);
            }
        }

        // POST api/Employees
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {
            string sql = $@"INSERT INTO Employee
            (FirstName,LastName,DepartmentId,IsSuperVisor)
            VALUES
            (
                '{employee.FirstName}',
                '{employee.LastName}',
                '{employee.DepartmentId}',
                '{employee.IsSuperVisor}'
                
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                employee.Id = newId;
                return CreatedAtRoute("GetEmployee", new { id = newId }, employee);
            }
        }


        // PUT api/Employee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Employee employee)
        {
            string sql = $@"
            UPDATE Employee
            SET 
                FirstName = '{employee.FirstName}',
                LastName = '{employee.LastName}',
                DepartmentId = '{employee.DepartmentId}',
                IsSuperVisor = '{employee.IsSuperVisor}'
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
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }



        // DELETE api/Employee/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            string sql = $@"DELETE FROM Employee WHERE Id = {id}";

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

        private bool EmployeeExists(int id)
        {
            string sql = $"SELECT Id FROM Employee WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Product>(sql).Count() > 0;
            }
        }
    }
}





























    

