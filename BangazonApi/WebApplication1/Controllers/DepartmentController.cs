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
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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
        public async Task<IActionResult> Get( string _filter, int _gt)
        {
            string sql = @"
            SELECT
                d.Id,
                d.Name,
                d.Budget
            FROM Department d
            WHERE 1=1
            ";

            if (_gt != null && _filter == "budget")
            {
                string isGt = $@"
                    AND d.Budget > '{_gt}'
       
                ";
                sql = $"{sql} {isGt}";
            }

            // Console.WriteLine(sql);

            using (IDbConnection conn = Connection)
            {

                IEnumerable<Department> departments = await conn.QueryAsync<Department>(
                    sql
                );
                return Ok(departments);
            }
        }
        /*
        // GET api/students/5
        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute]int id)
        {
            string sql = $@"
            SELECT
                d.Id,
                d.Name,
                d.Budget
            FROM Department d
            WHERE d.Id = {id}
            ";

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Department> departments = await conn.QueryAsync<Department>(
                    sql
                );
                return Ok(departments.Single());
            }
        }*/


        // GET api/Customer/5?_include=products
        //this  GET api/Department/2?_include=employees
        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> Get([FromRoute]int id, string _include)
        {
            string sql = $@"
            SELECT
                d.Id,
                d.Name,
                d.Budget
            FROM Department d
            WHERE d.Id = {id}
            ";


            if (_include != null)
            {
                if (_include == "employees")
                {
                    Dictionary<int, Department> report = new Dictionary<int, Department>();

                    IEnumerable<Department> custAndProd = Connection.Query<Department, Employee, Department>(
                       $@"
                    SELECT 
                        d.Id,
                        d.Name,
                        d.Budget,

                        e.Id,
                        e.FirstName,
                        e.LastName,
                        e.IsSuperVisor,
                        e.DepartmentId
                       
                    FROM Department d
                    JOIN Employee e ON d.Id = e.DepartmentId
                    WHERE d.Id = {id};
                ",
                        (generatedDepartment, generatedEmployee) => {
                            if (!report.ContainsKey(generatedDepartment.Id))
                            {
                                report[generatedDepartment.Id] = generatedDepartment;
                            }

                            report[generatedDepartment.Id].employees.Add(generatedEmployee);

                            return generatedDepartment;
                        }
                    );

                    return Ok(report.Values);
                }

               
            }

            using (IDbConnection conn = Connection)
            {
                IEnumerable<Department> department = await conn.QueryAsync<Department>(sql);
                return Ok(department);
            }
        }




        // Add new one
        // POST api/students
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Department department)
        {
            string sql = $@"INSERT INTO Department
            (Name, Budget)
            VALUES
            (
                '{department.Name}'
                ,'{department.Budget}'
            );
            SELECT SCOPE_IDENTITY();";

            using (IDbConnection conn = Connection)
            {
                var newId = (await conn.QueryAsync<int>(sql)).Single();
                department.Id = newId;
                return CreatedAtRoute("GetDepartment", new { id = newId }, department);
            }
        }

        //edit
        // PUT api/students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Department department)
        {
            string sql = $@"
            UPDATE Department
            SET 
                Name = '{department.Name}',
                Budget = {department.Budget}
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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool DepartmentExists(int id)
        {
            string sql = $"SELECT Id FROM Department WHERE Id = {id}";
            using (IDbConnection conn = Connection)
            {
                return conn.Query<Department>(sql).Count() > 0;
            }
        }


    }
}