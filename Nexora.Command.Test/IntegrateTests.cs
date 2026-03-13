using Nexora.Command.Executor;
using Nexora.Command.Tree;
using static Nexora.Command.Tree.CommandTreeNode;

namespace Nexora.Command.Test;

public class IntegrateTests
{

    [Test]
    public void IncrementThroughCommand_ShouldPerformCorrectly()
    {
        int a = 1;
        int b = 1;
        var root = new RootNode()
            .Then(Literal("increment")
                .Then(Text("variableName")
                    .Then(Number("time")
                        .Executes((args) =>
                        {
                            string variableName = args["variableName"];
                            if (!int.TryParse(args["time"], out var time))
                            {
                                return new ArgumentException("<time> argument parse failed. It should be an integer");
                            }

                            if (variableName == "a")
                            {
                                a += time;
                            }
                            else
                            {
                                b += time;
                            }

                            return CommandExecutionResult.Success;
                        }))));

        var locator = new CommandLocator(root.Freeze());
        var lexer = new CommandLexer("increment a 2");

        var execution = locator.Locate(lexer);
        Assert.That(execution, Is.Not.Null);

        execution.Invoke();

        Assert.Multiple(() =>
        {
            Assert.That(a, Is.EqualTo(3));
            Assert.That(b, Is.EqualTo(1));
        });
    }
}