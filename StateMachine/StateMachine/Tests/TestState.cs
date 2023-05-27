namespace StateMachine.Tests;

public record TestState {
    public record Step1SingleState : ISingleState {
        public required string Value1 { get; init; } = string.Empty;

        public void ThrowIfEmpty() {
            StateMachineException.ThrowIfEmpty(Value1, GetType().Name);
        }
    }

    public record Step2SingleState : ISingleState {
        public required int Value2 { get; init; }

        public void ThrowIfEmpty() {
            StateMachineException.ThrowIfEmpty(Value2, GetType().Name);
        }
    };

    public required Step1SingleState Step1Single { get; set; }
    public required Step2SingleState Step2Single { get; set; }

    public static TestState Empty =>
        new() {
            Step1Single = new Step1SingleState { Value1 = string.Empty },
            Step2Single = new Step2SingleState { Value2 = 0 }
        };
}