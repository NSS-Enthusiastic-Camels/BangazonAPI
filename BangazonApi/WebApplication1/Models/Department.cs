using System;
using System.Collections.Generic;

namespace BangazonApi
{
    public class Department
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public int Budget { get; set; }

        public List<Product> employees = new List<Product>();
    }
}
