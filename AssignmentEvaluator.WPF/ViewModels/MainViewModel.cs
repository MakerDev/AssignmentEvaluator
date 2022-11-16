using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services;
using AssignmentEvaluator.WPF.Core;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AssignmentEvaluator.WPF.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly IEvaluationManager _evaluationManager;
        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;

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
        public bool SortById
        {
            get { return _assignmentInfo.Options.SortByStudentId; }
            set
            {
                _assignmentInfo.Options.SortByStudentId = value;
                RaisePropertyChanged(nameof(SortById));
            }
        }

        public bool CompareAnswers
        {
            get { return _assignmentInfo.Options.CompareAnswers; }
            set
            {
                _assignmentInfo.Options.CompareAnswers = value;
                RaisePropertyChanged(nameof(CompareAnswers));
            }
        }

        public bool GenerateAnswerFiles
        {
            get { return _assignmentInfo.Options.GenerateAnswerFiles; }
            set
            {
                _assignmentInfo.Options.GenerateAnswerFiles = value;
                RaisePropertyChanged(nameof(GenerateAnswerFiles));
            }
        }

        #endregion

        public DelegateCommand SelectStudentFile { get; set; }
        public DelegateCommand SelectLabFolderCommand { get; set; }
        public DelegateCommand StartEvaluationCommand { get; set; }
        public DelegateCommand LoadLastAssignmentInfoCommand { get; set; }
        public DelegateCommand ConfigureProblemIdsCommand { get; set; }

        public MainViewModel(IEvaluationManager evaluationManager, IRegionManager regionManager, IDialogService dialogService)
        {
            _assignmentInfo = evaluationManager.AssignmentInfo;
            _evaluationManager = evaluationManager;
            _regionManager = regionManager;
            _dialogService = dialogService;

            SelectLabFolderCommand = new DelegateCommand(SelectLabFolder);
            SelectStudentFile = new DelegateCommand(SelectStudentListFile);
            StartEvaluationCommand = new DelegateCommand(StartEvaluation, CanStartEvaluation);
            LoadLastAssignmentInfoCommand = new DelegateCommand(LoadLastAssignmentInfo);
            ConfigureProblemIdsCommand = new DelegateCommand(ConfigureProblemIds);
        }

        private void StartEvaluation()
        {
            _evaluating = true;
            StartEvaluationCommand.RaiseCanExecuteChanged();

            _dialogService.ShowDialog("EvaluationDialog", null, result =>
            {
                _evaluating = false;

                if (result.Result == ButtonResult.OK)
                {
                    _regionManager.RequestNavigate(RegionNames.CONTENT_REGION, "EvaluationView");
                }
                
                StartEvaluationCommand.RaiseCanExecuteChanged();
            });
        }

        private void ConfigureProblemIds()
        {
            if (string.IsNullOrEmpty(LabFolderPath))
            {
                return;
            }
            var answersDirectory = new DirectoryInfo(Path.Combine(LabFolderPath, "answers"));
            var pythonFiles = answersDirectory.GetFiles().Where(x => x.Extension == ".py").ToList();
            List<int> problemIds = new List<int>();

            foreach (var pythonFile in pythonFiles)
            {
                var id = int.Parse(Regex.Match(pythonFile.Name, @"\d+").Value);
                problemIds.Add(id);
            }

            problemIds.Sort();

            ProblemNumbers = string.Join(' ', problemIds);
        }

        private async void LoadLastAssignmentInfo()
        {
            var info = await _evaluationManager.LoadLastAssignmentInfo();

            if (info == null)
            {
                return;
            }

            LabFolderPath = info.LabFolderPath;
            SavefileName = info.SavefileName;
            StudentFilePath = info.StudentsCsvFile;
            ProblemNumbers = string.Join(' ', info.ProblemIds);

            RaisePropertyChanged(nameof(SortById));
            RaisePropertyChanged(nameof(CompareAnswers));
            RaisePropertyChanged(nameof(GenerateAnswerFiles));
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

        bool _evaluating = false;

        private bool CanStartEvaluation()
        {
            return !(string.IsNullOrEmpty(LabFolderPath)
                || string.IsNullOrEmpty(ProblemNumbers)
                || string.IsNullOrEmpty(SavefileName)
                || string.IsNullOrEmpty(StudentFilePath)
                || _evaluating);
        }

        private void ParseProblemIds()
        {
            var problemIds = ProblemNumbers.Split(' ');

            _assignmentInfo.ProblemIds.Clear();

            foreach (var problemId in problemIds)
            {
                if (string.IsNullOrWhiteSpace(problemId))
                {
                    continue;
                }

                _assignmentInfo.ProblemIds.Add(int.Parse(problemId));
            }

            _assignmentInfo.ProblemIds.Sort();
        }
    }
}
