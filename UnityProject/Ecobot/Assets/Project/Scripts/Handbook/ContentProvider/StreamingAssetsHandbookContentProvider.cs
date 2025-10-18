using System.Threading;
using System.Threading.Tasks;
using Handbook.Models;
using UnityEngine;

namespace Handbook.ContentProvider
{
    public class StreamingAssetsHandbookContentProvider : IHandbookContentProvider
    {
        public string RootPath => _inner.RootPath;

        private readonly FileSystemHandbookContentProvider _inner;

        public StreamingAssetsHandbookContentProvider()
        {
            var root = Application.streamingAssetsPath;
            _inner = new FileSystemHandbookContentProvider(root);
        }

        public Task<HandbookManifest> LoadManifestAsync(CancellationToken ct = default)
        {
            return _inner.LoadManifestAsync(ct);
        }

        public Task<string> LoadPageMarkdownAsync(string pageId, string language, CancellationToken ct = default)
        {
            return _inner.LoadPageMarkdownAsync(pageId, language, ct);
        }

        public bool PageExists(string pageId, string language)
        {
            return _inner.PageExists(pageId, language);
        }

        public string BuildMediaPath(string relativePath)
        {
            return _inner.BuildMediaPath(relativePath);
        }
    }
}