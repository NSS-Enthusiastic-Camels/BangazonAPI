using BangazonApi;
using System;
using System.Collections.Generic;

namespace BangazonAPI 
{
    public class Customer 
    {
    
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public List<Product> products = new List<Product>();
        public List<PaymentType> paymentTypes = new List<PaymentType>();
    }
    


}