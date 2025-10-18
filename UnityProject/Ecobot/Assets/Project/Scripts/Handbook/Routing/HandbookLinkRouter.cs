using System;
using System.Text.RegularExpressions;
using Handbook.Routing.Actions;

namespace Handbook.Routing
{
    public class HandbookLinkRouter : IHandbookLinkRouter
    {
        public LinkActionBase Resolve(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return new InvalidLinkAction(url, "Empty url");

        if (TryResolveHandbook(url, out var handbook))
            return handbook;

        if (TryResolveTutorial(url, out var tutorial))
            return tutorial;

        if (TryResolveExternal(url, out var external))
            return external;

        return new InvalidLinkAction(url, "Unknown or invalid scheme");
    }

    public bool TryResolveHandbook(string url, out OpenHandbookPageAction action)
    {
        action = null;

        const string scheme = "handbook://";
        if (!url.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
            return false;

        var raw = url;
        var rest = url.Substring(scheme.Length).Trim();

        if (string.IsNullOrWhiteSpace(rest))
        {
            action = new OpenHandbookPageAction(raw, null, null);
            return false;
        }

        var hashIdx = rest.IndexOf('#');
        var id = hashIdx < 0 ? rest : rest.Substring(0, hashIdx);
        var anchor = hashIdx < 0 ? null : rest.Substring(hashIdx + 1);

        id = NormalizeToken(id);
        anchor = string.IsNullOrWhiteSpace(anchor) ? null : NormalizeToken(anchor);

        if (!IsValidId(id))
        {
            action = new OpenHandbookPageAction(raw, null, anchor);
            return false;
        }

        if (anchor != null && !IsValidId(anchor))
        {
            action = new OpenHandbookPageAction(raw, id, null);
            return false;
        }

        action = new OpenHandbookPageAction(raw, id, anchor);
        return true;
    }

    public bool TryResolveTutorial(string url, out TriggerTutorialStepAction action)
    {
        action = null;

        const string scheme = "tutorial://";
        if (!url.StartsWith(scheme, StringComparison.OrdinalIgnoreCase))
            return false;

        var raw = url;
        var stepId = url.Substring(scheme.Length).Trim();

        if (string.IsNullOrWhiteSpace(stepId))
            return false;

        action = new TriggerTutorialStepAction(raw, stepId);
        return true;
    }

    public bool TryResolveExternal(string url, out OpenExternalUrlAction action)
    {
        action = null;

        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            action = new OpenExternalUrlAction(url, url);
            return true;
        }

        return false;
    }

    private string NormalizeToken(string value)
    {
        return value?.Trim().ToLowerInvariant();
    }

    private bool IsValidId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Разрешаем только [a-z0-9-]+ — как договорились
        return Regex.IsMatch(value, "^[a-z0-9-]+$");
    }
    }
}