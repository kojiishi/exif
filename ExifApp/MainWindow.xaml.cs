using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Exif {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            this.ActionQueue.StateChanged += OnActionQueueStateChanged;

            this.LoadSettings();
            this.ParseArguments();
        }

        protected override void OnClosing(CancelEventArgs e) {
            this.SaveSettings();

            base.OnClosing(e);
        }

        public ObservableCollection<UIExifFile> Files { get; } = new ObservableCollection<UIExifFile>();
        readonly ExifActionQueue ActionQueue = new ExifActionQueue();
        readonly Dictionary<ExifFile, UIExifFile> UIFromExifFile = new Dictionary<ExifFile, UIExifFile>();

        #region Settings

        public bool IsIdle {
            get { return (bool)GetValue(IsIdleProperty); }
            set { SetValue(IsIdleProperty, value); }
        }

        public string Destination {
            get { return (string)GetValue(DestinationProperty); }
            set { SetValue(DestinationProperty, value); }
        }

        public static readonly DependencyProperty IsIdleProperty =
            DependencyProperty.Register("IsIdle", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));
        public static readonly DependencyProperty DestinationProperty =
            DependencyProperty.Register("Destination", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        void LoadSettings() {
            var settings = Properties.Settings.Default;
            this.Destination = settings.Destination;
        }

        void SaveSettings() {
            var settings = Properties.Settings.Default;
            settings.Destination = this.Destination;
            settings.Save();
        }

        #endregion

        #region Sources

        void ParseArguments() {
            var options = ExifCommandLineArgs.Parse(Environment.GetCommandLineArgs(), 1);
            if (options.Destination != null)
                this.Destination = options.Destination;
            this.AddSources(options.Sources);
        }

        void AddSources(IEnumerable<string> sources) {
            foreach (var item in ExifFile.Enumerate(sources)) {
                var ui = new UIExifFile(item);
                this.UIFromExifFile.Add(item, ui);
                this.Files.Add(ui);
            }
        }

        private void OnClear(object sender, RoutedEventArgs e) {
            if (!this.IsIdle)
                return;
            this.Files.Clear();
            this.UIFromExifFile.Clear();
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);

            if (!this.IsIdle)
                e.Effects = DragDropEffects.None;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        protected override void OnDrop(DragEventArgs e) {
            base.OnDrop(e);

            if (!this.IsIdle)
                return;

            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (paths != null)
                this.AddSources(paths);
        }

        #endregion

        #region Actions

        private void OnStartMove(object sender, RoutedEventArgs e) {
            ExifFile.DryRun = false;
            this.MoveAsync();
        }

        private void OnStartCopy(object sender, RoutedEventArgs e) {
            ExifFile.DryRun = false;
            this.MoveAsync(ExifFile.MoveOptions.Copy);
        }

        private void OnStartDryRun(object sender, RoutedEventArgs e) {
            ExifFile.DryRun = true;
            this.MoveAsync();
        }

        async void MoveAsync(ExifFile.MoveOptions options = ExifFile.MoveOptions.Move) {
            if (!this.IsIdle)
                return;

            this.ActionQueue.Clear();
            this.ActionQueue.AddMoveToFormatString(this.Destination, options);

            ExifFile.Logger = this.OnLog;
            foreach (var item in this.Files)
                item.SetPending();

            this.IsIdle = false;
            try {
                await Task.Run(() => {
                    this.ActionQueue.Run(this.Files.Select(ui => ui.ExifFile));
                });
            } finally {
                this.IsIdle = true;
            }
        }

        #endregion

        #region Item States and Log

        void OnActionQueueStateChanged(object sender, ExifActionQueue.StateChangedEventArgs e) {
            this.UpdateUIItem(e.File, ui => {
                ui.State = e.State;
                if (e.Error != null)
                    ui.StatusText = e.Error.Message;
            });
        }

        void OnLog(ExifFile.LogArgs e) {
            this.UpdateUIItem(e.ExifFile, ui => ui.StatusText = e.Message);
        }

        void UpdateUIItem(ExifFile file, Action<UIExifFile> action) {
            if (!this.Dispatcher.CheckAccess()) {
                this.Dispatcher.BeginInvoke(new Action<ExifFile, Action<UIExifFile>>(this.UpdateUIItem), file, action);
                return;
            }

            var ui = this.UIFromExifFile[file];
            action(ui);
        }

        #endregion
    }
}
