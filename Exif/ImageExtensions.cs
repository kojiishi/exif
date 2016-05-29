using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Exif {
    public static partial class ImageExtensions {
        #region ImageFormat and Encoder Utilities

        public static bool IsRawFormat(this Image image, ImageFormat format) {
            return image.RawFormat.Guid == format.Guid;
        }

        public static bool IsJpeg(this Image image) {
            return image.IsRawFormat(ImageFormat.Jpeg);
        }

        public static ImageCodecInfo RawFormatEncoder(this Image image) {
            return EncoderFromFormatID(image.RawFormat.Guid);
        }

        /// <summary>
        /// Find encoder's ImageCodecInfo from MIME type.
        /// </summary>
        public static ImageCodecInfo EncoderFromMimeType(string mimeType) {
            return ImageCodecInfo.GetImageEncoders()
                .First(e => e.MimeType == mimeType);
        }

        /// <summary>
        /// Find encoder's ImageCodecInfo from format GUID.
        /// </summary>
        public static ImageCodecInfo EncoderFromFormatID(Guid formatID) {
            return ImageCodecInfo.GetImageEncoders()
                .First(e => e.FormatID == formatID);
        }

        #endregion
    }
}
