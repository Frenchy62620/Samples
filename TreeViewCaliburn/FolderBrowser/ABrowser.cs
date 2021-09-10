using Caliburn.Micro;
using System.Windows;
using System.Windows.Media;

namespace TreeViewCaliburn.FolderBrowser
{
    public abstract class ABrowser:PropertyChangedBase
    {
        public readonly SolidColorBrush Primary = (SolidColorBrush)Application.Current.Resources["PrimaryHueMidBrush"];
        public readonly SolidColorBrush Unselected = new SolidColorBrush(Colors.Red);
        public readonly SolidColorBrush Selected = new SolidColorBrush(Colors.ForestGreen);

        private SolidColorBrush backColor;
        public SolidColorBrush BackColor
        {
            get
            {
                return backColor;
            }

            set
            {
                if (backColor == value) return;
                backColor = value;

                NotifyOfPropertyChange(() => BackColor);
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public ImageSource Icon { get; set; }
        //public string Name {get; set; }

        private BindableCollection<string> files;
        public BindableCollection<string> Files
        {
            get => files;
            set
            {
                if (files == value) return;
                files = value;
                NotifyOfPropertyChange(() => Files);
            }
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

        private int size;
        public int Size
        {
            get => size;
            set
            {
                if (size == value) return;
                size = value;
                NotifyOfPropertyChange(() => Size);
            }
        }

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                NotifyOfPropertyChange(() => IsEnabled);
            }
        }


        //private bool? isSelected;
        //public bool? IsSelected
        //{
        //    get => isSelected;
        //    set
        //    {
        //        if (isSelected == value) return;


        //        isSelected = value;
        //        FirstParent.NbFolderSelected += value == false ? -1 : 1;
        //        NotifyOfPropertyChange(() => IsSelected);
        //    }
        //}

        private bool? isSelected;
        public bool? IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value) return;
                isSelected = value;

                if (!IsNotFile)
                {
                    BackColor = value == true ? Selected : Unselected;
                }
                else
                    BackColor = Primary;


                if (Children != null && value != null && IsNotFile && IsNotDrive)
                {
                    foreach (var i in Children)
                    {
                        if (i.IsNotFile) continue;
                        i.IsSelected = value;
                    }
                }
                else if(!IsNotFile && IsNotDrive)
                {
                    var (t, s) = NumberOfFilesSelected(this);

                    if (value == true && t - s > 0 || value == false && s > 0)
                    {
                        Parent.IsSelected = null;
                    }
                    else
                        Parent.IsSelected = value;
                }

                FirstParent.SomethingSelected = value == false ? FirstParent.GetAnySelected(FirstParent.Children) : true;

                NotifyOfPropertyChange(() => IsSelected);
                NotifyOfPropertyChange(() => BackColor);
            }
        }


        public bool IsNotFile { get; set; }
        public bool IsNotDrive { get; set; }

        public ABrowser Parent { get; set; }
        public ShellViewModel FirstParent { get; set; }

        private (int nbFiles, int nbFilesSelected) NumberOfFilesSelected(ABrowser a)
        {
            int s = 0, t = 0;
            //var folder = a.Name.Split('\\').First();
            var list = a.Parent.Children;
            foreach (var f in a.Parent.Children)
            {
                if (!f.IsNotFile) t++;
                if (!f.IsNotFile && f.IsSelected == true) s++;
            }
            return (t, s);
        }
        //public abstract bool GetSelectionStatus<T>(T b);
    }
}
