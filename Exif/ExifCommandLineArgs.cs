using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    public class ExifCommandLineArgs {
        ExifCommandLineArgs() { }

        public IList<string> Sources { get; } = new List<string>();
        public string Destination { get; private set; }

        public static ExifCommandLineArgs Parse(string[] args, int skip = 0) {
            var result = new ExifCommandLineArgs();
            for (int i = skip; i < args.Length; i++) {
                var arg = args[i];
                if (arg[0] == '-') {
                    for (int j = 1; j < arg.Length; j++) {
                        switch (arg[j]) {
                        case 'n':
                            ExifFile.DryRun = true;
                            break;
                        case 's':
                            ExifActionQueue.DisableParallel = true;
                            break;
                        default:
                            throw new ArgumentException(string.Format("Option \"{0}\" not supported.", arg[j]));
                        }
                    }
                    continue;
                }

                switch (arg) {
                case "p":
                    ExifFile.DryRun = true;
                    ExifActionQueue.DisableParallel = true;
                    goto case "moveto";
                case "m":
                case "mv":
                case "moveto":
                    if (result.Destination != null)
                        throw new ArgumentException("More than one command is not supported.");
                    result.Destination = args[++i];
                    break;
                default:
                    result.Sources.Add(arg);
                    break;
                }
            }
            return result;
        }
    }
}
