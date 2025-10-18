using System;

namespace Handbook.Models
{
    [Serializable]
    public class HandbookLink
    {
        public string url;

        // Роутер заполнит производные поля (если применимо)
        public string kind;    // "handbook", "tutorial", "external"
        public string pageId;
        public string anchor;
        public string stepId;
    }
}