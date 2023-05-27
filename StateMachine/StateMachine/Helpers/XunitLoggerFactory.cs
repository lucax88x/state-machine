using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Xunit.Abstractions;

namespace StateMachine.Helpers;

public static class XunitLoggerFactory
{
    public static Logger CreateLogger(ITestOutputHelper? testOutputHelper)
    {
        var logger = new LoggerConfiguration();

        if (testOutputHelper is not null)
        {
            logger = logger
                .WriteTo
                .TestOutput(testOutputHelper);
        }

        return logger.CreateLogger();
    }

    public static Logger CreateLogger(IMessageSink? messageSink)
    {
        var logger = new LoggerConfiguration();

        if (messageSink is not null)
        {
            logger = logger
                .WriteTo
                .TestOutput(messageSink);
        }

        return logger.CreateLogger();
    }
}