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
    public class ShellViewModel: Screen
    {
        public ShellViewModel()
        {
            var drives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed).Select(d => d.Name).ToList();
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


        private bool somethingSelected;
        public bool SomethingSelected
        {
            get => somethingSelected;
            set
            {
                if (somethingSelected == value) return;
                somethingSelected = value;
                NotifyOfPropertyChange(() => CanValidate);
                NotifyOfPropertyChange(() => CanCancel);
            }
        }

        public void TreeView_SelectedItemChanged(object sender, RoutedEventArgs e)
        {

        }

        #region Validate/Cancel

        public bool CanValidate => SomethingSelected;
        public bool CanCancel => SomethingSelected;

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
            //await LoadChildrenItemsAsync(context);
            await context.Children.ParallelForEachAsync(ProcessMessageAsync, Environment.ProcessorCount);

        }

        private async Task LoadChildrenItemsAsync(ABrowser context)
        {
            foreach (var i in context.Children)
            {
                if (i.Children == null && i.IsNotFile && i.IsNotDrive)
                {
                    i.Children = new BindableCollection<ABrowser>();

                    var folders = Directory.EnumerateDirectories(i.Name);
                                           //.Where(f => (File.GetAttributes(f) & FileAttributes.Hidden) != FileAttributes.Hidden)
                                           //.Select(folder => new FolderTreeItemViewModel(this, i, folder));
                    //i.Children.AddRange(folders);

                    foreach (var folder in folders)
                    {
                        i.Children.Add(new FolderTreeItemViewModel(this, i, folder));
                    }

                    var files = Directory.EnumerateFiles(i.Name, "*OpenClassrooms.htm");

                    i.IsEnabled = files.Count() > 0;
                    //i.IsEnabled = files.Any();
                    foreach (var file in files)
                    {
                        i.Children.Add(new FolderTreeItemViewModel(this, i, file, true));
                    }
                }
                else
                {
                    i.IsEnabled = true;
                }
            }
            await Task.Delay(0);

        }

        //private async Task<List<ABrowser>> GetSubItems(string path)
        //{
        //    return Task.Run(() =>
        //    {
        //        var folders = Directory.EnumerateDirectories(path);
        //        var files = Directory.EnumerateFiles(path);
        //    });


        //}

        //private Task<List<ABrowser>> GetSubItems(string path)
        //{
        //    return Task.Run(() =>
        //    {
        //        var folders = Directory.EnumerateDirectories(path);
        //        return new List<ABrowser>();
        //    });

            
        //}


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

        public  async Task ProcessMessageAsync(ABrowser i)
        {
            if (i.Children == null && i.IsNotFile && i.IsNotDrive)
            {
                i.Children = new BindableCollection<ABrowser>();

                var folders = Directory.EnumerateDirectories(i.Name);

                foreach (var folder in folders)
                {
                    if ((File.GetAttributes(folder) & FileAttributes.Hidden) == FileAttributes.Hidden)
                        continue;
                    i.Children.Add(new FolderTreeItemViewModel(this, i, folder));
                    
                }

                var files = Directory.EnumerateFiles(i.Name, "*OpenClassrooms.htm");

                i.IsEnabled = files.Any();
                foreach (var file in files)
                {
                    i.Children.Add(new FolderTreeItemViewModel(this, i, file, true));
                }
            }
            else
            {
                i.IsEnabled = true;
            }
            await Console.Out.WriteLineAsync($"Processing Message: {i.Name}");
        }



    }
}
