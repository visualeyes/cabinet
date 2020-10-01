using System;
using System.IO;
using System.IO.Abstractions;

namespace Cabinet.FileSystem {
    internal static class PathExtensions {
        // https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
        public static string MakeRelativeTo(this string fullPath, string basePath) {
            if (string.IsNullOrEmpty(fullPath)) throw new ArgumentNullException(nameof(fullPath));
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(nameof(basePath));

            var baseUri = new Uri(basePath.EnsureEndsWithDirectorySeparatorChar());
            var fullUri = new Uri(fullPath);

            if (baseUri.Scheme != fullUri.Scheme) {
                // Don't know how to handle
                throw new NotSupportedException($"Cannot make paths with different schemes relative: {baseUri.Scheme} (base), {fullUri.Scheme} (full)");
            }

            var relativeUri = baseUri.MakeRelativeUri(fullUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(fullUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase)) {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static string EnsureEndsWithDirectorySeparatorChar(this string path) {
            // Append a slash only if the path is a directory and does not have a slash.
            if (!Path.HasExtension(path) &&
                !path.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                return path + Path.DirectorySeparatorChar;
            }

            return path;
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
