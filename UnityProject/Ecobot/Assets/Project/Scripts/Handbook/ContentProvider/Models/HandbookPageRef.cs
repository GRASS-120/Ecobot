using System;

namespace Handbook.Models
{
    [Serializable]
    public class HandbookPageRef
    {
        public string id;
        public string title;

        public string[] tags;
        public string summary;
        public string hash;
        public string updatedAt;
        public bool hidden;
    }
}