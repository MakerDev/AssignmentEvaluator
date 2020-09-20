using AssignmentEvaluator.Models;
using AssignmentEvaluator.WPF.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class StudentViewViewModel : BindableBase
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly IEventAggregator _eventAggregator;

        private int _currentStudentIndex = 0;

        public DelegateCommand MoveToNextStudent { get; set; }
        public DelegateCommand MoveToPreviousStudent { get; set; }

        public StudentViewViewModel(AssignmentInfo assignmentInfo, IEventAggregator eventAggregator)
        {
            _assignmentInfo = assignmentInfo;
            _eventAggregator = eventAggregator;

            MoveToNextStudent = new DelegateCommand(MoveNext, CanMoveNext);
            MoveToPreviousStudent = new DelegateCommand(MoveBack, CanMoveBack);
        }

        private bool CanMoveNext()
        {
            return (_currentStudentIndex + 1) < _assignmentInfo.Students.Count;
        }

        private void MoveNext()
        {

        }

        private bool CanMoveBack()
        {
            return (_currentStudentIndex - 1) >= 0;
        }

        private void MoveBack()
        {

        }
    }
}
