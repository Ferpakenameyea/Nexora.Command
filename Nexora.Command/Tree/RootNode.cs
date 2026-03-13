namespace Nexora.Command.Tree;

public sealed class RootNode : CommandTreeNode
{
    public override CommandTreeNodeType Type => CommandTreeNodeType.Root;
}