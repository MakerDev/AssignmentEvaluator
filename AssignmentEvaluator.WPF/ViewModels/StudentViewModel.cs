using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class StudentViewModel : BindableBase, INavigationAware
    {
        private Student _student = new Student();
        public Student Student
        {
            get { return _student; }
            set { SetProperty(ref _student, value); }
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

        public DelegateCommand SaveAsJsonCommand { get; set; }
        public DelegateCommand ExportCsvCommand { get; set; }

        public StudentViewModel(EvaluationManager evaluationManager)
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
    }
}
