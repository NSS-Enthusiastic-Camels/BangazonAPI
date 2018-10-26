using System.Collections.Generic;

namespace BangazonApi
{
	public class PaymentType
	{
		public int Id { get; set; }
		public int AccountNumber { get; set; }
		public string Name { get; set; }
		public int CustomerId { get; set; }
	}

}