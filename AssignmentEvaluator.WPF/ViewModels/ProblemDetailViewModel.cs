using AssignmentEvaluator.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;

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
                testCaseViewModels.Add(new TestCaseViewModel(Context, testCase));
            }

            TestCaseViewModels = testCaseViewModels;
        }

        public ObservableCollection<TestCaseViewModel> TestCaseViewModels { get; set; }
        public EvaluationContext Context { get; }
        public Problem Problem { get; }
    }
}
