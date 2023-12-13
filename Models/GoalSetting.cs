using System.ComponentModel.DataAnnotations;
#nullable disable
namespace TimeMate.Models
{
    public class GoalSetting
    {
            [Key]
            public int Id { get; set; }

            [Required]
            [StringLength(255)]
            public string description { get; set; }

            [DataType(DataType.Date)]
            [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
            public DateTime? dueDate { get; set; }

            public bool isComplete { get; set; }

            public string assignedTo { get; set; }
        public string assignedBy { set; get; }

    }
}
