using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services;
using AssignmentEvaluator.WPF.Core;
using AssignmentEvaluator.WPF.Dialogs;
using AssignmentEvaluator.WPF.ViewModels;
using AssignmentEvaluator.WPF.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;
using Unity;

namespace AssignmentEvaluator.WPF
{

    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<ShellView>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<EvaluationManager>();
            containerRegistry.RegisterSingleton<CsvManager>();
            containerRegistry.GetContainer().RegisterFactory(typeof(AssignmentInfo), "assignmentinfo", (c, t, n) =>
            {
                return c.Resolve<EvaluationManager>().AssignmentInfo;
            });

            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<EvaluationView, EvaluationViewModel>();
            containerRegistry.RegisterForNavigation<StudentView, StudentViewModel>();

            containerRegistry.RegisterDialog<EvaluationDialog, EvaluationDialogViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<CoreModule>();
        }
    }
}
