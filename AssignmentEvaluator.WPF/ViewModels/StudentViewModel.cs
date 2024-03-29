﻿using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services;
using AssignmentEvaluator.WPF.Core;
using AssignmentEvaluator.WPF.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class StudentViewModel : BindableBase, INavigationAware
    {
        private Student _student = new();
        public Student Student
        {
            get { return _student; }
            set { 
                SetProperty(ref _student, value);
                SetProblemDetails();
            }
        }

        private bool _savingJson;
        public bool SavingJson
        {
            get { return _savingJson; }
            set
            {
                SetProperty(ref _savingJson, value);
                SaveAsJsonCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _exportingCsv;
        public bool ExportingCsv
        {
            get { return _exportingCsv; }
            set
            {
                SetProperty(ref _exportingCsv, value);
                ExportCsvCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<ProblemDetailViewModel> _problemDetails;
        private readonly IEvaluationManager _evaluationManager;

        public ObservableCollection<ProblemDetailViewModel> ProblemDetails
        {
            get { return _problemDetails; }
            set
            {
                SetProperty(ref _problemDetails, value);
            }
        }

        public DelegateCommand CompleteEvaluationCommand { get; set; }
        public DelegateCommand ReevaluateCommand { get; set; }
        public DelegateCommand SaveAsJsonCommand { get; set; }
        public DelegateCommand ExportCsvCommand { get; set; }
        public CompositeCommand CloseAllExpandersCommand { get; set; } = ApplicationCommands.CloseAllExpanders;

        public StudentViewModel(IEvaluationManager evaluationManager, IEventAggregator eventAggregator)
        {
            SaveAsJsonCommand = new DelegateCommand(async () =>
            {
                SavingJson = true;
                await evaluationManager.SaveAsJsonAsync();
                SavingJson = false;
            }, CanSaveJson);

            ExportCsvCommand = new DelegateCommand(async () =>
            {
                ExportingCsv = true;
                await evaluationManager.ExportCsvAsync();
                ExportingCsv = false;
            }, CanExportCsv);

            ReevaluateCommand = new DelegateCommand(async () =>
            {
                var was_completed = Student.IsEvaluationCompleted;
                var student = await evaluationManager.ReevaluateStudent(Student.Name);

                if (student != null)
                {
                    Student = student;
                }

                //Reset evaluation state to false again.
                eventAggregator.GetEvent<StudentEvaluationCompletedEvent>().Publish(was_completed);
            });

            CompleteEvaluationCommand = new DelegateCommand(() =>
            {
                Student.IsEvaluationCompleted = !Student.IsEvaluationCompleted;
                RaisePropertyChanged(nameof(Student));

                eventAggregator.GetEvent<StudentEvaluationCompletedEvent>().Publish(Student.IsEvaluationCompleted);
            });

            _evaluationManager = evaluationManager;
        }

        private bool CanSaveJson()
        {
            return !SavingJson;
        }

        private bool CanExportCsv()
        {
            return !ExportingCsv;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Student = navigationContext.Parameters["Student"] as Student;
        }

        private void SetProblemDetails()
        {
            var problemDetails = new ObservableCollection<ProblemDetailViewModel>();

            foreach (var problem in Student.Problems)
            {
                var detailViewModel = new ProblemDetailViewModel(
                    _evaluationManager.AssignmentInfo.EvaluationContexts[problem.Id], problem);
                problemDetails.Add(detailViewModel);
            }

            ProblemDetails = problemDetails;
        }
    }
}
