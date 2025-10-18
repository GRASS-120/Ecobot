using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Handbook.ContentProvider;
using Handbook.Models;

namespace Handbook.Parser
{
    public interface IHandbookRepository
    {
        HandbookManifest Manifest { get; }
        string Language { get; }

        Task InitializeAsync(IHandbookContentProvider provider, IHandbookMarkdownParser parser, CancellationToken ct = default);
        Task RefreshManifestAsync(CancellationToken ct = default);

        bool TryGetPageRef(string pageId, out HandbookPageRef pageRef);
        bool TryResolveRedirect(string idOrAlias, out string resolvedPageId);
        bool PageExists(string pageId);

        Task<HandbookPage> LoadPageAsync(string pageId, CancellationToken ct = default);

        IReadOnlyList<HandbookSection> EnumerateSections();
        IReadOnlyList<HandbookPageRef> EnumerateAllPages(bool includeHidden = false);

        string BuildMediaPath(string relativePath);

        void Dispose();
    }
}