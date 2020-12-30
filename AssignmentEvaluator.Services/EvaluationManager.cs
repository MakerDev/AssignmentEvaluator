using AssignmentEvaluator.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{

    public class EvaluationManager
    {
        private readonly PythonExecuter _pythonExecuter;
        private readonly LabScanner _labScanner;
        private readonly JsonManager _jsonManager;
        private readonly CsvManager _csvManager;

        private List<Problem> _cachedProeblems = null;

        public AssignmentInfo AssignmentInfo { get; set; } = new AssignmentInfo();

        public EvaluationManager(PythonExecuter pythonExecuter, JsonManager jsonManager, CsvManager csvManager)
        {
            _pythonExecuter = pythonExecuter;
            _labScanner = new LabScanner(AssignmentInfo, pythonExecuter);
            _jsonManager = jsonManager;
            _csvManager = csvManager;
        }

        public async Task SaveAsJsonAsync()
        {
            await _jsonManager.SaveAsync(AssignmentInfo, AssignmentInfo.SaveFilePath, false);
        }

        public async Task ExportCsvAsync()
        {
            await _csvManager.ExportCsvAsync(AssignmentInfo);
        }

        public async Task EvaluateAsync(IProgress<int> progress = null)
        {
            AssignmentInfo.StudentNameIdPairs = _csvManager.LoadStudentInfosFromCsv(AssignmentInfo.StudentsCsvFile);

            await CreateEvaluationContextsAsync();

            var assignmentInfoFromSavefile = await _jsonManager.LoadAsync<AssignmentInfo>(AssignmentInfo.SaveFilePath, false);

            if (assignmentInfoFromSavefile == null)
            {
                await EvaluateInternalAsync(progress);
                await SaveAsJsonAsync();
                await ExportCsvAsync();
            }
            else
            {
                var evaluationContext = AssignmentInfo.EvaluationContexts;
                //This overrides existing evaluationContext.
                AssignmentInfo = assignmentInfoFromSavefile;
                AssignmentInfo.EvaluationContexts = evaluationContext;
                progress.Report(100);
            }
        }

        public async Task<Student> ReevaluateStudent(string name)
        {
            var studentDir = Directory.GetDirectories(Path.Combine(AssignmentInfo.LabFolderPath, "codes"))
                                .FirstOrDefault(x => x.Contains(name));

            if (studentDir == null)
            {
                return null;
            }

            var studentIdx = AssignmentInfo.Students.FindIndex(x => x.Name == name);

            if (studentIdx < 0)
            {
                return null;
            }

            AssignmentInfo.Students[studentIdx] = await _labScanner.GenerateStudentAsync(new DirectoryInfo(studentDir));

            return AssignmentInfo.Students[studentIdx];
        }

        private async Task<Student> EvaluateStudentInternalAsync(string name,
            DirectoryInfo studentSubmission,
            Action reportProgress)
        {
            Student student;

            if (studentSubmission == null)
            {
                student = GetNotSubmittedStudent(name, AssignmentInfo.StudentNameIdPairs[name]);
            }
            else
            {
                student = await _labScanner.GenerateStudentAsync(studentSubmission);
            }

            reportProgress();

            return student;
        }

        private async Task EvaluateInternalAsync(IProgress<int> progress = null)
        {
            var studentSumbissionDirs =
                new DirectoryInfo(Path.Combine(AssignmentInfo.LabFolderPath, "codes"))
                .GetDirectories()
                .ToDictionary(d => d.Name.Split('_')[0]);

            var allStudentNames = AssignmentInfo.StudentNameIdPairs.Keys.ToList();
            var allStudentCount = allStudentNames.Count;

            List<Task<Student>> studentTasks = new List<Task<Student>>();
            int doneCount = 0;
            foreach (var name in allStudentNames)
            {
                var studentSubmission = studentSumbissionDirs.GetValueOrDefault(name);
                var task = EvaluateStudentInternalAsync(name, studentSubmission, ()=>
                {
                    Interlocked.Increment(ref doneCount);
                    progress.Report(doneCount * 100 / allStudentCount);
                });

                studentTasks.Add(task);
            }

            await Task.WhenAll(studentTasks);

            studentTasks.ForEach((t) =>
            {
                AssignmentInfo.Students.Add(t.Result);
            });

            if (AssignmentInfo.Options.SortByStudentId)
            {
                AssignmentInfo.Students.Sort((s1, s2) =>
                {
                    return s1.Id - s2.Id;
                });
            }
        }

        private Student GetNotSubmittedStudent(string name, int id)
        {
            if (_cachedProeblems == null)
            {
                _cachedProeblems = new List<Problem>();

                foreach (var context in AssignmentInfo.EvaluationContexts.Values)
                {
                    Problem problem = new Problem
                    {
                        Id = context.ProblemId,
                        Submitted = false,
                        Feedback = "미제출",
                    };

                    for (int i = 0; i < context.TestCaseInputs.Count; i++)
                    {
                        problem.TestCases.Add(new TestCase
                        {
                            Id = i,
                            IsPassed = false,
                        });
                    }

                    _cachedProeblems.Add(problem);
                }
            }

            Student student = new Student
            {
                Name = name,
                Id = id,
                SubmissionState = SubmissionState.NotSubmitted,
                Problems = _cachedProeblems,
            };

            return student;
        }

        private async Task CreateEvaluationContextsAsync()
        {
            var filesInsideAnswerFolder = new DirectoryInfo(Path.Combine(AssignmentInfo.LabFolderPath, "answers")).GetFiles();
            var pythonFiles = filesInsideAnswerFolder.Where(f => f.Extension == ".py").ToList();

            for (int i = 0; i < AssignmentInfo.ProblemIds.Count; i++)
            {
                var pythonFile = pythonFiles[i];
                var pythonFileNameWithoutExtension = pythonFile.Name.Substring(0, 2);

                var testCaseInputs = filesInsideAnswerFolder.Where(x => x.Name.Contains(pythonFileNameWithoutExtension + "_in"))
                                                            .Select(x => File.ReadAllText(x.FullName))
                                                            .ToList();

                var bannedKeywordFile = filesInsideAnswerFolder
                    .FirstOrDefault(f => f.Name.Contains(pythonFileNameWithoutExtension + "_banned"));

                var bannedKeywords = new List<string>();

                if (bannedKeywordFile != null)
                {
                    bannedKeywords = (await File.ReadAllTextAsync(bannedKeywordFile.FullName)).Split('\n').ToList();
                }

                EvaluationContext evaluationContext = new EvaluationContext
                {
                    ProblemId = AssignmentInfo.ProblemIds[i],
                    TestCaseInputs = testCaseInputs,
                    AnswerCode = await File.ReadAllTextAsync(pythonFile.FullName),
                    BannedKeywords = bannedKeywords,
                };

                if (AssignmentInfo.Options.GenerateAnswerFiles)
                {
                    await GenerateAnswerFiles(pythonFile, pythonFileNameWithoutExtension, testCaseInputs, evaluationContext);
                }
                else
                {
                    await ReadAnswersFromExistingFiles(pythonFile, pythonFileNameWithoutExtension, testCaseInputs, evaluationContext);
                }

                AssignmentInfo.EvaluationContexts.Add(AssignmentInfo.ProblemIds[i], evaluationContext);
            }
        }

        //Read from existing files
        private static async Task ReadAnswersFromExistingFiles(FileInfo pythonFile,
                                                               string pythonFileNameWithoutExtension,
                                                               List<string> testCaseInputs,
                                                               EvaluationContext evaluationContext)
        {
            for (int j = 0; j < testCaseInputs.Count; j++)
            {
                string testCaseInput = testCaseInputs[j];
                var result = await File.ReadAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"{pythonFileNameWithoutExtension}_ans_{j}.txt"));

                evaluationContext.TestCaseResults.Add(result);
            }
        }

        private async Task GenerateAnswerFiles(FileInfo pythonFile,
                                               string pythonFileNameWithoutExtension,
                                               List<string> testCaseInputs,
                                               EvaluationContext evaluationContext)
        {
            for (int j = 0; j < testCaseInputs.Count; j++)
            {
                string testCaseInput = testCaseInputs[j];
                var executionResult = await _pythonExecuter.ExecuteAsync(pythonFile, testCaseInput);

                evaluationContext.TestCaseResults.Add(executionResult.Result);

                await File.WriteAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"{pythonFileNameWithoutExtension}_ans_{j}.txt"), executionResult.Result);
            }
        }
    }
}
