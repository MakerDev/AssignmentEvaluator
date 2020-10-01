using AssignmentEvaluator.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class TestCaseViewModel : BindableBase
    {
        private readonly EvaluationContext _context;
        private readonly TestCase _testCase;

        public TestCaseViewModel(EvaluationContext context, TestCase testCase)
        {
            _context = context;
            _testCase = testCase;

            ChangePassedStateCommand = new DelegateCommand(() => IsPassed = !IsPassed);
        }

        public DelegateCommand ChangePassedStateCommand { get; set; }

        public int Id { get { return _testCase.Id; } }
        public string Inputs { get { return _context.TestCaseInputs[Id]; } }
        public string Answer { get { return _context.TestCaseResults[Id]; } }
        public string Result { get { return _testCase.Result; } }

        public bool IsPassed
        {
            get { return _testCase.IsPassed; }
            set
            {
                _testCase.IsPassed = value;
                RaisePropertyChanged(nameof(IsPassed));
            }
        }

        public string Comment
        {
            get { return _testCase.Comment; }
            set
            {
                _testCase.Comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }
    }
}
