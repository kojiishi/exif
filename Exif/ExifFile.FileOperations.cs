using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    partial class ExifFile {
        public enum MoveOptions {
            Move,
            Copy,
        };

        public void MoveToFormatString(string destinationPath, MoveOptions options = MoveOptions.Move) {
            destinationPath = this.FormatString(destinationPath);
            this.MoveTo(destinationPath, options);
        }

        public void MoveTo(string destinationPath, MoveOptions options = MoveOptions.Move) {
            destinationPath = Path.Combine(this.File.Directory.FullName, destinationPath);
            var destinationDirectory = new DirectoryInfo(destinationPath);
            if (!DryRun && !destinationDirectory.Exists)
                destinationDirectory.Create();

            var destinationName = this.File.Name;
            if (this.OriginalDateTime == null)
                destinationName = ExifFile.EmbedDateTimeToFileName(destinationName, this.DateTimeFromFileTime);

            // Image must be disposed to unlock the source file.
            this.DisposeImages();

            var operation = MoveCopyOperation.GetOperation(options);
            var destination = new FileInfo(Path.Combine(destinationDirectory.FullName, destinationName));
            this.File = this.MoveFileTo(this.File, destination, operation);

            if (this.ThumbnailFile != null) {
                var thumbnailDestination = destination.ReplaceExtension(this.ThumbnailFile.Extension);
                Debug.Assert(!thumbnailDestination.Exists);
                this.ThumbnailFile = this.MoveFileTo(this.ThumbnailFile, thumbnailDestination, operation);
            }
        }

        FileInfo MoveFileTo(FileInfo source, FileInfo destination, MoveCopyOperation operation) {
            var withRename = false;
            if (destination.Exists) {
                if (source.FullName == destination.FullName) {
                    this.Log(source, "Skipped (the destination is the same location)");
                    return destination;
                }

                this.Log(source, "Comparing with {0}", destination);
                if (source.ContentEquals(destination))
                    return operation.SameFileExists(this, source, destination);

                destination = destination.UniqueFileName();
                withRename = true;
            }

            Debug.Assert(!destination.Exists);
            return operation.Execute(this, source, destination, withRename);
        }

        abstract class MoveCopyOperation {
            public static MoveCopyOperation GetOperation(MoveOptions options) {
                if (ExifFile.DryRun)
                    return DryRun;
                return options == MoveOptions.Move ? Move : Copy;
            }

            static readonly MoveCopyOperation DryRun = new DryRunOperation();
            static readonly MoveCopyOperation Move = new MoveOperation();
            static readonly MoveCopyOperation Copy = new CopyOperation();

            public abstract FileInfo SameFileExists(ExifFile file, FileInfo source, FileInfo destination);
            public abstract FileInfo Execute(ExifFile file, FileInfo source, FileInfo destination, bool withRename);

            class DryRunOperation : MoveCopyOperation {
                public override FileInfo SameFileExists(ExifFile file, FileInfo source, FileInfo destination) {
                    file.Log(source, "Dry-run (the same file exists in {0})", destination.Directory);
                    return source;
                }
                public override FileInfo Execute(ExifFile file, FileInfo source, FileInfo destination, bool withRename) {
                    file.Log(source, withRename ? "Dry-run with rename to {0}" : "Dry-run to {0}", destination);
                    return source;
                }
            }

            class MoveOperation : MoveCopyOperation {
                public override FileInfo SameFileExists(ExifFile file, FileInfo source, FileInfo destination) {
                    file.Log(source, "Deleting (the same file exists in {0})", destination.Directory);
                    source.Delete();
                    file.Log(source, "Deleted (the same file exists in {0})", destination.Directory);
                    return destination;
                }
                public override FileInfo Execute(ExifFile file, FileInfo source, FileInfo destination, bool withRename) {
                    file.Log(source, withRename ? "Moving with rename to {0}" : "Moving to {0}", destination);
                    source.MoveTo(destination.FullName);
                    file.Log(source, withRename ? "Moved with rename to {0}" : "Moved to {0}", destination);
                    return destination;
                }
            }

            class CopyOperation : MoveCopyOperation {
                public override FileInfo SameFileExists(ExifFile file, FileInfo source, FileInfo destination) {
                    file.Log(source, "Skipped (the same file exists in {0})", destination.Directory);
                    return destination;
                }
                public override FileInfo Execute(ExifFile file, FileInfo source, FileInfo destination, bool withRename) {
                    file.Log(source, withRename ? "Copying with rename to {0}" : "Copying to {0}", destination);
                    source.CopyTo(destination.FullName);
                    file.Log(source, withRename ? "Copied with rename to {0}" : "Copied to {0}", destination);
                    return destination;
                }
            }
        }
    }
}
