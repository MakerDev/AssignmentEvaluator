using AssignmentEvaluator.Models;
using AssignmentEvaluator.WPF.Core;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class ProblemDetailViewModel : BindableBase
    {
        private bool _isResultExpanded = false;
        public bool IsResultExpanded
        {
            get { return _isResultExpanded; }
            set { SetProperty(ref _isResultExpanded, value); }
        }

        public DelegateCommand CloseAllExpanderCommand { get; set; }

        public ProblemDetailViewModel(EvaluationContext context, Problem problem)
        {
            Context = context;
            Problem = problem;

            var testCaseViewModels = new ObservableCollection<TestCaseViewModel>();

            foreach (var testCase in problem.TestCases)
            {
                testCaseViewModels.Add(new TestCaseViewModel(Context, testCase, OnTestCaseStatusChanged));
            }

            TestCaseViewModels = testCaseViewModels;

            CloseAllExpanderCommand = new DelegateCommand(() =>
            {
                IsResultExpanded = false;
            });

            ApplicationCommands.CloseAllExpanders.RegisterCommand(CloseAllExpanderCommand);
        }

        //To update Score
        private void OnTestCaseStatusChanged()
        {
            RaisePropertyChanged(nameof(Problem));
        }

        public double AdditionalScore
        {
            get { return Problem.AdditionalScore; }
            set
            {
                Problem.AdditionalScore = value;
                RaisePropertyChanged(nameof(Problem));
            }
        }

        public ObservableCollection<TestCaseViewModel> TestCaseViewModels { get; set; }
        public EvaluationContext Context { get; }
        public Problem Problem { get; }
    }
}
