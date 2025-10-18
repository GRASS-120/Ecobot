using System;
using System.Collections.Generic;

namespace Handbook.Models
{
    [Serializable]
    public class HandbookManifest
    {
        public string version;
        public string language;
        public string mediaBasePath;
        public string defaultPageId;

        public List<HandbookSection> sections = new();
        public List<HandbookRedirect> redirects = new();
    }
}