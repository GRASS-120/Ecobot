using Handbook.Models;

namespace Handbook.Parser.Search
{
    public class HandbookSearchResult
    {
        public HandbookPageRef Page { get; }
        public float Score { get; }
        public string Snippet { get; }
        public string Anchor { get; }

        public HandbookSearchResult(HandbookPageRef page, float score, string snippet, string anchor = null)
        {
            Page = page;
            Score = score;
            Snippet = snippet;
            Anchor = anchor;
        }
    }
}