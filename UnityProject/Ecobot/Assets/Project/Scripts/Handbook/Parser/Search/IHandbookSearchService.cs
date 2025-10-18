using System.Collections.Generic;

namespace Handbook.Parser.Search
{
    public interface IHandbookSearchService
    {
        IReadOnlyList<HandbookSearchResult> Query(IHandbookRepository repo, string text, bool includeHidden = false, int limit = 50);
    }
}