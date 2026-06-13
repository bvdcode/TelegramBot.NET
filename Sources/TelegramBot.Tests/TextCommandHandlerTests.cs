using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;
using TelegramBot.Handlers;

namespace TelegramBot.Tests;

public class TextCommandHandlerTests
{
    [Fact]
    public void ExactCommandRouteMatchesParameterlessMethod()
    {
        var handler = CreateHandler("/start", nameof(CommandController.Start));

        Assert.Equal(Method(nameof(CommandController.Start)), handler.GetMethodInfo());
        Assert.Null(handler.GetArguments());
    }

    [Fact]
    public void CommandRouteConvertsTypedArguments()
    {
        var handler = CreateHandler("/limit 42 true", nameof(CommandController.SetLimit));
        object[] args = handler.GetArguments()!;

        Assert.Equal(Method(nameof(CommandController.SetLimit)), handler.GetMethodInfo());
        Assert.Collection(
            args,
            value => Assert.Equal(42, Assert.IsType<int>(value)),
            value => Assert.True(Assert.IsType<bool>(value)));
    }

    [Fact]
    public void CommandRouteKeepsQuotedArgumentTogether()
    {
        var handler = CreateHandler("/say \"hello world\"", nameof(CommandController.Say));

        Assert.Equal(Method(nameof(CommandController.Say)), handler.GetMethodInfo());
        Assert.Equal(["hello world"], handler.GetArguments());
    }

    [Fact]
    public void NonCommandTextDoesNotMatchCommandRoute()
    {
        var handler = CreateHandler("start", nameof(CommandController.Start));

        Assert.Null(handler.GetMethodInfo());
        Assert.Null(handler.GetArguments());
    }

    [Fact]
    public void ArgumentConversionFailureDoesNotMatchCommandRoute()
    {
        var handler = CreateHandler("/limit not-a-number true", nameof(CommandController.SetLimit));

        Assert.Null(handler.GetMethodInfo());
    }

    [Fact]
    public void EqualCommandRoutesThrowAmbiguousMatchException()
    {
        var exception = Assert.Throws<AmbiguousMatchException>(() =>
            CreateHandler(
                "/duplicate",
                nameof(CommandController.DuplicateOne),
                nameof(CommandController.DuplicateTwo)));

        Assert.Contains("/duplicate", exception.Message);
    }

    private static TextCommandHandler CreateHandler(string text, params string[] methodNames)
    {
        return new TextCommandHandler(methodNames.Select(Method).ToArray(), CreateUpdate(text));
    }

    private static MethodInfo Method(string name)
    {
        return typeof(CommandController).GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    }

    private static Update CreateUpdate(string text)
    {
        return new Update
        {
            Message = new Message
            {
                Text = text,
            },
        };
    }

    private sealed class CommandController
    {
        [TextCommand("/start")]
        public void Start()
        {
        }

        [TextCommand("/limit")]
        public void SetLimit(int limit, bool enabled)
        {
        }

        [TextCommand("/say")]
        public void Say(string text)
        {
        }

        [TextCommand("/duplicate")]
        public void DuplicateOne()
        {
        }

        [TextCommand("/duplicate")]
        public void DuplicateTwo()
        {
        }
    }
}
