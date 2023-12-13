#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMate.ViewModels
{
    public class CreateTimesheetViewModel
    {
        [Required]

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required]
        public string Task { get; set; }
        [Display(Name = "Task Type")]
        [Required]
        public string TaskType { get; set; }
        [Display(Name = "Hours Spent")]
        public int HoursSpent { get; set; }
        [Display(Name ="Start Date")]
        public DateTime WeekStartDate { get; set; }

        [Display(Name = "File")]
        public IFormFile File { get; set; }
    }
}
