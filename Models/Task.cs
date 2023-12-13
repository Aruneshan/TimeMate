using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
#nullable disable

namespace TimeMate.Models
{
    public class Tasks
    {
        [Key]
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        

        // Navigation properties

        [ForeignKey("Workspace")]
        public int WorkspaceId { get; set; }
        public WorkSpace Workspace { get; set; }
        //public int projectId { get; set; }
        //public Project Project { get; set; }

    }
}
