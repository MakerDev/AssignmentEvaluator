using AssignmentEvaluator.Models;
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
    public class StudentViewModel : BindableBase
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
                Student = _assignmentInfo.Students[_currentStudentIndex];
            }
        }

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

        public StudentViewModel(AssignmentInfo assignmentInfo, IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            _assignmentInfo = assignmentInfo;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;

            MoveToNextStudent = new DelegateCommand(MoveNext, CanMoveNext);
            MoveToPreviousStudent = new DelegateCommand(MoveBack, CanMoveBack);
        }

        private bool CanMoveNext()
        {
            return (_currentStudentIndex + 1) < _assignmentInfo.Students.Count;
        }

        private void MoveNext()
        {
            _currentStudentIndex++;
        }

        private bool CanMoveBack()
        {
            return (_currentStudentIndex - 1) >= 0;
        }

        private void MoveBack()
        {
            _currentStudentIndex--;
        }
    }
}
