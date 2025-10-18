using System.Threading;
using System.Threading.Tasks;
using Handbook.Models;

namespace Handbook.ContentProvider
{
    public interface IHandbookContentProvider
    {
        string RootPath { get; }

        Task<HandbookManifest> LoadManifestAsync(CancellationToken ct = default);
        Task<string> LoadPageMarkdownAsync(string pageId, string language, CancellationToken ct = default);

        bool PageExists(string pageId, string language);
        string BuildMediaPath(string relativePath);
    }
}