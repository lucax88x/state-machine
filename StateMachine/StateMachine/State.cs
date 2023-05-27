namespace StateMachine;

public record State {
    public required string Name { get; init; }
    public required StateStatus Status { get; init; }
    public string LastError { get; init; } = string.Empty;
}