# Nexora.Command

## Description

Using Minecraft-like command parsing and execution in .NET C#

## Supported Platforms

- .NET 9.0

## Usage

```bash
dotnet add package Nexora.Command
```

Command Registration:

```csharp
using System;
using Nexora.Command.Executor;
using Nexora.Command.Tree;
using static Nexora.Command.Tree.CommandTreeNode;

// register 'say <content>' which outputs <content> to Console through Console.WriteLine()
var root = new RootNode()
    .Then(
        Literal("say")
            .Then(
                Text("content").Executes((args) => 
                {
                    // get argument value as string in context
                    var content = args["content"];
                    Console.WriteLine(content);
                    return CommandExecutionResult.Success;
                })));
// freeze your node before execution. This will sort the children nodes and make them unmodifiable
root.Freeze();

var locator = new CommandLocator(root.Freeze());
var lexer = new CommandLexer("say hello!");
var execution = locator.Locate(lexer);

execution.Invoke();
// outputs: hello!
```