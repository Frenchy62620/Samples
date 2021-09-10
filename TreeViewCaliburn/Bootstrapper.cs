using Caliburn.Micro;
using Ninject;
using System;
using System.Collections.Generic;
using System.Windows;



namespace TreeViewCaliburn
{
    public class Bootstrapper : BootstrapperBase
    {
        private IKernel kernel;

        public Bootstrapper()
        {
            Initialize();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }


        protected override void Configure()
        {
            kernel = new StandardKernel();

            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();

            kernel.Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();


        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            OnSettingsLoaded();

            //Coroutine.BeginExecute(kernel
            //    .Get<SettingsLoaderViewModel>()
            //    .Load(OnSettingsLoaded)
            //    .GetEnumerator());
        }
        private void OnSettingsLoaded()
        {
            DisplayRootViewFor<MainWindowViewModel>();
        }
        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            kernel.Get<ILog>().Error(e.ExceptionObject as Exception);
        }
    }
}
