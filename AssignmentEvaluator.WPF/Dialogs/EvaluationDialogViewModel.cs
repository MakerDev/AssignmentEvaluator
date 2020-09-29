using AssignmentEvaluator.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AssignmentEvaluator.WPF.Dialogs
{
    public class EvaluationDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "Evaluation Dialog";

        public event Action<IDialogResult> RequestClose;
        public bool EvaluationCompleted { get; set; } = false;

        public DelegateCommand CloseDialogCommand { get; set; }

        private int _evaluationProgress = 0;
        public int EvaluationProgress
        {
            get { return _evaluationProgress; }
            set {
                SetProperty(ref _evaluationProgress, value); 
            }
        }

        public EvaluationDialogViewModel()
        {
            CloseDialogCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.OK)), CanCloseDialog);
        }

        public bool CanCloseDialog()
        {
            return EvaluationCompleted;
        }

        public void OnDialogClosed()
        {

        }

        public async void OnDialogOpened(IDialogParameters parameters)
        {
            var evaluationManager = parameters.GetValue<EvaluationManager>("EvaluationManager");

            var progress = new Progress<int>(value =>
            {
                EvaluationProgress = value;

                if (value >= 100)
                {
                    EvaluationCompleted = true;
                    CloseDialogCommand.RaiseCanExecuteChanged();
                }
            });

            await evaluationManager.EvaluateAsync(progress);
        }
    }
}
