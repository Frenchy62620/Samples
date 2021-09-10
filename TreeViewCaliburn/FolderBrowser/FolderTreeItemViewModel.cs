using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TreeViewCaliburn.FolderBrowser
{
    public class FolderTreeItemViewModel : ABrowser
    {



        public FolderTreeItemViewModel(ShellViewModel firstparent, ABrowser parent, string item, bool isfile = false)
        {
            //Application.Current.Resources["BackColor"] = Application.Current.Resources["PrimaryHueMidBrush"];
            StyleToUse = (Style)Application.Current.TryFindResource(isfile ? "MaterialDesignActionCheckBox" : "MaterialDesignCheckBox"); //MaterialDesignActionCheckBox MaterialDesignCheckBox PrimaryHueMidBrush
            
            Parent = parent;
            FirstParent = firstparent;
            Name = item;
            IsNotDrive = item.Length > 3;
            Icon = ShellIcon.GetShellIcon(Name);
            IsNotFile = !isfile;
            //IsEnabled = true;
            IsSelected = false;
            Size = IsNotFile ? 18 : 14;
            if (isfile)
            {
                BackColor = IsSelected == true ? Selected : Unselected;
                return;
            }
            else
                BackColor = Primary;


            if (!IsNotDrive)
            {
                var folders = Directory.EnumerateDirectories(item)
                                       .Where(dir => (File.GetAttributes(dir) & FileAttributes.Hidden) != FileAttributes.Hidden)
                                       .Select(dir => new FolderTreeItemViewModel(FirstParent, this, dir));

                Children = new BindableCollection<ABrowser>(folders);

 
                //Children.AddRange(files);
            }
        }




        private Style styleToUse;
        public Style StyleToUse
        {
            get => styleToUse;
            set
            {
                //if (styleToUse == value) return;
                styleToUse = value;
                NotifyOfPropertyChange(() => StyleToUse);
            }
        }


    }
}
