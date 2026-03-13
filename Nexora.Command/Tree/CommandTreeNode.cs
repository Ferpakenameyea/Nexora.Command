
namespace Nexora.Command.Tree;

public delegate CommandExecutionResult CommandCallback(Dictionary<string, string> args);

public abstract class CommandTreeNode
{
    public abstract CommandTreeNodeType Type { get; }
    public CommandCallback? Callback { get; private set; }
    public IList<CommandTreeNode> Children { get; private set; } = new List<CommandTreeNode>();
    public bool CanBeTerminal => Callback != null;
    public bool Frozen { get; private set; } = false;
    public int MatchPriority { get; init; } = 0;
    private static readonly IComparer<CommandTreeNode> _comparer = new Comparer();
    public CommandTreeNode Executes(CommandCallback callback)
    {
        Callback = callback;
        return this;
    }

    public CommandTreeNode Then(CommandTreeNode child)
    {
        if (Type == CommandTreeNodeType.GreedyText)
        {
            throw new InvalidOperationException("GreedyText node cannot have child command node");
        }
        Children.Add(child);
        return this;
    }

    public CommandTreeNode Freeze()
    {
        if (!Frozen)
        {
            Frozen = true;
            var children = (List<CommandTreeNode>)Children;
            children.Sort(comparer: _comparer);
            Children = children.AsReadOnly();

            foreach (var child in Children)
            {
                child.Freeze();
            }
        }
        return this; 
    }

    public static LiteralNode Literal(string expect, int priority = 0)
    {
        return new LiteralNode(expect)
        {
            MatchPriority = priority
        };
    }

    public static ArgumentNode Number(string argumentName, int priority = 0)
    {
        return new ArgumentNode(
            argumentName,
            ArgumentType.Number
        )
        {
            MatchPriority = priority
        };
    }

    public static ArgumentNode Text(string argumentName, int priority = 0)
    {
        return new ArgumentNode(
            argumentName,
            ArgumentType.Text
        )
        {
            MatchPriority = priority
        };
    }

    public static ArgumentNode GreedyText(string argumentName)
    {
        return new ArgumentNode(
            argumentName,
            ArgumentType.GreedyText
        )
        {
            MatchPriority = int.MinValue
        };
    }

    private sealed class Comparer : IComparer<CommandTreeNode>
    {
        public int Compare(CommandTreeNode? x, CommandTreeNode? y)
        {
            switch ((x, y))
            {
                case (null, not null):
                    return -1;
                case (not null, null):
                    return 1;
                case (null, null):
                    return 0;
                case (not null, not null):
                    if (x.MatchPriority > y.MatchPriority)
                    {
                        return -1;
                    }
                    if (x.MatchPriority == y.MatchPriority)
                    {
                        return 0;
                    }
                    return 1;
            }
        }
    }
}