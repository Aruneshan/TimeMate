using System;
using System.Collections.Generic;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;
#nullable disable

namespace TimeMate.Models
{
    public partial class Week
    {
        public Week()
        {
            TimeSheet = new HashSet<TimeSheet>();
        }

        public int WeekId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string Pmid { get; set; }

        public virtual TimeMateUser Pm { get; set; }
        public virtual ICollection<TimeSheet> TimeSheet { get; set; }
    }
}
