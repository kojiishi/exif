using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exif {
    public enum ExifActionState {
        None,
        Started,
        Completed,
        Skipped,
        Error,
    }

    public class ExifActionQueue {
        readonly List<Action<ExifFile>> _Actions = new List<Action<ExifFile>>();

        public static bool DisableParallel { get; set; }

        public void Add(Action<ExifFile> operation) {
            Debug.Assert(operation != null);
            this._Actions.Add(operation);
        }

        public void AddMoveToFormatString(string destinationPath, ExifFile.MoveOptions options = ExifFile.MoveOptions.Move) {
            this.Add(item => item.MoveToFormatString(destinationPath, options));
        }

        public void Clear() {
            this._Actions.Clear();
        }

        public void Run(IEnumerable<ExifFile> source) {
            if (DisableParallel) {
                foreach (var item in source)
                    this.RunActions(item);
                return;
            }

            source.AsParallel()
                .WithDegreeOfParallelism(16)
                .Select(item => { this.RunActions(item); return true; })
                .ToArray();
        }

        void RunActions(ExifFile item) {
            if (!item.IsValid) {
                item.Log(null, "Ignored (File not supported)");
                FireStateChanged(item, ExifActionState.Skipped);
                return;
            }

            FireStateChanged(item, ExifActionState.Started);
            item.Log(null, "Started...");
            try {
                foreach (var action in this._Actions)
                    action(item);
                FireStateChanged(item, ExifActionState.Completed);
            } catch (Exception err) {
                Debug.Fail($"Error on {item.Name}", err.ToString());
                FireStateChanged(item, ExifActionState.Error, err);
            }
        }

        public class StateChangedEventArgs : EventArgs {
            public StateChangedEventArgs(ExifFile file, ExifActionState state, Exception error = null) {
                this.File = file;
                this.State = state;
                this.Error = error;
            }

            public ExifFile File { get; }
            public ExifActionState State { get; }
            public Exception Error { get; }
        }

        void FireStateChanged(ExifFile file, ExifActionState state, Exception err = null) {
            if (StateChanged != null)
                StateChanged(this, new StateChangedEventArgs(file, state, err));
        }

        public event EventHandler<StateChangedEventArgs> StateChanged;
    }
}
