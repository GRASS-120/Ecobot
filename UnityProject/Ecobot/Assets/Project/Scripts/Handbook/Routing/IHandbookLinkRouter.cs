using Handbook.Routing.Actions;

namespace Handbook.Routing
{
    public interface IHandbookLinkRouter
    {
        LinkActionBase Resolve(string url);

        bool TryResolveHandbook(string url, out OpenHandbookPageAction action);
        bool TryResolveTutorial(string url, out TriggerTutorialStepAction action);
        bool TryResolveExternal(string url, out OpenExternalUrlAction action);
    }
}