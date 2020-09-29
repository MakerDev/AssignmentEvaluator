using Prism.Unity;
using Prism.Ioc;
using System.Windows;
using AssignmentEvaluator.WPF.Views;
using AssignmentEvaluator.Models;
using AssignmentEvaluator.WPF.ViewModels;
using AssignmentEvaluator.Services;
using Unity;
using Unity.Lifetime;
using AssignmentEvaluator.WPF.Dialogs;

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

            containerRegistry.GetContainer().RegisterFactory(typeof(AssignmentInfo), "assignmentinfo", (c, t, n) =>
            {
                return c.Resolve<EvaluationManager>().AssignmentInfo;
            });

            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
            containerRegistry.RegisterForNavigation<EvaluationView, EvaluationViewModel>();
            containerRegistry.RegisterForNavigation<StudentView, StudentViewModel>();

            containerRegistry.RegisterDialog<EvaluationDialog, EvaluationDialogViewModel>();
        }
    }
}
