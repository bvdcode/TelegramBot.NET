using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;
using TelegramBot.Handlers;

namespace TelegramBot.Tests;

public class TextMessageHandlerTests
{
    [Fact]
    public void ExactTextRouteBeatsFallback()
    {
        var handler = CreateHandler("vpn", nameof(RouteController.Vpn), nameof(RouteController.Fallback));

        Assert.Equal(Method(nameof(RouteController.Vpn)), handler.GetMethodInfo());
        Assert.Null(handler.GetArguments());
    }

    [Fact]
    public void FallbackRouteHandlesUnmatchedText()
    {
        var handler = CreateHandler("unknown", nameof(RouteController.Vpn), nameof(RouteController.Fallback));

        Assert.Equal(Method(nameof(RouteController.Fallback)), handler.GetMethodInfo());
        Assert.Equal(["unknown"], handler.GetArguments());
    }

    [Fact]
    public void RegexRouteMatchesEntireTextByDefault()
    {
        var exactHandler = CreateHandler(
            "123456",
            nameof(RouteController.SixDigitCode),
            nameof(RouteController.Fallback));
        var partialHandler = CreateHandler(
            "1234567",
            nameof(RouteController.SixDigitCode),
            nameof(RouteController.Fallback));

        Assert.Equal(Method(nameof(RouteController.SixDigitCode)), exactHandler.GetMethodInfo());
        Assert.Equal(Method(nameof(RouteController.Fallback)), partialHandler.GetMethodInfo());
    }

    [Fact]
    public void RegexRouteCanMatchSubstringWhenConfigured()
    {
        var handler = CreateHandler(
            "please open vpn",
            nameof(RouteController.ContainsVpn),
            nameof(RouteController.Fallback));

        Assert.Equal(Method(nameof(RouteController.ContainsVpn)), handler.GetMethodInfo());
    }

    [Fact]
    public void HigherPriorityCanOverrideDefaultRouteRank()
    {
        var handler = CreateHandler(
            "priority",
            nameof(RouteController.PriorityExact),
            nameof(RouteController.PriorityRegex));

        Assert.Equal(Method(nameof(RouteController.PriorityRegex)), handler.GetMethodInfo());
    }

    [Fact]
    public void MultipleAttributesOnSameMethodDoNotCreateAmbiguity()
    {
        var handler = CreateHandler("duplicate", nameof(RouteController.DuplicateExact));

        Assert.Equal(Method(nameof(RouteController.DuplicateExact)), handler.GetMethodInfo());
    }

    [Fact]
    public void EqualBestCandidatesThrowAmbiguousMatchException()
    {
        var exception = Assert.Throws<AmbiguousMatchException>(() =>
            CreateHandler(
                "same",
                nameof(RouteController.AmbiguousExactOne),
                nameof(RouteController.AmbiguousExactTwo)));

        Assert.Contains(nameof(RouteController.AmbiguousExactOne), exception.Message);
        Assert.Contains(nameof(RouteController.AmbiguousExactTwo), exception.Message);
    }

    [Fact]
    public void ObsoleteTextQueryMatchesExactTextOnly()
    {
        var exactHandler = CreateHandler(
            "legacy",
            nameof(RouteController.LegacyTextQuery),
            nameof(RouteController.Fallback));
        var partialHandler = CreateHandler(
            "legacy-value",
            nameof(RouteController.LegacyTextQuery),
            nameof(RouteController.Fallback));

        Assert.Equal(Method(nameof(RouteController.LegacyTextQuery)), exactHandler.GetMethodInfo());
        Assert.Equal(Method(nameof(RouteController.Fallback)), partialHandler.GetMethodInfo());
    }

    [Fact]
    public void ObsoleteTextQueryWithoutPatternActsAsFallback()
    {
        var handler = CreateHandler(
            "anything",
            nameof(RouteController.LegacyFallback),
            nameof(RouteController.Vpn));

        Assert.Equal(Method(nameof(RouteController.LegacyFallback)), handler.GetMethodInfo());
        Assert.Equal(["anything"], handler.GetArguments());
    }

    private static TextMessageHandler CreateHandler(string text, params string[] methodNames)
    {
        return new TextMessageHandler(methodNames.Select(Method).ToArray(), CreateUpdate(text));
    }

    private static MethodInfo Method(string name)
    {
        return typeof(RouteController).GetMethod(
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

    private sealed class RouteController
    {
        [Text("vpn")]
        public void Vpn()
        {
        }

        [TextFallback]
        public void Fallback(string text)
        {
        }

        [TextRegex(@"\d{6}")]
        public void SixDigitCode(string text)
        {
        }

        [TextRegex("vpn", MatchEntireText = false)]
        public void ContainsVpn()
        {
        }

        [Text("priority")]
        public void PriorityExact()
        {
        }

        [TextRegex("priority", Priority = 200)]
        public void PriorityRegex()
        {
        }

        [Text("duplicate")]
        [Text("duplicate")]
        public void DuplicateExact()
        {
        }

        [Text("same")]
        public void AmbiguousExactOne()
        {
        }

        [Text("same")]
        public void AmbiguousExactTwo()
        {
        }

#pragma warning disable CS0618
        [TextQuery("legacy")]
        public void LegacyTextQuery()
        {
        }

        [TextQuery]
        public void LegacyFallback(string text)
        {
        }
#pragma warning restore CS0618
    }
}
