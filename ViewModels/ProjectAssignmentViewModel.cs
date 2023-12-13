#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMate.ViewModels
{
    public class ProjectAssignmentViewModel
    {
        [Display(Name = "Search Email")]
        [Required]
        public string searchEmail { get; set; }

        public string searchKeyword { get; set; }
    }
}
