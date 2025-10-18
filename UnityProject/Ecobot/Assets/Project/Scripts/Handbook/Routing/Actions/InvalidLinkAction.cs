namespace Handbook.Routing
{
    public sealed class InvalidLinkAction : LinkActionBase
    {
        public string Reason { get; }

        public InvalidLinkAction(string raw, string reason)
        {
            Raw = raw;
            Reason = reason;
        }
    }
}