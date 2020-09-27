using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services;
using AssignmentEvaluator.WPF.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly EvaluationManager _evaluationManager;
        private readonly IRegionManager _regionManager;
        private string _studentFilePath;
        public string StudentFilePath
        {
            get { return _studentFilePath; }
            set
            {
                _assignmentInfo.StudentsCsvFile = value;
                SetProperty(ref _studentFilePath, value);
                StartEvaluationCommand.RaiseCanExecuteChanged();
            }
        }

        private string _labFolderPath = "";
        public string LabFolderPath
        {
            get { return _labFolderPath; }
            set
            {
                SetProperty(ref _labFolderPath, value);
                _assignmentInfo.LabFolderPath = value;
                StartEvaluationCommand.RaiseCanExecuteChanged();
            }
        }

        private string _problemNumbers;
        public string ProblemNumbers
        {
            get { return _problemNumbers; }
            set
            {
                SetProperty(ref _problemNumbers, value);
                ParseProblemIds();
                StartEvaluationCommand.RaiseCanExecuteChanged();
            }
        }

        private string _savefileName;
        public string SavefileName
        {
            get { return _savefileName; }
            set
            {
                SetProperty(ref _savefileName, value);
                _assignmentInfo.SavefileName = value;
                StartEvaluationCommand.RaiseCanExecuteChanged();
            }
        }

        #region OPTIONS
        private bool _sortById;
        public bool SortById
        {
            get { return _sortById; }
            set { SetProperty(ref _sortById, value); }
        }


        #endregion

        public DelegateCommand SelectStudentFile { get; set; }
        public DelegateCommand SelectLabFolderCommand { get; set; }
        public DelegateCommand StartEvaluationCommand { get; set; }

        public MainViewModel(EvaluationManager evaluationManager, IRegionManager regionManager)
        {
            _assignmentInfo = evaluationManager.AssignmentInfo;
            _evaluationManager = evaluationManager;
            _regionManager = regionManager;

            SelectLabFolderCommand = new DelegateCommand(SelectLabFolder);
            SelectStudentFile = new DelegateCommand(SelectStudentListFile);
            StartEvaluationCommand = new DelegateCommand(StartEvaluation, CanStartEvaluation);
        }

        private void SelectLabFolder()
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();

            LabFolderPath = dialog.SelectedPath;
        }

        private void SelectStudentListFile()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.ShowDialog();

            StudentFilePath = fileDialog.FileName;

            if (!StudentFilePath.EndsWith("csv"))
            {
                StudentFilePath = "";

                MessageBox.Show("csv파일을 골라주세요");
            }
        }

        private bool CanStartEvaluation()
        {
            return !(string.IsNullOrEmpty(LabFolderPath)
                || string.IsNullOrEmpty(ProblemNumbers)
                || string.IsNullOrEmpty(SavefileName)
                || string.IsNullOrEmpty(StudentFilePath));
        }

        private async void StartEvaluation()
        {
            await _evaluationManager.EvaluateAsync();
            
            _regionManager.RequestNavigate(RegionNames.CONTENT_REGION, "EvaluationView");
        }

        private void ParseProblemIds()
        {
            var problemIds = ProblemNumbers.Split(' ');

            foreach (var problemId in problemIds)
            {
                _assignmentInfo.ProblemIds.Add(int.Parse(problemId));
            }
        }
    }
}
