using System.ComponentModel.DataAnnotations;
#nullable disable

namespace TimeMate.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
        ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "SMTP Server")]
        public string SmtpServer { get; set; }

        [Required]
        [Display(Name = "SMTP Port")]
        public int SmtpPort { get; set; }

        [Required]
        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "SMTP Password")]
        public string SmtpPassword { get; set; }
    }

}
