namespace Handbook.Routing
{
    public sealed class OpenExternalUrlAction : LinkActionBase
    {
        public string Url { get; }

        public OpenExternalUrlAction(string raw, string url)
        {
            Raw = raw;
            Url = url;
        }
    }
}