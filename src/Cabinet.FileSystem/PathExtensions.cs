using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cabinet.FileSystem {
    public static class PathExtensions {
        public static string MakeRelativeTo(this string fullPath, string basePath) {
            if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                basePath += Path.DirectorySeparatorChar;
            }

            var fullPathUri = new Uri(fullPath, UriKind.Absolute);
            var basePathUri = new Uri(basePath, UriKind.Absolute);

            var relUri = basePathUri.MakeRelativeUri(fullPathUri);
            return relUri.ToString();
        }

        public static bool IsSameDirectory(this string path1, string path2) {
            if (String.IsNullOrWhiteSpace(path1)) throw new ArgumentNullException(nameof(path1));
            if (String.IsNullOrWhiteSpace(path2)) throw new ArgumentNullException(nameof(path2));

            var subDir = GetDirectoryInfoBase(path1);
            var baseDir = GetDirectoryInfoBase(path2);

            return subDir.IsSameDirectory(baseDir);
        }

        public static bool IsSameDirectory(this DirectoryInfoBase item1, DirectoryInfoBase item2) {
            if (item1 == null) throw new ArgumentNullException(nameof(item1));
            if (item2 == null) throw new ArgumentNullException(nameof(item2));


            string dir1Path = GetNormalisedFullPath(item1);
            string dir2Path = GetNormalisedFullPath(item2);

            return dir1Path.Equals(dir2Path, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSubDirectoryOf(this string subPath, string basePath) {
            var subDir = GetDirectoryInfoBase(subPath);
            var baseDir = GetDirectoryInfoBase(basePath);

            return subDir.IsChildOf(baseDir);
        }

        public static bool IsChildOf(this DirectoryInfoBase subDir, DirectoryInfoBase baseDir) {
            var isChild = false;

            while (subDir?.Parent != null) {
                if (subDir.Parent.IsSameDirectory(baseDir)) {
                    isChild = true;
                    break;
                } else {
                    subDir = subDir.Parent;
                }
            }

            return isChild;
        }

        // Handle cases where the directory ends with a /
        private static string GetNormalisedFullPath(DirectoryInfoBase dir) {
            return dir.Parent == null ? dir.FullName : Path.Combine(dir.Parent.FullName, dir.Name);
        }

        private static DirectoryInfoBase GetDirectoryInfoBase(string basePath) {
            return (DirectoryInfoBase)(new DirectoryInfo(basePath));
        }
    }
}
