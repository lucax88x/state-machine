using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StateMachine.Helpers;
using Xunit.Abstractions;

namespace StateMachine.Tests;

public class StateMachineTests : IClassFixture<StateMachineTests.Fixture>
{
    readonly Fixture _fixture;
    readonly TestStateMachine _sut;
    readonly Guid _stateMachineId = Guid.NewGuid();

    public StateMachineTests(Fixture fixture)
    {
        _fixture = fixture;
        _fixture.Build();

        _sut = _fixture.GetRequiredService<TestStateMachine>();

        Fake.ClearRecordedCalls(_fixture.TestClient1Mock);
    }

    [Fact]
    public async Task initial_run_everything_should_be_good_with_empty_state_and_empty_states()
    {
        // GIVEN
        Retrieves(new List<State>(), TestState.Empty);

        Client1Call1Returns("value1");
        Client1Call2Returns(2);

        // WHEN
        var result = await _sut.Start(_stateMachineId, "param1");

        // THEN
        result.Step1Single.Value1.Should().Be("value1");
        result.Step2Single.Value2.Should().Be(2);

        ShouldPersistSuccess(nameof(TestState.Step1SingleState));
        ShouldPersistSuccess(nameof(TestState.Step2SingleState));
        ShouldPersistState("value1", 0);
        ShouldPersistState("value1", 2);
    }

    [Fact]
    public async Task initial_run_everything_should_be_good_with_empty_state_and_not_empty_states()
    {
        // GIVEN
        Retrieves(BuildStates(), TestState.Empty);

        Client1Call1Returns("value1");
        Client1Call2Returns(2);

        // WHEN
        var result = await _sut.Start(_stateMachineId, "param1");

        // THEN
        result.Step1Single.Value1.Should().Be("value1");
        result.Step2Single.Value2.Should().Be(2);

        ShouldPersistSuccess(nameof(TestState.Step1SingleState));
        ShouldPersistSuccess(nameof(TestState.Step2SingleState));
        ShouldPersistState("value1", 0);
        ShouldPersistState("value1", 2);
    }

    [Fact]
    public async Task second_run_with_fetched_state_should_not_persist_state_if_outcome_is_the_same()
    {
        // GIVEN
        Retrieves(new List<State>(), BuildState("value1", 2));

        Client1Call1Returns("value1");
        Client1Call2Returns(2);

        // WHEN
        var result = await _sut.Start(_stateMachineId, "param1");

        // THEN
        result.Step1Single.Value1.Should().Be("value1");
        result.Step2Single.Value2.Should().Be(2);

        ShouldPersistSuccess(nameof(TestState.Step1SingleState));
        ShouldPersistSuccess(nameof(TestState.Step2SingleState));
        ShouldNotPersistState();
    }

    [Fact]
    public async Task run_should_skip_steps_and_do_nothing_but_return_retrieved_state()
    {
        // GIVEN
        Retrieves(BuildStates(StateStatus.Skipped, StateStatus.Skipped), BuildState("value10", 20));

        // WHEN
        var result = await _sut.Start(_stateMachineId, "param1");

        // THEN
        result.Step1Single.Value1.Should().Be("value10");
        result.Step2Single.Value2.Should().Be(20);

        ShouldNotCallClient1Call1();
        ShouldNotCallClient1Call2();
        ShouldNotPersistSuccess();
        ShouldNotPersistState();
    }

    List<State> BuildStates(StateStatus step1Status = StateStatus.NotStarted,
        StateStatus step2Status = StateStatus.NotStarted) =>
        new()
        {
            new State { Name = nameof(TestState.Step1SingleState), Status = step1Status },
            new State { Name = nameof(TestState.Step2SingleState), Status = step2Status }
        };

    void Retrieves(List<State> states, TestState state) =>
        A.CallTo(() => _fixture.StateMachinePersistorMock.Retrieve(_stateMachineId, TestState.Empty))
            .Returns((states, state));

    void Client1Call1Returns(string value) => A.CallTo(() => _fixture.TestClient1Mock.Call1("param1")).Returns(value);
    void Client1Call2Returns(int value) => A.CallTo(() => _fixture.TestClient1Mock.Call2()).Returns(value);

    TestState BuildState(string value1, int value2) =>
        new()
        {
            Step1Single = new TestState.Step1SingleState { Value1 = value1 },
            Step2Single = new TestState.Step2SingleState { Value2 = value2 }
        };

    void ShouldPersistState(string value1, int value2) =>
        A.CallTo(
                () => _fixture.StateMachinePersistorMock.PersistState(
                    _stateMachineId,
                    A<TestState>.That.Matches(s => s.Step1Single.Value1 == value1 && s.Step2Single.Value2 == value2)
                )
            )
            .MustHaveHappenedOnceExactly();

    void ShouldNotPersistState() =>
        A.CallTo(
                () => _fixture.StateMachinePersistorMock.PersistState(
                    _stateMachineId,
                    A<TestState>._
                )
            )
            .MustNotHaveHappened();

    void ShouldNotPersistState(string value1, int value2) =>
        A.CallTo(
                () => _fixture.StateMachinePersistorMock.PersistState(
                    _stateMachineId,
                    A<TestState>.That.Matches(s => s.Step1Single.Value1 == value1 && s.Step2Single.Value2 == value2)
                )
            )
            .MustNotHaveHappened();

    void ShouldPersistSuccess(string stateName) =>
        A.CallTo(() => _fixture.StateMachinePersistorMock.PersistSuccess(_stateMachineId, stateName))
            .MustHaveHappenedOnceExactly();

    void ShouldNotPersistSuccess() =>
        A.CallTo(() => _fixture.StateMachinePersistorMock.PersistSuccess(_stateMachineId, A<string>._))
            .MustNotHaveHappened();

    void ShouldNotCallClient1Call1() =>
        A.CallTo(() => _fixture.TestClient1Mock.Call1("param1"))
            .MustNotHaveHappened();

    void ShouldNotCallClient1Call2() =>
        A.CallTo(() => _fixture.TestClient1Mock.Call2())
            .MustNotHaveHappened();

    public class Fixture : ServicesFixture,
        IAsyncLifetime
    {
        readonly IMessageSink _messageSink;
        public ITestClient1 TestClient1Mock { get; }
        public IStateMachinePersistor StateMachinePersistorMock { get; }

        public Fixture(IMessageSink messageSink)
        {
            _messageSink = messageSink;

            TestClient1Mock = A.Fake<ITestClient1>();
            StateMachinePersistorMock = A.Fake<IStateMachinePersistor>();
        }

        public Task InitializeAsync()
        {
            ConfigureServices(
                s =>
                {
                    RegisterXunitLogger(s, _messageSink);

                    s.AddTransient<TestStateMachine>();
                    s.AddTransient<IStateMachinePersistor>(_ => StateMachinePersistorMock);
                    s.AddTransient<ITestClient1>(_ => TestClient1Mock);
                }
            );

            return Task.CompletedTask;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}