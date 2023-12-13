using System.ComponentModel.DataAnnotations;
using TimeMate.Areas.Identity.Data;
#nullable disable

namespace TimeMate.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }

        [Display(Name = "employee ID")]
        public string EmployeeId { get; set; }
        public TimeMateUser employee { get; set; } 
        [Required]
        public DateTime startDate { get; set; }
        [Required]
        public DateTime endDate { get; set; }
        [Required]
        public string Reason { get; set; }
        public LeaveStatus status { get; set; }

        [Display(Name = "Compensatory Date")]
        public DateTime? CompensatoryDate { get; set; }

        [Display(Name = "Manager Approval")]
        public LeaveStatus ManagerApproval { get; set; }

    }
}
