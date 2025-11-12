using System;
using System.Threading;
using System.Threading.Tasks;
using Handbook.Parser;
using Handbook.Parser.BlockTypes;
using Handbook.UI.BlockView.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Handbook.UI.BlockView.Types
{
    public class ImageBlockView : HandbookBlockView
    {
        [SerializeField] private Image _image;

        private CancellationTokenSource _cts;

        public override void Setup(HandbookBlockBase model, HandbookBlockRenderContext ctx)
        {
            var img = model as ImageBlock;
            if (img == null || _image == null) return;

            var path = ctx.BuildMediaPath?.Invoke(img.src) ?? img.src;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            LoadSpriteAsync(path, ctx.SpriteCache, _cts.Token);
        }

        public override void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async void LoadSpriteAsync(string path, ISpriteCache cache, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            if (cache != null && cache.TryGet(path, out var cached))
            {
                _image.sprite = cached;
                _image.SetNativeSize();
                return;
            }

            var url = ToFileUrl(path);
            var request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
            var op = request.SendWebRequest();

            while (!op.isDone)
            {
                if (ct.IsCancellationRequested)
                {
                    request.Abort();
                    return;
                }
                await Task.Yield();
            }

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                return;

            var tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(request);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);

            _image.sprite = sprite;
            _image.SetNativeSize();

            cache?.Set(path, sprite);
        }

        private string ToFileUrl(string path)
        {
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                return path;

            return "file://" + path.Replace('\\', '/');
        }
    }
}