using System;

namespace Handbook.Models
{
    [Serializable]
    public class HandbookPageMeta
    {
        public string[] tags;
        public string updatedAt;
        public bool hidden;
    }
}