using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Handbook.Models;
using Newtonsoft.Json;

namespace Handbook.ContentProvider
{
    public class FileSystemHandbookContentProvider : IHandbookContentProvider
    {
        public string RootPath => _rootPath;

        private readonly string _rootPath;

        public FileSystemHandbookContentProvider(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("Root path must not be empty.", nameof(rootPath));

            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException($"Handbook root not found: {rootPath}");

            _rootPath = rootPath;
        }

        public async Task<HandbookManifest> LoadManifestAsync(CancellationToken ct = default)
        {
            var path = GetManifestPath();
            if (!File.Exists(path))
                throw new FileNotFoundException($"Handbook manifest.json not found at path: {path}");

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            var json = await reader.ReadToEndAsync();
            ct.ThrowIfCancellationRequested();

            var manifest = JsonConvert.DeserializeObject<HandbookManifest>(json);
            if (manifest == null)
                throw new InvalidOperationException("Failed to parse Handbook manifest.json");

            if (manifest.sections == null)
                manifest.sections = new();

            if (manifest.redirects == null)
                manifest.redirects = new();

            return manifest;
        }

        public async Task<string> LoadPageMarkdownAsync(string pageId, string language, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(pageId))
                throw new ArgumentException("Page id must not be empty.", nameof(pageId));

            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentException("Language must not be empty.", nameof(language));

            var path = GetPagePath(language, pageId);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Handbook page not found: {path}");

            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);
            var text = await reader.ReadToEndAsync();
            ct.ThrowIfCancellationRequested();

            return text;
        }

        public bool PageExists(string pageId, string language)
        {
            if (string.IsNullOrWhiteSpace(pageId))
                return false;

            if (string.IsNullOrWhiteSpace(language))
                return false;

            var path = GetPagePath(language, pageId);
            return File.Exists(path);
        }

        public string BuildMediaPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return _rootPath;

            return Path.Combine(_rootPath, relativePath).Replace('\\', '/');
        }

        private string GetManifestPath()
        {
            return Path.Combine(_rootPath, "manifest.json");
        }

        private string GetPagePath(string language, string pageId)
        {
            var fileName = $"{pageId}.md";
            return Path.Combine(_rootPath, "pages", language, fileName);
        }
    }
}