namespace Nexora.Command.Test;

using Nexora.Command.Tree;
using static Nexora.Command.Tree.CommandTreeNode;

public class TreeBuildTests
{
    [Test]
    public void TestBuildSingleLiteralTree_WithFreezing_ShouldHaveConfiguredCorrectly()
    {
        CommandTreeNode commandTreeNode = Literal("action", priority: 25565)
            .Freeze();
        Assert.That(commandTreeNode, Is.TypeOf<LiteralNode>());
        Assert.Multiple(() =>
        {
            var node = (LiteralNode) commandTreeNode;
            Assert.That(node.CanBeTerminal, Is.False);
            Assert.That(node.Callback, Is.Null);
            Assert.That(node.Frozen, Is.True);
            Assert.That(node.Children, Is.InstanceOf<IReadOnlyCollection<CommandTreeNode>>());
            Assert.That(node.Children, Is.Empty);
            Assert.That(node.MatchPriority, Is.EqualTo(25565));
            Assert.That(node.Expect, Is.EqualTo("action"));
        });
    }

    [Test]
    public void TestFreezing_ShouldOrderChildrenByPriority()
    {
        CommandTreeNode root = Literal("action")
            .Then(Literal("13", priority: 13))
            .Then(Literal("99", priority: 99))
            .Then(Literal("100", priority: 100))
            .Then(Literal("-2", priority: -2))
            .Then(Literal("-10", priority: -10))
            .Freeze();

        Assert.That(root.Children.Select(c => c.MatchPriority), Is.Ordered.Descending);
    }

    [Test]
    public void GreedyTextNode_ShouldAlwaysBeTheLast()
    {
        CommandTreeNode root = Literal("action")
            .Then(GreedyText("greedy"))
            .Then(Literal("13", priority: 13))
            .Then(Literal("99", priority: 99))
            .Then(Literal("100", priority: 100))
            .Then(Literal("-2", priority: -2))
            .Then(Literal("-10", priority: -10))
            .Freeze();

        Assert.That(root.Children[^1].Type, Is.EqualTo(CommandTreeNodeType.GreedyText));
    }

    [Test]
    public void GreedyTextNode_TryAddChild_ShouldThrowException()
    {
        CommandTreeNode node = GreedyText("text");

        Assert.Throws<InvalidOperationException>(() =>
        {
            node.Then(Literal("literal"))
                .Freeze();
        });
    }
}
