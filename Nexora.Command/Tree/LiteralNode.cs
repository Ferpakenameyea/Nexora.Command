namespace Nexora.Command.Tree;

public sealed class LiteralNode(string expect) : CommandTreeNode
{
    public override CommandTreeNodeType Type => CommandTreeNodeType.Literal;

    public string Expect { get; } = expect;
}