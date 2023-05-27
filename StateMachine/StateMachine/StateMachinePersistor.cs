namespace StateMachine;

public interface IStateMachinePersistor {
    Task<(List<State> states, T state)> Retrieve<T>(Guid id, T defaultState);
    Task PersistSuccess(Guid id, string stateName);
    Task PersistError(Guid id, string stateName, Exception exception);
    Task PersistState<TState>(Guid id, TState updatedState);
}

public class StateMachinePersistor : IStateMachinePersistor {
    public Task<(List<State> states, T state)> Retrieve<T>(Guid id, T defaultState) {
        List<State> states = new();

        return Task.FromResult((states, defaultState));
    }

    public Task PersistSuccess(Guid id, string stateName) {
        return Task.CompletedTask;
    }

    public Task PersistError(Guid id, string stateName, Exception exception) {
        return Task.CompletedTask;
    }

    public Task PersistState<TState>(Guid id, TState updatedState) {
        return Task.CompletedTask;
    }
}