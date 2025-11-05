using System;
using System.Collections.Generic;
using System.IO;
using Handbook.Editor.Processing;
using Unity.VisualScripting;

namespace Handbook.Editor.Scanning
{
    public class DirectoryScanner
    {
        public List<ScanModels.SectionNode> ScanSections(string scanRoot, bool allowRootPagesNode = true)
        {
            var result = new List<ScanModels.SectionNode>();
            if (!Directory.Exists(scanRoot))
                return result;

            // Корневые подпапки — секции
            var dirs = Directory.GetDirectories(scanRoot);
            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);

            // Файлы .md в корне — тоже страницы (в "безымянном" разделе)
            var rootPages = ScanPagesInDirectory(scanRoot, scanRoot);

// Добавляем "root" только на самом верхнем уровне
            if (allowRootPagesNode && rootPages.Count > 0)
            {
                var rootSection = new ScanModels.SectionNode
                {
                    RawName = "",
                    Id = "root",
                    Title = "Root",
                    OrderIndex = 0
                };
                rootSection.Pages.AddRange(rootPages);
                result.Add(rootSection);
            }

            for (int i = 0; i < dirs.Length; i++)
            {
                var dir = dirs[i];
                var name = Path.GetFileName(dir);
                var (order, clean) = OrderAndName(name);

                var sec = new ScanModels.SectionNode
                {
                    RawName = name,
                    OrderIndex = order,
                    Title = clean,
                    Id = SlugGenerator.ToSlug(clean)
                };

                // Страницы в этой папке
                sec.Pages.AddRange(ScanPagesInDirectory(dir, scanRoot));

                // Подпапки = children
                sec.Children.AddRange(ScanSections(dir, allowRootPagesNode: false));
                
                // Сортировки внутри секции
                sec.Pages.Sort((a, b) =>
                {
                    var o = a.OrderIndex.CompareTo(b.OrderIndex);
                    if (o != 0) return o;
                    return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
                });

                sec.Children.Sort((a, b) =>
                {
                    var o = a.OrderIndex.CompareTo(b.OrderIndex);
                    if (o != 0) return o;
                    return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
                });

                result.Add(sec);
            }

            // Сортировка сёстриц-секций
            result.Sort((a, b) =>
            {
                var o = a.OrderIndex.CompareTo(b.OrderIndex);
                if (o != 0) return o;
                return string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase);
            });

            return result;
        }

        private List<ScanModels.PageNode> ScanPagesInDirectory(string dir, string root)
        {
            var list = new List<ScanModels.PageNode>();
            if (!Directory.Exists(dir)) return list;

            var files = Directory.GetFiles(dir, "*.md", SearchOption.TopDirectoryOnly);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < files.Length; i++)
            {
                var path = files[i];
                var fileName = Path.GetFileNameWithoutExtension(path);
                var (order, clean) = OrderAndName(fileName);

                var title = TryReadFirstH1(path);
                if (string.IsNullOrWhiteSpace(title))
                    title = clean;

                var relDir = GetRelativePath(root, dir);
                var relPath = string.IsNullOrEmpty(relDir) ? fileName : $"{relDir.Replace('\\','/')}/{fileName}";

                list.Add(new ScanModels.PageNode
                {
                    FilePath = path.Replace('\\', '/'),
                    FileName = fileName,
                    RelativePath = relPath,
                    Id = SlugGenerator.ToSlug(clean),
                    Title = title,
                    OrderIndex = order
                });
            }

            return list;
        }
        
        private string GetRelativePath(string root, string dir)
        {
            var r = root.Replace('\\', '/').TrimEnd('/');
            var d = dir.Replace('\\', '/').TrimEnd('/');
            if (d.StartsWith(r, StringComparison.OrdinalIgnoreCase))
                return d.Substring(r.Length).TrimStart('/');
            return d;
        }

        private (int order, string clean) OrderAndName(string name)
        {
            // Префикс NN_... используется для порядка
            if (name.Length >= 3 && char.IsDigit(name[0]) && char.IsDigit(name[1]) && name[2] == '_')
            {
                var num = (name[0] - '0') * 10 + (name[1] - '0');
                var clean = name.Substring(3);
                return (num, clean);
            }
            return (int.MaxValue, name);
        }

        private string TryReadFirstH1(string filePath)
        {
            try
            {
                using var sr = new StreamReader(filePath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    // H1: "# Title"
                    if (line.StartsWith("# "))
                        return line.Substring(2).Trim();
                }
            }
            catch
            {
                // Игнорируем ошибки чтения в генераторе
            }
            return null;
        }
    }
}