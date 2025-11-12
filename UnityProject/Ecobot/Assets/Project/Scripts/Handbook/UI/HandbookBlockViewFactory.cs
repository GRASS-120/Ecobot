using System;
using System.Collections.Generic;
using Handbook.Parser;
using Handbook.UI.BlockView;
using Handbook.UI.BlockView.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Handbook.UI
{
    public class HandbookBlockViewFactory : IHandbookBlockViewFactory
    {
        private readonly Dictionary<Type, HandbookBlockView> _map = new();
        private readonly Func<HandbookBlockRenderContext> _ctxProvider;

        public HandbookBlockViewFactory(Func<HandbookBlockRenderContext> ctxProvider)
        {
            _ctxProvider = ctxProvider;
        }

        public void Register<TBlock>(HandbookBlockView prefab) where TBlock : HandbookBlockBase
        {
            _map[typeof(TBlock)] = prefab;
        }

        public HandbookBlockView Create(HandbookBlockBase model, Transform parent)
        {
            if (model == null) return null;

            if (!_map.TryGetValue(model.GetType(), out var prefab) || prefab == null)
                return null;

            var instance = Object.Instantiate(prefab, parent);
            instance.Setup(model, _ctxProvider());
            return instance;
        }
    }
}