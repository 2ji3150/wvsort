using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace wvsort {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Listview_sorce = new ObservableCollection<ProcessPreview>();
            listView.DataContext = Listview_sorce;
        }
        int i = 1;
        List<string> Queue = new List<string>();
        List<string> NewTrackNames = new List<string>();

        ObservableCollection<ProcessPreview> Listview_sorce;

        private void Open_Click(object sender, RoutedEventArgs e) {
            i = int.Parse(stratnumber.Text);
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog() { IsFolderPicker = true }) {
                if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
                Listview_sorce.Clear();
                foreach (var wv in Directory.EnumerateFiles(dialog.FileName, "*.wv")) {
                    string trackname = Path.GetFileName(wv);
                    string tracknum = trackname.Substring(0, 3);
                    string newtracknum = $"{i:D2} ";
                    bool needtomodify = (newtracknum != tracknum);
                    if (needtomodify) {
                        string newtrackname = trackname.Remove(0, 3).Insert(0, newtracknum);
                        string newtrackfullpath = Path.Combine(Directory.GetParent(wv).FullName, newtrackname);
                        Listview_sorce.Add(new ProcessPreview() { Before = trackname, After = newtrackname });
                        Queue.Add(wv);
                        NewTrackNames.Add(newtrackname);
                    }
                    else Listview_sorce.Add(new ProcessPreview() { Before = trackname });
                    i++;
                }
                Task.Run(() => Dispatcher.BeginInvoke((Action)(() => Resizelistviewcolumn()), DispatcherPriority.Background));
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
            stratnumber.Text = "01";
            open.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
        }

        private void Start_Click(object sender, RoutedEventArgs e) {
            if (!(Queue.Count > 0)) return;
            string orgparent = Directory.GetParent(Queue[0]).FullName;
            string temproot = Path.Combine(orgparent, "WV_SORT_TEMP");
            for (int k = 0; k < Queue.Count; k++) {
                string temppath = Path.Combine(temproot, k.ToString());
                Directory.CreateDirectory(temppath);
                string tempwv = Path.Combine(temppath, NewTrackNames[k]);
                File.Move(Queue[k], tempwv);
            }
            foreach (var wv in Directory.EnumerateFiles(temproot, "*.wv", SearchOption.AllDirectories)) {
                string originalpath = Path.Combine(orgparent, Path.GetFileName(wv));
                File.Move(wv, originalpath);
            }
            Directory.Delete(temproot, true);
            Queue.Clear();
            NewTrackNames.Clear();
            Listview_sorce.Clear();
            Task.Run(() => Dispatcher.BeginInvoke((Action)(() => Resizelistviewcolumn()), DispatcherPriority.Background));
            stratnumber.Text = i.ToString("D2");
            System.Media.SystemSounds.Asterisk.Play();
            MessageBox.Show("完成です");
        }
        private const double LIST_VIEW_COLUMN_MARGIN = 10;
        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e) => Resizelistviewcolumn();

        void Resizelistviewcolumn() {
            //  ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;
            var listBoxChrome = VisualTreeHelper.GetChild(listView, 0) as FrameworkElement;
            var scrollViewer = VisualTreeHelper.GetChild(listBoxChrome, 0) as ScrollViewer;
            var scrollBar = scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer) as ScrollBar;
            var w = scrollBar.ActualWidth;
            var workingWidth = listView.ActualWidth - LIST_VIEW_COLUMN_MARGIN - w;
            gView.Columns[0].Width = workingWidth * 0.5;
            gView.Columns[1].Width = workingWidth * 0.5;
        }
    }
}
