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
            if (this._ExifImage == null)
                return null;

            this.CacheProperties(this._ExifImage);

            if (this.ExifImageLoaded != null)
                this.ExifImageLoaded(this, EventArgs.Empty);

            return this._ExifImage;
        }

        Image TryLoadExifImage() {
            if (this.ThumbnailFile != null)
                return Image.FromFile(this.ThumbnailFile.FullName);
            if (IsJpeg(this.File))
                return Image.FromFile(this.File.FullName);
            return null;
        }

        void DisposeImages() {
            if (this._ExifImage != null) {
                this._ExifImage.Dispose();
                this._ExifImage = null;
            }
        }

        bool _IsCached;
        DateTime? _ExifOriginalDateTime;
        string _EquipModel;

        void CacheProperties(Image exifImage) {
            this._ExifOriginalDateTime = exifImage.ExifOriginalDateTimeOrDefault();
            this._EquipModel = exifImage.ExifEquipModel();
        }

        void EnsureCached() {
            if (this._IsCached)
                return;
            this._IsCached = true;

            // Load it to cache.
            this.ExifImageOrDefault();
        }

        public DateTime? ExifOriginalDateTime {
            get { this.EnsureCached(); return this._ExifOriginalDateTime; }
        }

        public string EquipModel {
            get { this.EnsureCached(); return this._EquipModel; }
        }

        public event EventHandler ExifImageLoaded;
    }
}
