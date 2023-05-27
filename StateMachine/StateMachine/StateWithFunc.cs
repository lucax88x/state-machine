namespace StateMachine;

public record StateWithFunc<T> : State {
    public required Func<T, IServiceProvider, Task<T>> Func { get; init; }
}