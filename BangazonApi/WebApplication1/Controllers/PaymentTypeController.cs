using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.AspNetCore.Http;

namespace BangazonApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentTypeController : ControllerBase
	{
		private readonly IConfiguration _config;

		public PaymentTypeController(IConfiguration config)
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

		// GET api/paymenttype
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			string sql = @"
            SELECT
                p.Id,
				p.AcctNumber,
				p.Name,
				p.CustomerId
            FROM PaymentType p
            WHERE 1=1
            ";

			Console.WriteLine(sql);

			using (IDbConnection conn = Connection)
			{

				IEnumerable<PaymentType> paymentType = await conn.QueryAsync<PaymentType>(
					sql
				);
				return Ok(paymentType);
			}
		}

		// GET api/paymenttype/5
		[HttpGet("{id}", Name = "GetPaymentType")]
		public async Task<IActionResult> Get([FromRoute]int id)
		{
			string sql = $@"
            SELECT
                p.Id,
				p.AcctNumber,
				p.Name,
				p.CustomerId
            FROM PaymentType p
            WHERE p.Id = {id}
            ";

			using (IDbConnection conn = Connection)
			{
				IEnumerable<PaymentType> paymenttypes = await conn.QueryAsync<PaymentType>(sql);
				return Ok(paymenttypes);
			}
		}

		// POST api/paymenttype
		[HttpPost]
		public async Task<IActionResult> Post([FromBody] PaymentType paymentType)
		{
			string sql = $@"INSERT INTO PaymentType 
            (AcctNumber, Name, CustomerId)
            VALUES
            (
                '{paymentType.AcctNumber}'
                ,'{paymentType.Name}'
				,'{paymentType.CustomerId}'
            );
            SELECT SCOPE_IDENTITY();";

			using (IDbConnection conn = Connection)
			{
				var newId = (await conn.QueryAsync<int>(sql)).Single();
				paymentType.Id = newId;
				return CreatedAtRoute("GetPaymentType", new { id = newId }, paymentType);
			}
		}

		// PUT api/paymenttype/5
		[HttpPut("{id}")]
		public async Task<IActionResult> Put(int id, [FromBody] PaymentType paymentType)
		{
			string sql = $@"
            UPDATE PaymentType
            SET AcctNumber = '{paymentType.AcctNumber}',
                Name = '{paymentType.Name}'
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
				if (!PaymentTypeExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
		}

		// DELETE api/paymenttype/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			string sql = $@"DELETE FROM PaymentType WHERE Id = {id}";

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

		private bool PaymentTypeExists(int id)
		{
			string sql = $"SELECT Id FROM PaymentType WHERE Id = {id}";
			using (IDbConnection conn = Connection)
			{
				return conn.Query<PaymentType>(sql).Count() > 0;
			}
		}
	}

}