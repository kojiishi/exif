using NUnit.Framework;
using System;

namespace Exif.Test {
    public class FileExtensionsTest {
        [TestCase(0)]
        [TestCase(1000)]
        [TestCase(1001)]
        [TestCase(1002)]
        [TestCase(1003)]
        public void ContentEqualsByteArrayTest(int length) {
            byte[] b1 = new byte[length], b2 = new byte[length];
            for (int i = 0; i < length; i++)
                b1[i] = b2[i] = (byte)i;
            Assert.IsTrue(FileExtensions.ContentEquals(b1, b2));

            if (length > 0) {
                b2[length - 1] = (byte)length;
                Assert.IsFalse(FileExtensions.ContentEquals(b1, b2));
            }
        }
    }
}
