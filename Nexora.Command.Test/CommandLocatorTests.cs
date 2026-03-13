using Nexora.Command.Executor;
using Nexora.Command.Tree;

namespace Nexora.Command.Test;

public class CommandLocatorTests
{
    [Test]
    public void Locate_SimpleCommand_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell").Then(
            CommandTreeNode.Literal("@user")
                .Executes(args => CommandExecutionResult.Success)
        );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.callback, Is.Not.Null);
    }

    [Test]
    public void Locate_WithTextArgument_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell").Then(
            CommandTreeNode.Literal("@user")
                .Then(
                    CommandTreeNode.Text("message")
                        .Executes(args => CommandExecutionResult.Success)
                )
        );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user hello");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["message"], Is.EqualTo("hello"));
    }

    [Test]
    public void Locate_WithNumberArgument_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell").Then(
            CommandTreeNode.Literal("@user")
                .Then(
                    CommandTreeNode.Number("amount")
                        .Executes(args => CommandExecutionResult.Success)
                )
        );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user 123");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["amount"], Is.EqualTo("123"));
    }

    [Test]
    public void Locate_CommandNotFound_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell").Then(
            CommandTreeNode.Literal("@user")
                .Executes(args => CommandExecutionResult.Success)
        );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("hello world");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_IncompleteCommand_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell").Then(
            CommandTreeNode.Literal("@user")
                .Executes(args => CommandExecutionResult.Success)
        );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_WithGreedyText_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell").Then(
            CommandTreeNode.Literal("@user")
                .Then(
                    CommandTreeNode.GreedyText("message")
                        .Executes(args => CommandExecutionResult.Success)
                )
        );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user this is a long message with spaces");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["message"], Is.EqualTo("this is a long message with spaces"));
    }

    [Test]
    public void Locate_MultipleBranches_ShouldMatchCorrectBranch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            )
            .Then(
                CommandTreeNode.Literal("@channel")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @channel hello");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Locate_NestedStructure_ShouldMatchDeepPath()
    {
        // Arrange
        var root = new RootNode();
        var adminNode = CommandTreeNode.Literal("admin")
            .Then(
                CommandTreeNode.Literal("user")
                    .Then(
                        CommandTreeNode.Literal("ban")
                            .Then(
                                CommandTreeNode.Text("username")
                                    .Executes(args => CommandExecutionResult.Success)
                            )
                    )
            );

        root.Then(adminNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("admin user ban testuser");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["username"], Is.EqualTo("testuser"));
    }

    [Test]
    public void Locate_MultipleArguments_ShouldCaptureAll()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("x")
                    .Then(
                        CommandTreeNode.Number("y")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 10 20");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.args["x"], Is.EqualTo("10"));
            Assert.That(result.Value.args["y"], Is.EqualTo("20"));
        });
    }

    [Test]
    public void Locate_Backtrack_ShouldTryNextBranchOnFailure()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Number("amount")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            )
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user hello");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["message"], Is.EqualTo("hello"));
    }

    [Test]
    public void Locate_MultipleRootCommands_ShouldMatchCorrectOne()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );
        var banNode = CommandTreeNode.Literal("ban")
            .Then(
                CommandTreeNode.Text("username")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Then(banNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("ban baduser");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["username"], Is.EqualTo("baduser"));
    }

    [Test]
    public void Locate_WithPriority_ShouldMatchHigherPriority()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user", priority: 1)
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            )
            .Then(
                CommandTreeNode.Literal("@everyone", priority: 0)
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @everyone hello");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        // Should match @user (priority 1) first
        // But since "@everyone" doesn't match "@user", it should backtrack and match @everyone
    }

    [Test]
    public void Locate_WithDecimalNumber_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 3.14");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["value"], Is.EqualTo("3.14"));
    }

    [Test]
    public void Locate_WithQuotedString_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user \"hello world\"");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["message"], Is.EqualTo("hello world"));
    }

    [Test]
    public void Locate_ComplexNestedStructure_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var modNode = CommandTreeNode.Literal("mod")
            .Then(
                CommandTreeNode.Literal("user")
                    .Then(
                        CommandTreeNode.Text("username")
                            .Then(
                                CommandTreeNode.Literal("set")
                                    .Then(
                                        CommandTreeNode.Text("property")
                                            .Then(
                                                CommandTreeNode.Text("value")
                                                    .Executes(args => CommandExecutionResult.Success)
                                            )
                                    )
                            )
                    )
            );

        root.Then(modNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("mod user testuser set role admin");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.args["username"], Is.EqualTo("testuser"));
            Assert.That(result.Value.args["property"], Is.EqualTo("role"));
            Assert.That(result.Value.args["value"], Is.EqualTo("admin"));
        });
    }

    [Test]
    public void Locate_MixedArgumentTypes_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Number("times")
                            .Then(
                                CommandTreeNode.Text("message")
                                    .Executes(args => CommandExecutionResult.Success)
                            )
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user 3 hello");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.Value.args["times"], Is.EqualTo("3"));
            Assert.That(result.Value.args["message"], Is.EqualTo("hello"));
        });
    }

    [Test]
    public void Locate_EmptyInput_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_OnlySpaces_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("   ");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_CommandNotExist_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("hello world");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_IncompleteCommand_MissingSubcommand_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_UnexpectedArgumentType_TextWhenNumberExpected_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc abc");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_ExtraArgumentsAfterTerminal_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user extra");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_AllBranchesFailed_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Number("amount")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            )
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Number("count")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user notanumber");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_InvalidNumberFormat_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 12.34.56");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_UnclosedQuotedString_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user \"unclosed");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_InvalidEscapeSequence_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Then(
                        CommandTreeNode.Text("message")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user \"invalid\\escape\"");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_WrongLiteral_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @admin");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_PartialLiteralMatch_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @u");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_LiteralWithSuffix_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @users");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_MissingAllRequiredArguments_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("x")
                    .Then(
                        CommandTreeNode.Number("y")
                            .Then(
                                CommandTreeNode.Number("z")
                                    .Executes(args => CommandExecutionResult.Success)
                            )
                    )
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 1");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_MissingOneRequiredArgument_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("x")
                    .Then(
                        CommandTreeNode.Number("y")
                            .Executes(args => CommandExecutionResult.Success)
                    )
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 10");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_NumberStartsWithDot_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc .5");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_NumberEndsWithDot_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 123.");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_NegativeNumber_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc -123");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_DeepNestFailure_MiddleNode_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var adminNode = CommandTreeNode.Literal("admin")
            .Then(
                CommandTreeNode.Literal("user")
                    .Then(
                        CommandTreeNode.Literal("wrong")
                            .Then(
                                CommandTreeNode.Text("username")
                                    .Executes(args => CommandExecutionResult.Success)
                            )
                    )
            );

        root.Then(adminNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("admin user ban testuser");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_SpecialCharacters_ShouldReturnNull()
    {
        // Arrange
        var root = new RootNode();
        var tellNode = CommandTreeNode.Literal("tell")
            .Then(
                CommandTreeNode.Literal("@user")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(tellNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("tell @user@domain.com");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Locate_Zero_ShouldMatchAsNumber()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 0");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["value"], Is.EqualTo("0"));
    }

    [Test]
    public void Locate_VeryLargeNumber_ShouldMatch()
    {
        // Arrange
        var root = new RootNode();
        var calcNode = CommandTreeNode.Literal("calc")
            .Then(
                CommandTreeNode.Number("value")
                    .Executes(args => CommandExecutionResult.Success)
            );

        root.Then(calcNode);
        root.Freeze();

        var locator = new CommandLocator(root);
        var lexer = new CommandLexer("calc 999999999");

        // Act
        var result = locator.Locate(lexer);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value.args["value"], Is.EqualTo("999999999"));
    }
}