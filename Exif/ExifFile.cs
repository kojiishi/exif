using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Exif {
    [DebuggerDisplay("{File} {ThumbnailFile}")]
    public partial class ExifFile : IDisposable {
        internal ExifFile(FileInfo file, FileInfo thumbnailFile = null) {
            Debug.Assert(file != null || thumbnailFile != null);
            this.File = file;
            this.ThumbnailFile = thumbnailFile;
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            this.DisposeImages();
        }

        public FileInfo File { get; private set; }
        public string Name { get { return Path.GetFileNameWithoutExtension(this.File.Name); } }
        public FileInfo ThumbnailFile { get; private set; }

        public DateTime? DateTimeFromFileName {
            get { return ParseDateTime(this.File.Name); }
        }

        static readonly Regex dateTimePattern = new Regex(@"\d{8}_\d{6}", RegexOptions.Compiled);

        static DateTime? ParseDateTime(string name) {
            name = Path.GetFileNameWithoutExtension(name);
            DateTime value;
            if (name.Length >= 15) {
                foreach (Match match in dateTimePattern.Matches(name)) {
                    if (DateTime.TryParseExact(match.Value,
                        "yyyyMMdd_HHmmss",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out value)) {
                        return value;
                    }
                }
            }
            return null;
        }

        public DateTime? OriginalDateTime {
            get {
                return this.ExifOriginalDateTime ??
                    this.DateTimeFromFileName ??
                    Min(this.File.CreationTime, this.File.LastWriteTime);
            }
        }

        public string FormatString(string format) {
            format = format.Replace("{date:", "{0:");
            return string.Format(format, this.OriginalDateTime);
        }

        static DateTime Min(DateTime d1, DateTime d2) {
            return d1 < d2 ? d1 : d2;
        }
    }
}
