using Microsoft.Extensions.DependencyInjection;
using Serilog;
using StateMachine.Tests;
using Xunit.Abstractions;

namespace StateMachine.Helpers;

public class ServicesFixture : IDisposable
{
    readonly ServiceCollection _serviceCollection = new();

    ServiceProvider? _serviceProvider;

    public ServiceProvider Services => _serviceProvider ?? throw new Exception("You must call Build before resolving!");

    public void ConfigureServices(Action<ServiceCollection> configureServices) => configureServices(_serviceCollection);

    public T? GetService<T>() => Services.GetService<T>();

    public T GetRequiredService<T>() where T : notnull => Services.GetRequiredService<T>();

    public void Build() => _serviceProvider = _serviceCollection.BuildServiceProvider();

    protected void RegisterXunitLogger(IServiceCollection services, IMessageSink messageSink) =>
        services.AddLogging(
            l => l
                .AddSerilog(XunitLoggerFactory.CreateLogger(messageSink))
        );

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}