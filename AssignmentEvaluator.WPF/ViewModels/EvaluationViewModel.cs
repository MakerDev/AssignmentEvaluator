using AssignmentEvaluator.Models;
using AssignmentEvaluator.WPF.Core;
using AssignmentEvaluator.WPF.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class EvaluationViewModel : BindableBase
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;

        public DelegateCommand MoveToNextStudent { get; set; }
        public DelegateCommand MoveToPreviousStudent { get; set; }

        private int _currentStudentIndex = 0;
        public int CurrentStudentIndex
        {
            get { return _currentStudentIndex; }
            set
            {
                _currentStudentIndex = value;
                Student = Students[_currentStudentIndex];
            }
        }

        public List<Student> Students { get;  }

        private Student _student;
        public Student Student
        {
            get
            {
                return _student;
            }
            set
            {
                SetProperty(ref _student, value);
            }
        }

        public EvaluationViewModel(AssignmentInfo assignmentInfo, IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            _assignmentInfo = assignmentInfo;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;

            Students = _assignmentInfo.Students;

            MoveToNextStudent = new DelegateCommand(MoveNext, CanMoveNext);
            MoveToPreviousStudent = new DelegateCommand(MoveBack, CanMoveBack);
        }

        private bool CanMoveNext()
        {
            return (_currentStudentIndex + 1) < Students.Count;
        }

        private void MoveNext()
        {
            _currentStudentIndex++;

            SwitchStudent(Student);
        }

        private bool CanMoveBack()
        {
            return (_currentStudentIndex - 1) >= 0;
        }

        private void MoveBack()
        {
            _currentStudentIndex--;

            SwitchStudent(Student);
        }

        private void SwitchStudent(Student student)
        {
            var param = new NavigationParameters();
            param.Add("Student", student);

            _regionManager.RequestNavigate(RegionNames.PROBLEM_LIST_REGION, "StudentView", param);
        }
    }
}
