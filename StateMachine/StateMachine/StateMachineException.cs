using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace StateMachine;

[Serializable]
public class StateMachineException : Exception {
    public StateMachineException(string propertyName, string stateName) :
        this($"Property {propertyName} not set, set or rerun {stateName}") { }

    public StateMachineException(string message) : base(message) { }

    public StateMachineException() { }

    public StateMachineException(string message, Exception innerException) : base(message, innerException) { }

    protected StateMachineException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(
        serializationInfo,
        streamingContext
    ) { }

    public static void ThrowIfEmpty(int value, string stateName, [CallerArgumentExpression("value")] string? paramName = null) {
        if (value == default) {
            throw new StateMachineException(paramName ?? nameof(value), stateName);
        }
    }
    
    public static void ThrowIfEmpty(string value, string stateName, [CallerArgumentExpression("value")] string? paramName = null) {
        if (value == default) {
            throw new StateMachineException(paramName ?? nameof(value), stateName);
        }
    }
    
    public static void ThrowIfEmpty<T>(IEnumerable<T> value, string stateName, [CallerArgumentExpression("value")] string? paramName = null) {
        if (value == default) {
            throw new StateMachineException(paramName ?? nameof(value), stateName);
        }
    }
}