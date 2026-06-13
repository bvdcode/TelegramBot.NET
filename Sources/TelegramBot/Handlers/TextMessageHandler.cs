using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Telegram.Bot.Types;
using TelegramBot.Attributes;

namespace TelegramBot.Handlers
{
    internal class TextMessageHandler : ITelegramUpdateHandler
    {
        private const int ExactRouteRank = 3;
        private const int RegexRouteRank = 2;
        private const int FallbackRouteRank = 1;

        private readonly List<object> _args;
        private readonly MethodInfo? _methodInfo;

        public TextMessageHandler(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            _args = new List<object>();
            _methodInfo = GetMethodInfo(controllerMethods, update);
        }

        private MethodInfo? GetMethodInfo(IReadOnlyCollection<MethodInfo> controllerMethods, Update update)
        {
            if (update.Type != Telegram.Bot.Types.Enums.UpdateType.Message
                || update.Message == null
                || string.IsNullOrWhiteSpace(update.Message.Text))
            {
                return null;
            }

            string text = update.Message.Text;
            var candidates = new List<TextRouteCandidate>();
            foreach (var method in controllerMethods)
            {
                if (!CanHandleText(method))
                {
                    continue;
                }

                AddExactTextCandidates(candidates, method, text);
                AddRegexTextCandidates(candidates, method, text);
                AddFallbackTextCandidates(candidates, method);
                AddObsoleteTextQueryCandidates(candidates, method, text);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            List<TextRouteCandidate> bestCandidates = candidates
                .GroupBy(candidate => candidate.Method)
                .Select(group => group
                    .OrderByDescending(candidate => candidate.Priority)
                    .ThenByDescending(candidate => candidate.RouteRank)
                    .First())
                .OrderByDescending(candidate => candidate.Priority)
                .ThenByDescending(candidate => candidate.RouteRank)
                .ToList();

            TextRouteCandidate bestCandidate = bestCandidates[0];
            List<TextRouteCandidate> ambiguousCandidates = bestCandidates
                .Where(candidate => candidate.Priority == bestCandidate.Priority
                    && candidate.RouteRank == bestCandidate.RouteRank)
                .ToList();

            if (ambiguousCandidates.Count > 1)
            {
                throw new AmbiguousMatchException(
                    "Multiple methods found for text: " + text
                    + ". Candidates: " + string.Join(", ", ambiguousCandidates.Select(FormatCandidate)));
            }

            if (bestCandidate.Method.GetParameters().Length > 0)
            {
                _args.Add(text);
            }

            return bestCandidate.Method;
        }

        private static bool CanHandleText(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 0
                || parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
        }

        private static void AddExactTextCandidates(
            ICollection<TextRouteCandidate> candidates,
            MethodInfo method,
            string text)
        {
            foreach (TextAttribute attribute in method.GetCustomAttributes(typeof(TextAttribute), false))
            {
                if (attribute.IsMatch(text))
                {
                    candidates.Add(new TextRouteCandidate(
                        method,
                        attribute.Priority,
                        ExactRouteRank,
                        nameof(TextAttribute)));
                }
            }
        }

        private static void AddRegexTextCandidates(
            ICollection<TextRouteCandidate> candidates,
            MethodInfo method,
            string text)
        {
            foreach (TextRegexAttribute attribute in method.GetCustomAttributes(typeof(TextRegexAttribute), false))
            {
                if (attribute.IsMatch(text))
                {
                    candidates.Add(new TextRouteCandidate(
                        method,
                        attribute.Priority,
                        RegexRouteRank,
                        nameof(TextRegexAttribute)));
                }
            }
        }

        private static void AddFallbackTextCandidates(
            ICollection<TextRouteCandidate> candidates,
            MethodInfo method)
        {
            foreach (TextFallbackAttribute attribute in method.GetCustomAttributes(typeof(TextFallbackAttribute), false))
            {
                candidates.Add(new TextRouteCandidate(
                    method,
                    attribute.Priority,
                    FallbackRouteRank,
                    nameof(TextFallbackAttribute)));
            }
        }

        private static void AddObsoleteTextQueryCandidates(
            ICollection<TextRouteCandidate> candidates,
            MethodInfo method,
            string text)
        {
#pragma warning disable 0618
            foreach (TextQueryAttribute attribute in method.GetCustomAttributes(typeof(TextQueryAttribute), false))
#pragma warning restore 0618
            {
                if (!attribute.IsMatch(text))
                {
                    continue;
                }

                candidates.Add(new TextRouteCandidate(
                    method,
                    attribute.Priority,
                    attribute.IsFallback ? FallbackRouteRank : ExactRouteRank,
                    "TextQueryAttribute"));
            }
        }

        private static string FormatCandidate(TextRouteCandidate candidate)
        {
            return candidate.Method.DeclaringType?.Name + "." + candidate.Method.Name
                + " (" + candidate.RouteName + ", priority " + candidate.Priority + ")";
        }

        public object[]? GetArguments()
        {
            return _args.Count > 0 ? _args.ToArray() : null;
        }

        public MethodInfo? GetMethodInfo()
        {
            return _methodInfo;
        }

        private sealed class TextRouteCandidate
        {
            public TextRouteCandidate(
                MethodInfo method,
                int priority,
                int routeRank,
                string routeName)
            {
                Method = method;
                Priority = priority;
                RouteRank = routeRank;
                RouteName = routeName;
            }

            public MethodInfo Method { get; }

            public int Priority { get; }

            public int RouteRank { get; }

            public string RouteName { get; }
        }
    }
}
