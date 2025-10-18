using System;
using System.Collections.Generic;
using Handbook.Parser;
using Handbook.Parser.BlockTypes;

namespace Handbook.Models
{
    [Serializable]
    public class HandbookPage
    {
        public string id;
        public string title;
        public string rawMarkdown;

        public HandbookPageMeta meta = new();

        // Будут заполнены на этапе парсинга
        public List<HandbookAnchor> anchors = new();
        public List<HandbookLink> links = new();
        public List<HandbookBlockBase> blocks = new();
    }
}