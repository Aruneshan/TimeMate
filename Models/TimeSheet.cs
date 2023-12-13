using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;
#nullable disable

namespace TimeMate.Models
{
    public partial class TimeSheet
    {
        [Key]
        public int TimeSheetId { get; set; }
        public DateTime Date { get; set; }
        public string Task { get; set; }
        public string TaskType { get; set; }
        public int HoursSpent { get; set; }
        public string status { get; set; }

        [ForeignKey("TimeMateUser")]
        public string EmployeeId { get; set; }
        public TimeMateUser TimeMateUser { get; set;}

        [ForeignKey("Project")]
        public int projectId { get; set; }
        public Project Project { get; set; }
    }
}
