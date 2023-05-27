namespace StateMachine.Tests;

public interface ITestClient1 {
    Task<string> Call1(string param);
    Task<int> Call2();
}