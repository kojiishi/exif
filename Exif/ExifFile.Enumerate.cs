using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    partial class ExifFile {
        static bool IsJpeg(FileInfo file) { return IsExtension(file, ".jpg", ".jpe", ".jpeg"); }
        // DCF Thumbnail file.
        static bool IsThumbnail(FileInfo file) { return IsExtension(file, ".thm"); }
        static bool IsExtension(FileInfo file, string extension) {
            return string.Equals(file.Extension, extension, StringComparison.InvariantCultureIgnoreCase);
        }
        static bool IsExtension(FileInfo file, params string[] extensions) {
            var extension = file.Extension;
            return extensions.Any(e => string.Equals(extension, e, StringComparison.InvariantCultureIgnoreCase));
        }

        static readonly HashSet<string> _SupportedExtensions =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) {
            ".jpe",
            ".jpg",
            ".jpeg",
            ".png",
            ".mov",
            ".mp4",
        };

        public bool IsValid {
            get {
                if (this.File == null)
                    return false;
                return this.ThumbnailFile != null ||
                    _SupportedExtensions.Contains(this.File.Extension);
            }
        }

        public static IEnumerable<ExifFile> Enumerate(IEnumerable<string> sources, IEnumerable<ExifFile> existing = null) {
            var itemByName = new Dictionary<string, ExifFile>(StringComparer.InvariantCultureIgnoreCase);
            if (existing != null)
                foreach (var item in existing)
                    itemByName[item.Name] = item;

            foreach (var source in sources) {
                var dir = new DirectoryInfo(source);
                if (dir.Exists) {
                    foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                        AddFile(file, itemByName);
                } else {
                    AddFile(new FileInfo(source), itemByName);
                }
            }

            return itemByName.Values;
        }

        static void AddFile(FileInfo file, Dictionary<string, ExifFile> itemByName) {
            if ((file.Attributes & FileAttributes.Hidden) != 0)
                return;

            var name = Path.GetFileNameWithoutExtension(file.Name);
            ExifFile item;
            if (!itemByName.TryGetValue(name, out item)) {
                item = IsThumbnail(file) ? new ExifFile(null, file) : new ExifFile(file);
                itemByName.Add(name, item);
            } else if (IsThumbnail(file)) {
                Debug.Assert(item.ThumbnailFile == null);
                item.ThumbnailFile = file;
            } else {
                Debug.Assert(item.File == null);
                item.File = file;
            }
        }
    }
}
