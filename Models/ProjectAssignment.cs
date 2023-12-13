using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;
#nullable disable


namespace TimeMate.Models
{
    public class ProjectAssignment
    {
        [Key]
        public int ProjectAssignmentId { get; set; }
        public string status { get; set; }  
        [ForeignKey("Project")]
        public int projectId { get; set; }
        public Project Project { get; set; }

        [ForeignKey("TimeMateUser")]
        public string EmployeeId { get; set; }
        public TimeMateUser TimeMateUser { get; set; }
    }
}
