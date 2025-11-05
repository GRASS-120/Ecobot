using System.Collections.Generic;

namespace Handbook.Editor.Scanning
{
    public class ScanModels
    {
        public sealed class SectionNode
        {
            public string RawName;
            public string Id;
            public string Title;
            public int OrderIndex;
            public List<PageNode> Pages = new();
            public List<SectionNode> Children = new();
        }

        public sealed class PageNode
        {
            public string FilePath;
            public string RelativePath;
            public string Id;
            public string Title;
            public int OrderIndex;
            public string FileName;
            public string Hash;
            public string UpdatedAt;
        }
    }
}