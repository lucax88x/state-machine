using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StateMachine.Tests;

public class TestStateMachine {
    readonly IServiceProvider _serviceProvider;
    readonly ILogger<TestStateMachine> _logger;

    public TestStateMachine(ILogger<TestStateMachine> logger, IServiceProvider serviceProvider) {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<TestState> Start(Guid id, string parameter1) {
        var stateMachine = new StateMachine<TestState>(id, _serviceProvider);
        stateMachine.Add(
            nameof(TestState.Step1SingleState),
            async (state, sp) => {
                var client1 = sp.GetRequiredService<ITestClient1>();

                return state with { Step1Single = new TestState.Step1SingleState { Value1 = await client1.Call1(parameter1) } };
            }
        );
        stateMachine.Add(
            nameof(TestState.Step2SingleState),
            async (state, sp) => {
                var client1 = sp.GetRequiredService<ITestClient1>();

                return state with { Step2Single = new TestState.Step2SingleState { Value2 = await client1.Call2() } };
            }
        );

        return await stateMachine.Run(TestState.Empty);
    }
}