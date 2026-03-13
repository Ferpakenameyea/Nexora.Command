using Nexora.Command.Tree;

namespace Nexora.Command.Executor;

internal class CommandLocator
{
    private readonly CommandTreeNode _rootNode;

    public CommandLocator(CommandTreeNode rootNode)
    {
        if (!rootNode.Frozen)
        {
            throw new ArgumentException(
                "Freeze your command tree before passing it to command locator!");
        }

        _rootNode = rootNode;
    }

    public (CommandCallback callback, Dictionary<string, string> args)? Locate(CommandLexer lexer)
    {
        Dictionary<string, string> arguments = [];
        
        // root 是空节点，从 root 的子节点开始尝试匹配
        foreach (var child in _rootNode.Children)
        {
            var result = LocateRecursive(child, lexer, arguments);
            if (result != null)
            {
                return result;
            }
        }
        
        return null;
    }

    private static (CommandCallback, Dictionary<string, string>)? LocateRecursive(
        CommandTreeNode node,
        CommandLexer lexer,
        Dictionary<string, string> arguments)
    {
        // 尝试匹配当前节点
        string? capturedValue = node.Type switch
        {
            CommandTreeNodeType.Literal => lexer.TryNextLiteral(((LiteralNode)node).Expect),
            CommandTreeNodeType.Number => lexer.TryNextNumber(),
            CommandTreeNodeType.Text => lexer.TryNextString(),
            CommandTreeNodeType.GreedyText => lexer.TryNextGreedyString(),
            _ => null
        };

        if (capturedValue == null)
        {
            // 匹配失败，返回 null
            return null;
        }

        // 如果是参数节点，将捕获的值存入 arguments
        string? argumentName = null;
        if (node is ArgumentNode argNode)
        {
            argumentName = argNode.ArgumentName;
            arguments[argumentName] = capturedValue;
        }

        // 检查当前节点是否可以作为终端节点，并且 lexer 已经没有更多内容可供解析
        if (node.CanBeTerminal && lexer.AtEnd)
        {
            return (node.Callback!, new Dictionary<string, string>(arguments));
        }

        // 递归尝试所有子节点
        foreach (var child in node.Children)
        {
            var result = LocateRecursive(child, lexer, arguments);
            if (result != null)
            {
                return result;
            }
        }

        // 所有子节点都匹配失败，回溯
        if (argumentName != null)
        {
            arguments.Remove(argumentName);
        }

        // 回退 lexer 状态
        if (node.Type == CommandTreeNodeType.GreedyText)
        {
            lexer.ReturnGreedyString(capturedValue);
        }
        else if (node.Type == CommandTreeNodeType.Text)
        {
            lexer.ReturnString(capturedValue);
        }
        else if (node.Type == CommandTreeNodeType.Number)
        {
            lexer.ReturnContinuousToken(capturedValue);
        }
        else if (node.Type == CommandTreeNodeType.Literal)
        {
            lexer.ReturnContinuousToken(((LiteralNode)node).Expect);
        }

        return null;
    }
}