using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BangazonApi;


namespace BangazonApi
{
    public class TrainingProgram
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Capacity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
