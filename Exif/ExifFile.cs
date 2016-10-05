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

        static readonly Regex dateTimePattern = new Regex(@"\d{8}_\d{6}", RegexOptions.Compiled);
        const string dateTimeFormat = "yyyyMMdd_HHmmss";

        static DateTime? ParseDateTime(string name) {
            name = Path.GetFileNameWithoutExtension(name);
            DateTime value;
            if (name.Length >= 15) {
                foreach (Match match in dateTimePattern.Matches(name)) {
                    if (DateTime.TryParseExact(match.Value,
                        dateTimeFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal,
                        out value)) {
                        return value;
                    }
                }
            }
            return null;
        }

        static string EmbedDateTimeToFileName(string name, DateTime time)
        {
            var ext = Path.GetExtension(name);
            name = Path.GetFileNameWithoutExtension(name);
            return string.Concat(name, "_", time.ToString(dateTimeFormat), ext);
        }

        public DateTime? DateTimeFromFileName
        {
            get { return ParseDateTime(this.File.Name); }
        }

        public DateTime DateTimeFromFileTime
        {
            get { return Min(this.File.CreationTime, this.File.LastWriteTime); }
        }

        public DateTime? OriginalDateTime {
            get { return this.ExifOriginalDateTime ?? this.DateTimeFromFileName; }
        }

        public DateTime OriginalDateTimeOrFileTime
        {
            get { return this.OriginalDateTime ?? this.DateTimeFromFileTime; }
        }

        public string FormatString(string format) {
            format = format.Replace("{date:", "{0:");
            return string.Format(format, this.OriginalDateTimeOrFileTime);
        }

        static DateTime Min(DateTime d1, DateTime d2) {
            return d1 < d2 ? d1 : d2;
        }
    }
}
