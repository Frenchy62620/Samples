using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;

namespace TreeViewCaliburn.FolderBrowser
{
    public class ShellViewModel : Screen
    {
        public ShellViewModel()
        {
            var drives = DriveInfo.GetDrives()
                                  .Where(d => d.DriveType == DriveType.Fixed && !d.Name.StartsWith("C"))
                                  .Select(d => d.Name).ToList();
            Children = new BindableCollection<ABrowser>();
            drives.ForEach(drive => Children.Add(new FolderTreeItemViewModel(this, null, drive)));

        }


        private BindableCollection<ABrowser> children;
        public BindableCollection<ABrowser> Children
        {
            get => children;
            set
            {
                if (children == value) return;
                children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }


        private int nbFilesSelected;
        public int NbFilesSelected
        {
            get => nbFilesSelected;
            set
            {
                nbFilesSelected = value;
                NotifyOfPropertyChange(() => NbFilesSelected);
                NotifyOfPropertyChange(() => CanValidate);
                NotifyOfPropertyChange(() => CanCancel);
            }
        }


        public void TreeView_SelectedItemChanged(object sender, RoutedEventArgs e)
        {

        }

        #region Validate/Cancel

        public bool CanValidate => NbFilesSelected > 0;
        public bool CanCancel => NbFilesSelected > 0;

        public void Validate()
        {
            TryClose(true);
            foreach (var c in GetAllSelected(Children))
                System.Diagnostics.Debug.WriteLine(c.Name);
        }

        public void Cancel()
        {
            foreach (var c in GetAllSelected(Children)) c.IsSelected = false; ;
        }

        public void Close() => TryClose(false);

        #endregion Validate/Cancel

        #region TreeViewItem_Expanded

        public async void TreeViewItem_Expanded(TreeView sender, RoutedEventArgs e)
        {
            var context = (e.OriginalSource as TreeViewItem).DataContext as ABrowser;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            await context.Children.ParallelForEachAsync(ProcessMessageAsync, Environment.ProcessorCount);
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            await Console.Out.WriteLineAsync($"Processing Message: fini {elapsedMs} ms");
        }



        #endregion TreeViewItem_Expanded

        public bool GetAnySelected(IEnumerable<ABrowser> children)
        {
            if (children == null) return false;
            return children.Any(c => c.IsSelected != false || GetAnySelected(c.Children));
        }

        public IEnumerable<ABrowser> GetAllSelected(IEnumerable<ABrowser> item)
        {
            return item.Where(c => c.IsSelected != false && !c.IsNotFile)
                           .Union(item.Where(c => c.Children != null)
                           .SelectMany(y => GetAllSelected(y.Children)));
        }

        public async Task ProcessMessageAsync(ABrowser i)
        {
            if (i.Children == null && i.IsNotFile && i.IsNotDrive)
            {
                i.Children = new BindableCollection<ABrowser>();

                var folders = Directory.EnumerateDirectories(i.Name)
                                       .Where(folder => (File.GetAttributes(folder) & FileAttributes.Hidden) != FileAttributes.Hidden && 
                                                         !folder.EndsWith("OpenClassrooms_fichiers"))
                                       .Select(folder => new FolderTreeItemViewModel(this, i, folder));

                foreach (var folder in folders)
                {
                    i.Children.Add(folder);
                }

                var files = Directory.EnumerateFiles(i.Name, "*OpenClassrooms.htm")
                                     .Select(file => new FolderTreeItemViewModel(this, i, file, true));
                
                i.NbFiles = 0;
                i.IsEnabled = files.Any();
                foreach (var file in files)
                {
                    i.Children.Add(file);
                    i.NbFiles++;
                }
            }
            else
            {
                i.IsEnabled = true;
            }
            //await Console.Out.WriteLineAsync($"Processing Message: {i.Name}");
            await Task.Delay(0);
            //await Task.CompletedTask;
        }



    }
}
