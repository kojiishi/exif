using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Exif {
    public class UIExifFile : DependencyObject {
        public UIExifFile(ExifFile file) {
            this.ExifFile = file;
            file.ExifImageLoaded += OnExifImageLoaded;
        }

        public ExifFile ExifFile { get; }
        public string Name { get { return this.ExifFile.File.Name; } }
        public bool IsValid { get { return this.ExifFile.IsValid; } }

        public ExifActionState State {
            get { return (ExifActionState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public string StatusText {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public DateTime? OriginalDateTime {
            get { return (DateTime?)GetValue(OriginalDateTimeProperty); }
            set { SetValue(OriginalDateTimeProperty, value); }
        }

        public string EquipModel {
            get { return (string)GetValue(EquipModelProperty); }
            set { SetValue(EquipModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for State.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ExifActionState), typeof(UIExifFile), new PropertyMetadata(ExifActionState.None));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(UIExifFile), new PropertyMetadata(null));

        public static readonly DependencyProperty OriginalDateTimeProperty =
            DependencyProperty.Register("OriginalDateTime", typeof(DateTime?), typeof(UIExifFile), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for EquipModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EquipModelProperty =
            DependencyProperty.Register("EquipModel", typeof(string), typeof(UIExifFile), new PropertyMetadata(null));

        public void SetPending() {
            this.State = ExifActionState.None;
            this.StatusText = "Pending...";
        }

        void OnExifImageLoaded(object sender, EventArgs e) {
            // Retrieve EXIF properties in the caller's thread,
            // and set in UI thread.
            this.Dispatcher.BeginInvoke(
                new Action<DateTime, string>(this.SetExifProperties),
                this.ExifFile.OriginalDateTime,
                this.ExifFile.EquipModel);
        }

        void SetExifProperties(DateTime originalDateTime, string model) {
            this.OriginalDateTime = originalDateTime;
            this.EquipModel = model;
        }
    }
}
