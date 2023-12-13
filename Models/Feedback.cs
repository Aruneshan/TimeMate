using System.ComponentModel.DataAnnotations;
#nullable disable

namespace TimeMate.Models;

public class Feedback
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Rating is required")]
    [Range(1, 5, ErrorMessage = "Rating should be between 1 and 5")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; }
}