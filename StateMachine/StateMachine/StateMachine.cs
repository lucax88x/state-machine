using Microsoft.Extensions.DependencyInjection;

namespace StateMachine;

public class StateMachine<TState>
{
    readonly Guid _id;
    readonly IServiceProvider _serviceProvider;
    readonly List<StateWithFunc<TState>> _descriptedStates = new();

    public StateMachine(Guid id, IServiceProvider serviceProvider)
    {
        _id = id;
        _serviceProvider = serviceProvider;
    }

    public void Add(string stateName, Func<TState, IServiceProvider, Task<TState>> func)
    {
        _descriptedStates.Add(new StateWithFunc<TState>
            { Name = stateName, Status = StateStatus.NotStarted, Func = func });
    }

    public async Task<TState> Run(TState defaultState)
    {
        using var scope = _serviceProvider.CreateScope();
        var persistor = scope.ServiceProvider.GetRequiredService<IStateMachinePersistor>();

        var (retrievedStates, retrievedState) = await persistor.Retrieve(_id, defaultState);

        var actualStates = UpdateStatesFromPersistence(_descriptedStates, retrievedStates);

        var currentState = retrievedState;
        foreach (var state in actualStates)
        {
            if (state.Status != StateStatus.NotStarted)
            {
                continue;
            }

            try
            {
                TState updatedState = await state.Func(currentState, scope.ServiceProvider);

                if (updatedState is null)
                {
                    throw new StateMachineException("State is null!");
                }

                await persistor.PersistSuccess(_id, state.Name);

                if (!updatedState.Equals(currentState))
                {
                    currentState = updatedState;

                    await persistor.PersistState(_id, updatedState);
                }
            }
            catch (Exception ex)
            {
                // _logger
                await persistor.PersistError(_id, state.Name, ex);
            }
        }

        return currentState;
    }

    List<StateWithFunc<TState>> UpdateStatesFromPersistence(
        IReadOnlyList<StateWithFunc<TState>> states,
        IReadOnlyList<State> retrievedStates
    )
    {
        return states.Select(
                state =>
                {
                    var foundRetrievedState =
                        retrievedStates.FirstOrDefault(s => s.Name.Equals(state.Name, StringComparison.Ordinal));

                    if (foundRetrievedState is null)
                    {
                        return state;
                    }

                    return state with
                    {
                        Status = foundRetrievedState.Status, LastError = foundRetrievedState.LastError
                    };
                }
            )
            .ToList();
    }
}