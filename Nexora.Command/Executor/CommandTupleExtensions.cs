namespace Nexora.Command.Executor;
using Nexora.Command.Tree;

using Execution=(Tree.CommandCallback callback, Dictionary<string, string> args);
public static class CommandTupleExtensions
{
    public static CommandExecutionResult Invoke(this Execution execution)
    {
        return execution.callback(execution.args);
    }

    public static CommandExecutionResult? Invoke(this Execution? execution)
    {
        return execution?.callback(execution.Value.args);
    }
}