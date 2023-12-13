using System.ComponentModel.DataAnnotations;

namespace TimeMate.Models;

public class Feedback
{

    public string ? Name { get; set; }
    public string ? Email { get; set; }
    public int Rating { get; set; }

    public string ? Message { get; set; }

}