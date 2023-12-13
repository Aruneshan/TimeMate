using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable disable

namespace TimeMate.Models
{
    public partial class Project
    {
        [Key]
        public int projectId { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        [Display(Name = "Date Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd}")]
        public DateTime DateCreated { get; set; }

        [ForeignKey("WorkSpace")]
        public int WorkspaceId { get; set; }
        public WorkSpace WorkSpace { get; set; }

    }
}
