using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BangazonApi;


namespace BangazonApi
{
    public class TrainingProgram
    {
        public int Id { get; set; }

        public int MaxAttendees { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
