namespace Nexora.Command.Tree;

public sealed class ArgumentNode : CommandTreeNode
{
    public string ArgumentName { get; }
    public override CommandTreeNodeType Type { get; }

    public ArgumentNode(string argumentName, ArgumentType argumentType)
    {
        ArgumentName = argumentName;
        Type = argumentType switch
        {
            ArgumentType.Text => CommandTreeNodeType.Text,
            ArgumentType.Number => CommandTreeNodeType.Number,
            ArgumentType.GreedyText => CommandTreeNodeType.GreedyText,
            _ => throw new ArgumentException($"Argument of type {argumentType} is not supported.")
        };
    }
}