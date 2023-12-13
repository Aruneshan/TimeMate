#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TimeMate.Areas.Identity.Data;
using TimeMate.Models;
namespace TimeMate.ViewModels
{
    public class SubmitTimesheetViewModel
    {
        public int TimeSheetId { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public string Task { get; set; }
        public string TaskType { get; set; }
        public int HoursSpent { get; set; }
        public string status { get; set; }
        public int projectId { get; set; }
        public int EMployeeId { get; set; }


    }
}
