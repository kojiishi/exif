using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    public static class FileExtensions {
        #region ContentEquals

        public static bool ContentEquals(this FileInfo f1, FileInfo f2) {
            if (f1.Length != f2.Length)
                return false;

            using (var s1 = f1.OpenRead())
            using (var s2 = f2.OpenRead())
                return ContentEquals(s1, s2);
        }

        static bool ContentEquals(this Stream s1, Stream s2) {
            const int bufferSize = 1024 * 1024;
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];
            for (;;) {
                var count = Task.WhenAll(
                    s1.ReadAsync(buffer1, 0, bufferSize),
                    s2.ReadAsync(buffer2, 0, bufferSize)).Result;
                if (count[0] != count[1]) {
                    Debug.Fail("Read size should be the same");
                    return false;
                }
                if (count[0] <= 0)
                    return true;
                if (!ContentEquals(buffer1, buffer2, count[0]))
                    return false;
            }
        }

        internal static bool ContentEquals(byte[] b1, byte[] b2) {
            return b1.Length == b2.Length && ContentEquals(b1, b2, b1.Length);
        }

        internal static bool ContentEquals(byte[] b1, byte[] b2, int count) {
            if (count < 0 || count > b1.Length || count > b2.Length)
                throw new ArgumentOutOfRangeException("count");
            return memcmp(b1, b2, count) == 0;
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        #endregion

        #region Path Operations

        public static FileInfo UniqueFileName(this FileInfo file) {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
            var extension = file.Extension;
            for (int i = 1; ; i++) {
                var newName = string.Format("{1} ({0}){2}", i, nameWithoutExtension, extension);
                var newFile = new FileInfo(Path.Combine(file.DirectoryName, newName));
                if (!newFile.Exists)
                    return newFile;
            }
        }

        public static FileInfo ReplaceFileName(this FileInfo file, string name) {
            return new FileInfo(Path.Combine(file.DirectoryName, name));
        }

        public static FileInfo ReplaceFileNameWithoutExtension(this FileInfo file, string name) {
            return ReplaceFileName(file, name + file.Extension);
        }

        public static FileInfo ReplaceExtension(this FileInfo file, string extension) {
            return ReplaceFileName(file, Path.GetFileNameWithoutExtension(file.Name) + extension);
        }

        #endregion
    }
}
