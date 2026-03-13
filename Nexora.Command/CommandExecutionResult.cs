namespace Nexora.Command;

public readonly struct CommandExecutionResult(Exception error)
{
    public Exception? Error { get; } = error;
    public static CommandExecutionResult Success { get; } = new();

    public bool IsSuccess => Error == null;

    public static implicit operator CommandExecutionResult(Exception error)
    {
        return new CommandExecutionResult(error);
    }
}