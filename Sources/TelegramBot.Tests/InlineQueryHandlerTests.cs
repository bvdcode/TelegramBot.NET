using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;
using TelegramBot.Handlers;

namespace TelegramBot.Tests;

public class InlineQueryHandlerTests
{
    [Fact]
    public void ExactInlineRouteMatchesParameterlessMethod()
    {
        var handler = CreateHandler("menu", nameof(InlineController.Menu));

        Assert.Equal(Method(nameof(InlineController.Menu)), handler.GetMethodInfo());
        Assert.Null(handler.GetArguments());
    }

    [Fact]
    public void InlineRouteExtractsAndConvertsPlaceholderArgument()
    {
        var handler = CreateHandler("telegram/page/5", nameof(InlineController.Page));
        object[] args = handler.GetArguments()!;

        Assert.Equal(Method(nameof(InlineController.Page)), handler.GetMethodInfo());
        Assert.Collection(args, value => Assert.Equal(5, Assert.IsType<int>(value)));
    }

    [Fact]
    public void InlineRouteRejectsInvalidPlaceholderConversion()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            CreateHandler("telegram/page/not-a-number", nameof(InlineController.Page)));

        Assert.Contains("Method not found", exception.Message);
    }

    [Fact]
    public void EqualInlineRoutesThrowAmbiguousMatchException()
    {
        var exception = Assert.Throws<AmbiguousMatchException>(() =>
            CreateHandler(
                "duplicate",
                nameof(InlineController.DuplicateOne),
                nameof(InlineController.DuplicateTwo)));

        Assert.Contains("duplicate", exception.Message);
        Assert.Contains(nameof(InlineController.DuplicateOne), exception.Message);
        Assert.Contains(nameof(InlineController.DuplicateTwo), exception.Message);
    }

    private static InlineQueryHandler CreateHandler(string data, params string[] methodNames)
    {
        return new InlineQueryHandler(methodNames.Select(Method).ToArray(), CreateUpdate(data));
    }

    private static MethodInfo Method(string name)
    {
        return typeof(InlineController).GetMethod(
            name,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)!;
    }

    private static Update CreateUpdate(string data)
    {
        return new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Data = data,
            },
        };
    }

    private sealed class InlineController
    {
        [InlineCommand("menu")]
        public void Menu()
        {
        }

        [InlineCommand("telegram/page/{page}")]
        public void Page(int page)
        {
        }

        [InlineCommand("duplicate")]
        public void DuplicateOne()
        {
        }

        [InlineCommand("duplicate")]
        public void DuplicateTwo()
        {
        }
    }
}
