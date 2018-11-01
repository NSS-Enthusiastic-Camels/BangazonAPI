using BangazonApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonApi
{
    public class Employee
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsSuperVisor { get; set; }

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        //public Computer Computer { get; set; }
    }

}
