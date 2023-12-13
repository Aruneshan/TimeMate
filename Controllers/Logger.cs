using Serilog;
using ILogger = Serilog.ILogger;

namespace TimeMate.Controllers
{
    public class Logger
    {
        private static readonly ILogger _logger;

        static Logger()
        {
            // Configure Serilog to log messages to a file
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void LogError(string message)
        {
            _logger.Error("[ERROR] {Timestamp:yyyy-MM-dd HH:mm:ss}: {Message}", DateTime.Now, message);
        }

        public static void LogInfo(string message)
        {
            _logger.Information("[INFO] {Timestamp:yyyy-MM-dd HH:mm:ss}: {Message}", DateTime.Now, message);
        }
    }
}
