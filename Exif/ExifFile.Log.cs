using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    partial class ExifFile {
        public static bool DryRun { get; set; }

        public class LogArgs {
            public LogArgs(ExifFile item, FileInfo file, string format, object[] args) {
                this.ExifFile = item;
                this.FileInfo = file;
                this.MessageFormatString = format;
                this.MessageArgs = args;
            }

            public ExifFile ExifFile { get; }
            public FileInfo FileInfo { get; }
            public string Message { get { return string.Format(this.MessageFormatString, this.MessageArgs); } }
            public string MessageFormatString { get; }
            public object[] MessageArgs { get; }
        }

        public static Action<LogArgs> Logger { get; set; }

        internal void Log(FileInfo file, string format, params object[] args) {
            if (Logger == null)
                return;
            Logger(new LogArgs(this, file, format, args));
        }
    }
}
