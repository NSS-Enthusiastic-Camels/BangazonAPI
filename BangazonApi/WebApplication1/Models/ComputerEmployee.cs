using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BangazonApi.Models
{
    public class ComputerEmployee
    {
        public int Id { get; set; }

       
        public DateTime AssignDate { get; set; }

      
        public DateTime UnassignDate { get; set; }

        public int EmployeeId { get; set; }

        public int ComputerId { get; set; }

    }
}
