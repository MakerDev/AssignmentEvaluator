using AssignmentEvaluator.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class ProblemDetailViewModel : BindableBase
    {
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
        }

        //To update Score
        private void OnTestCaseStatusChanged()
        {
            RaisePropertyChanged(nameof(Problem));
        }

        public ObservableCollection<TestCaseViewModel> TestCaseViewModels { get; set; }
        public EvaluationContext Context { get; }
        public Problem Problem { get; }
    }
}
