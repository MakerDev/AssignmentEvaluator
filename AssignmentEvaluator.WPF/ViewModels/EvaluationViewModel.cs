using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services;
using AssignmentEvaluator.WPF.Core;
using AssignmentEvaluator.WPF.Events;
using Microsoft.VisualBasic;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class EvaluationViewModel : BindableBase
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;

        public DelegateCommand MoveToNextStudent { get; set; }
        public DelegateCommand MoveToPreviousStudent { get; set; }

        private int _currentStudentIndex = 0;
        public int CurrentStudentIndex
        {
            get { return _currentStudentIndex; }
            set
            {
                _currentStudentIndex = value;

                RaisePropertyChanged(nameof(Student));
                RaisePropertyChanged(nameof(CurrentStudentNum));

                MoveToNextStudent.RaiseCanExecuteChanged();
                MoveToPreviousStudent.RaiseCanExecuteChanged();

                SwitchStudent(Student);
            }
        }

        public int CurrentStudentNum { get { return CurrentStudentIndex + 1; } }
        public int CompletedStudentNum { get; set; }

        public int StudentCount { get; set; }

        public List<Student> Students { get; private set; }

        public Student Student
        {
            get
            {
                return Students[CurrentStudentIndex];
            }
        }

        private string _searchString = "" ;
        public string SearchString
        {
            get { return _searchString; }
            set { SetProperty(ref _searchString, value); }
        }

        public DelegateCommand MoveToCommand { get; set; }

        public EvaluationViewModel(EvaluationManager evaluationManager,
            IRegionManager regionManager, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _assignmentInfo = evaluationManager.AssignmentInfo;
            _regionManager = regionManager;
            _dialogService = dialogService;

            Students = _assignmentInfo.Students;
            StudentCount = Students.Count;

            MoveToNextStudent = new DelegateCommand(MoveNext, CanMoveNext);
            MoveToPreviousStudent = new DelegateCommand(MoveBack, CanMoveBack);

            CompletedStudentNum = Students.Where(x => x.IsEvaluationCompleted).Count();

            MoveToCommand = new DelegateCommand(()=>
            {
                MoveToStudent(SearchString);
            });

            eventAggregator.GetEvent<StudentEvaluationCompletedEvent>().Subscribe((isCompleted) =>
            {
                if (isCompleted)
                {
                    CompletedStudentNum++;
                }
                else
                {
                    CompletedStudentNum--;
                }

                RaisePropertyChanged(nameof(CompletedStudentNum));
            });

            //HACK: 생성자가 완전 종료 되기 전에(아마 View에서 InitializeComponent를 진행하고, View의 생성자 종료직전까지는 네비게이션이 안 되는 듯.)
            Task.Delay(100)
                .ConfigureAwait(true)
                .GetAwaiter().OnCompleted(() => SwitchStudent(Student));
        }

        private void MoveToStudent(string idOrName)
        {
            var isId = int.TryParse(idOrName, out int id);
            int newIndex = -1;
            if (isId)
            {
                newIndex = Students.FindIndex(0, StudentCount, (student) =>
                {
                    return student.Id == id;
                });
            }
            else
            {
                newIndex = Students.FindIndex(0, StudentCount, (student) =>
                {
                    return student.Name == idOrName;
                });
            }

            if (newIndex == -1)
            {
                var p = new DialogParameters();
                p.Add("Message", $"Couldn't find student {idOrName}");
                _dialogService.ShowDialog("MessageDisplayDialog", p, null);
                return;
            }
            
            CurrentStudentIndex = newIndex;
        }

        private bool CanMoveNext()
        {
            return (CurrentStudentIndex + 1) < Students.Count;
        }

        private void MoveNext()
        {
            CurrentStudentIndex++;
        }

        private bool CanMoveBack()
        {
            return (CurrentStudentIndex - 1) >= 0;
        }

        private void MoveBack()
        {
            CurrentStudentIndex--;
        }

        private void SwitchStudent(Student student)
        {
            var param = new NavigationParameters();
            param.Add("Student", student);

            _regionManager.RequestNavigate(RegionNames.STUDENT_REGION, "StudentView", param);
        }
    }
}
