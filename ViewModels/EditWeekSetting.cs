#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMate.ViewModels
{
    public class EditWeekSetting
    {
        [Display(Name = "Start Day")]
        public string StartDay { get; set; }
        [Display(Name = "End Day")]
        public string EndDay { get; set; }
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; }
    }
}
