using Caliburn.Micro;
using TreeViewCaliburn.FolderBrowser;

namespace TreeViewCaliburn
{
    public class MainWindowViewModel
    {
        private readonly IWindowManager windowManager;
        public MainWindowViewModel(IEventAggregator eventAggregator, IWindowManager windowManager)
        {
            this.windowManager = windowManager;
        }

        public void SelectFile()
        {
            var b = new ShellViewModel();

            var result = windowManager.ShowDialog(b);


        }
    }
}
