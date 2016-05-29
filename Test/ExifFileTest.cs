using NUnit.Framework;
using System;
using System.IO;

namespace Exif.Test {
    public class ExifFileTest {
        static object[] DateTimeFromFileNameTestData = {
            new object[] { "20160102_133543.mp4", new DateTime(2016, 1, 2, 13, 35, 43) },
            new object[] { "20160102_133543.JPG", new DateTime(2016, 1, 2, 13, 35, 43) },
            new object[] { "20160109_194417~7.jpg", new DateTime(2016, 1, 9, 19, 44, 17) },
            new object[] { "VID_20160102_133543.mp4", new DateTime(2016, 1, 2, 13, 35, 43) },
            new object[] { "IMG_20160102_133543.JPG", new DateTime(2016, 1, 2, 13, 35, 43) },
            new object[] { "IMG_0100.JPG", null },
        };

        [TestCaseSource(nameof(DateTimeFromFileNameTestData))]
        public void DateTimeFromFileNameTest(string input, DateTime? expected) {
            var item = new ExifFile(new FileInfo(input));
            Assert.AreEqual(expected, item.DateTimeFromFileName);
        }
    }
}
