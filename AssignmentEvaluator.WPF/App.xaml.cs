using Prism.Unity;
using Prism.Ioc;
using System.Windows;
using AssignmentEvaluator.WPF.Views;
using AssignmentEvaluator.Models;
using AssignmentEvaluator.WPF.ViewModels;
using AssignmentEvaluator.Services;

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
            containerRegistry.RegisterSingleton<AssignmentInfo>();
            containerRegistry.RegisterSingleton<EvaluationManager>();
            
            containerRegistry.RegisterForNavigation<StudentView, StudentViewModel>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();
        }
    }
}
