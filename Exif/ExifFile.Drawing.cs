using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    partial class ExifFile {
        Image _ExifImage;

        Image ExifImageOrDefault() {
            if (this._ExifImage != null)
                return this._ExifImage;

            this._ExifImage = this.TryLoadExifImage();
            if (this._ExifImage != null && this.ExifImageLoaded != null)
                this.ExifImageLoaded(this, EventArgs.Empty);

            return this._ExifImage;
        }

        Image TryLoadExifImage() {
            if (this.ThumbnailFile != null)
                return TryLoadImage(this.ThumbnailFile);
            if (IsJpeg(this.File))
                return TryLoadImage(this.File);
            return null;
        }

        static Image TryLoadImage(FileInfo file) {
            try {
                return Image.FromFile(file.FullName);
            } catch (Exception) {
                return null;
            }
        }

        void DisposeImages() {
            if (this._ExifImage != null) {
                this._ExifImage.Dispose();
                this._ExifImage = null;
            }
        }

        public DateTime? ExifOriginalDateTime {
            get { return this.ExifImageOrDefault()?.ExifOriginalDateTimeOrDefault(); }
        }

        public string EquipModel {
            get { return this.ExifImageOrDefault()?.ExifEquifModel(); }
        }

        public event EventHandler ExifImageLoaded;
    }
}
