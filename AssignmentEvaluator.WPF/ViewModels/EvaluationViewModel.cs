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
    public class EvaluationViewModel : BindableBase, INavigationAware
    {
        private readonly IEvaluationManager _evaluationManager;
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
        
        private AssignmentInfo _assignmentInfo;

        public DelegateCommand MoveToNextStudent { get; set; }
        public DelegateCommand MoveToPreviousStudent { get; set; }
        public DelegateCommand RestartCommand { get; set; }

        private int _currentStudentIndex = 0;
        public int CurrentStudentIndex
        {
            get { return _currentStudentIndex; }
            set
            {
                _currentStudentIndex = value;

                RaisePropertyChanged(nameof(Student));
                RaisePropertyChanged(nameof(CurrentStudentNum));

                MoveToNextStudent?.RaiseCanExecuteChanged();
                MoveToPreviousStudent?.RaiseCanExecuteChanged();

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

        public EvaluationViewModel(IEvaluationManager evaluationManager,
            IRegionManager regionManager, IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _evaluationManager = evaluationManager;
            _regionManager = regionManager;
            _dialogService = dialogService;

            InitializeContext();

            MoveToNextStudent = new DelegateCommand(MoveNext, CanMoveNext);
            MoveToPreviousStudent = new DelegateCommand(MoveBack, CanMoveBack);

            MoveToCommand = new DelegateCommand(()=>
            {
                MoveToStudent(SearchString);
            });

            RestartCommand = new DelegateCommand(() =>
            {
                regionManager.RequestNavigate(RegionNames.CONTENT_REGION, "MainView");
            });

            eventAggregator.GetEvent<StudentEvaluationCompletedEvent>().Subscribe((isCompleted) =>
            {
                if (isCompleted)
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


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            InitializeContext();

            ApplicationCommands.ResizeWindow(1400, 900);
        }

        private void InitializeContext()
        {
            _assignmentInfo = _evaluationManager.AssignmentInfo;
            Students = _assignmentInfo.Students;
            StudentCount = Students.Count;
            CompletedStudentNum = Students.Where(x => x.IsEvaluationCompleted).Count();

            CurrentStudentIndex = 0;
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
            //INFO : Move this if there is a more proper place.
            ApplicationCommands.CloseAllExpanders.RegisteredCommands.Clear();

            var param = new NavigationParameters();
            param.Add("Student", student);

            _regionManager.RequestNavigate(RegionNames.STUDENT_REGION, "StudentView", param);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
