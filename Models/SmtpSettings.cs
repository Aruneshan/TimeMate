#nullable disable
namespace TimeMate.Models
{
    public class smtpSettings
    {
        public string fromEmail { get; set; }
        public string smtpHost { get; set; }
        public int smtpPort { get; set; }
        public bool enableSsl { get; set; }
        public bool useDefaultCredentials { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }

}
