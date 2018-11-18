using Serilog;
using Serilog.Events;

namespace Shared
{
    public static class Logging
    {
        public static ILogger Configure(string scope, LogEventLevel level = LogEventLevel.Debug)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console()
                .CreateLogger()
                .ForContext("Scope", scope);
            return logger;
        }
    }
}
