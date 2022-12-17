using AssignmentEvaluator.Models;
using AssignmentEvaluator.Services.Exceptions;
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
    public class EvaluationManager : IEvaluationManager
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

        public async Task<AssignmentInfo> LoadLastAssignmentInfo()
        {
            var path = Path.Combine(AssignmentInfo.CacheFolder, "lastEvaluation.json");

            if (File.Exists(path) == false)
            {
                return null;
            }

            return await JsonManager.LoadAsync<AssignmentInfo>(path, false);
        }

        public async Task SaveAsJsonAsync()
        {
            await JsonManager.SaveAsync(AssignmentInfo, AssignmentInfo.SaveFilePath, false);
        }

        public async Task ExportCsvAsync()
        {
            await _csvManager.ExportCsvAsync(AssignmentInfo);
        }

        public void ClearEvaluationState()
        {
            AssignmentInfo.Students.Clear();
            AssignmentInfo.EvaluationContexts.Clear();
            AssignmentInfo.StudentNameIdPairs.Clear();
        }

        public async Task EvaluateAsync(IProgress<int> progress = null)
        {
            ClearEvaluationState();

            await CacheInfosAsync();

            AssignmentInfo.StudentNameIdPairs = _csvManager.LoadStudentInfosFromCsv(AssignmentInfo.StudentsCsvFile);

            await CreateEvaluationContextsAsync();

            var assignmentInfoFromSavefile = await JsonManager.LoadAsync<AssignmentInfo>(AssignmentInfo.SaveFilePath, false);

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
                foreach (var student in AssignmentInfo.Students)
                {
                    student.Problems = student.Problems.OrderBy(x => x.Id).ToList();
                }
                //Sort all infos
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

        private async Task CacheInfosAsync()
        {
            //Cache CSV file and reset CsvFilePath to the cached file path
            var csvCache = Path.Combine(AssignmentInfo.CacheFolder, "students-cached.csv");

            //같은 파일을 카피하려고 하면 IO exception이 발생
            if (AssignmentInfo.StudentsCsvFile != csvCache)
            {
                File.Copy(AssignmentInfo.StudentsCsvFile, csvCache, true);
                AssignmentInfo.StudentsCsvFile = csvCache;
            }

            await JsonManager.SaveAsync(AssignmentInfo, Path.Combine(AssignmentInfo.CacheFolder, "lastEvaluation"));
        }

        private async Task EvaluateInternalAsync(IProgress<int> progress = null)
        {
            var studentSumbissionDirs =
                new DirectoryInfo(Path.Combine(AssignmentInfo.LabFolderPath, "codes"))
                .GetDirectories()
                .ToDictionary(d => d.Name.Split('_')[0].Split('-')[0]);

            var allStudentNames = AssignmentInfo.StudentNameIdPairs.Keys.ToList();
            var allStudentCount = allStudentNames.Count;

            List<Task<Student>> studentTasks = new List<Task<Student>>();
            int doneCount = 0;
            foreach (var name in allStudentNames)
            {
                var studentSubmission = studentSumbissionDirs.GetValueOrDefault(name);
                var task = EvaluateStudentInternalAsync(name, studentSubmission, () =>
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
            else
            {
                AssignmentInfo.Students.Sort((s1, s2) =>
                {
                    return string.Compare(s1.Name, s2.Name);
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
                        MaxScore = context.MaxScore,
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
            var pythonFiles = filesInsideAnswerFolder.Where(f => f.Extension == ".py").OrderBy(x=>x.Name).ToList();

            for (int i = 0; i < pythonFiles.Count; i++)
            {
                var pythonFile = pythonFiles[i];
                var pythonFileNameWithoutExtension = pythonFile.Name.Split('.')[0];
                var problemId = int.Parse(pythonFileNameWithoutExtension[1..]);

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

                EvaluationContext evaluationContext = new()
                {
                    ProblemId = problemId,
                    MaxScore = AssignmentInfo.ProblemScores[i],
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

                AssignmentInfo.EvaluationContexts.Add(problemId, evaluationContext);
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
                var filename = $"{pythonFileNameWithoutExtension}_ans_{j}.txt";
                var path = Path.Combine(pythonFile.DirectoryName, filename);

                if (File.Exists(path) == false)
                {
                    throw new NoAnswerFileFoundException($"Answer file {filename} is not found.");
                }

                var result = await File.ReadAllTextAsync(path);

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

                var filePath = Path.Combine(pythonFile.DirectoryName, $"{pythonFileNameWithoutExtension}_ans_{j}.txt");
                await File.WriteAllTextAsync(filePath, executionResult.Result);
            }
        }
    }
}
