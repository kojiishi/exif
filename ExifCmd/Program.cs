using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    class Program {
        static int Main(string[] args) {
            try {
                MainCore(args);
            } catch (Exception err) {
                Console.Error.WriteLine(err.ToString());
                return 1;
            }
            return 0;
        }

        static void MainCore(string[] args) {
            ExifFile.Logger = e => {
                var info = new List<string>();
                if (e.ExifFile != null)
                    info.Add(e.ExifFile.File.Name);
                if (e.FileInfo != null && (e.ExifFile == null || e.FileInfo != e.ExifFile.File))
                    info.Add(e.FileInfo.Name);
                info.Add(e.Message);
                Console.WriteLine(string.Join(": ", info));
            };
            var options = ExifCommandLineArgs.Parse(args);
            var queue = new ExifActionQueue();
            queue.AddMoveToFormatString(options.Destination);
            queue.Run(ExifFile.Enumerate(options.Sources));
        }
    }
}
