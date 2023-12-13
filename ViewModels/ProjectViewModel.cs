#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TimeMate.ViewModels
{
    public class ProjectViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string description { get; set; }

        public DateTime DateCreated { get; set; }
        public SharedData SharedData { get; set; }
    }
}
