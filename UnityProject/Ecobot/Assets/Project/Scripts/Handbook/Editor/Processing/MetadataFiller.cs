using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Handbook.Editor.Scanning;

namespace Handbook.Editor.Processing
{
    public class MetadataFiller
    {
        public void FillForTree(List<ScanModels.SectionNode> sections, string language)
        {
            if (sections == null) return;
            for (int i = 0; i < sections.Count; i++)
                FillSection(sections[i]);
        }

        private void FillSection(ScanModels.SectionNode s)
        {
            if (s.Pages != null)
            {
                for (int i = 0; i < s.Pages.Count; i++)
                    FillPage(s.Pages[i]);
            }

            if (s.Children != null)
            {
                for (int i = 0; i < s.Children.Count; i++)
                    FillSection(s.Children[i]);
            }
        }

        private void FillPage(ScanModels.PageNode p)
        {
            try
            {
                var content = File.ReadAllText(p.FilePath, Encoding.UTF8);
                p.Hash = ComputeSha1(content);
                p.UpdatedAt = File.GetLastWriteTimeUtc(p.FilePath).ToString("o");
            }
            catch
            {
                p.Hash = string.Empty;
                p.UpdatedAt = string.Empty;
            }
        }

        private string ComputeSha1(string text)
        {
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            var hash = sha1.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));
            return sb.ToString();
        }
    }
}