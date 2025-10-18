namespace Handbook.Routing.Actions
{
    public sealed class OpenHandbookPageAction : LinkActionBase
    {
        public string PageId { get; }
        public string Anchor { get; }

        public OpenHandbookPageAction(string raw, string pageId, string anchor)
        {
            Raw = raw;
            PageId = pageId;
            Anchor = string.IsNullOrWhiteSpace(anchor) ? null : anchor;
        }
    }
}