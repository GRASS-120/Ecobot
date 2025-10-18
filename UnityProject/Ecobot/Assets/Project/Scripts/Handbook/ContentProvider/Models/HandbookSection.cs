using System;
using System.Collections.Generic;

namespace Handbook.Models
{
    [Serializable]
    public class HandbookSection
    {
        public string id;
        public string title;

        public List<HandbookPageRef> pages = new();
        public List<HandbookSection> children = new();
    }
}