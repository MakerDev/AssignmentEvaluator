using AssignmentEvaluator.WPF.Core.Dialogs.ViewModels;
using AssignmentEvaluator.WPF.Core.Dialogs.Views;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.WPF.Core
{
    public class CoreModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<MessageDisplayDialog, MessageDisplayDialogViewModel>();
        }
    }
}
