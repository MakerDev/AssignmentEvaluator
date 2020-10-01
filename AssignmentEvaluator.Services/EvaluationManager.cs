using AssignmentEvaluator.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    internal class StudentNameIdPair
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EvaluationManager
    {
        private readonly PythonExecuter _pythonExecuter;
        private readonly LabScanner _labScanner;
        private readonly JsonManager _jsonManager;

        public AssignmentInfo AssignmentInfo { get; set; } = new AssignmentInfo();

        public EvaluationManager(PythonExecuter pythonExecuter, JsonManager jsonManager)
        {
            _pythonExecuter = pythonExecuter;
            _labScanner = new LabScanner(AssignmentInfo, pythonExecuter);
            _jsonManager = jsonManager;
        }

        public async Task SaveAsJsonAsync()
        {
            await _jsonManager.SaveAsync(AssignmentInfo, AssignmentInfo.SaveFilePath, false);
        }

        public async Task ExportCsvAsync()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(CreateHeader(AssignmentInfo.EvaluationContexts.Values));

            double maxScore = 0;

            foreach (var context in AssignmentInfo.EvaluationContexts.Values)
            {
                maxScore += context.TestCaseInputs.Count * 3;
            }

            foreach (var student in AssignmentInfo.Students)
            {
                List<string> contents = new List<string>();
                contents.Add(student.Name);
                contents.Add(student.Id.ToString());
                //TODO : Optimize this.
                contents.Add(student.NormalizeScore(maxScore).ToString());

                string feedback = "";

                var evaluationContexts = AssignmentInfo.EvaluationContexts;

                foreach (var problem in student.Problems)
                {
                    contents.Add(problem.Score.ToString());

                    for (int i = 0; i < evaluationContexts[problem.Id].TestCaseInputs.Count; i++)
                    {
                        if (problem.TestCases.Count == 0)
                        {
                            contents.Add("Not Submitted");
                        }
                        else
                        {
                            contents.Add(problem.TestCases[i].IsPassed.ToString());
                        }
                    }

                    feedback += $"p{problem.Id}-{problem.Feedback}  ";

                    contents.Add(problem.Feedback);
                }

                contents.Add(feedback);

                builder.AppendLine(string.Join(',', contents.ToArray(), 0, contents.Count));
            }

            string csvPath = Path.Combine(AssignmentInfo.LabFolderPath, $"{AssignmentInfo.SavefileName}.csv");

            await File.WriteAllTextAsync(csvPath, builder.ToString());
        }

        private string CreateHeader(IEnumerable<EvaluationContext> evaluationContexts)
        {
            List<string> headers = new List<string>
            {
                "Name", "Id", "Score"
            };

            foreach (var context in evaluationContexts)
            {
                headers.Add($"p{context.ProblemId}-score");

                for (int i = 0; i < context.TestCaseInputs.Count; i++)
                {
                    headers.Add($"case{i + 1}");
                }

                headers.Add($"p{context.ProblemId}-Feedback");
            }

            headers.Add("Collected-Feedback");

            return string.Join(',', headers.ToArray(), 0, headers.Count);
        }

        public async Task EvaluateAsync(IProgress<int> progress = null)
        {
            LoadStudentInfosFromCsv(AssignmentInfo.StudentsCsvFile);

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

        //TODO : Report progress + Create progress dialog
        private async Task EvaluateInternalAsync(IProgress<int> progress = null)
        {
            var studentSumbissionDirs =
                new DirectoryInfo(Path.Combine(AssignmentInfo.LabFolderPath, "codes"))
                .GetDirectories()
                .ToDictionary(d => d.Name.Split('_')[0]);

            var allStudentNames = AssignmentInfo.StudentNameIdPairs.Keys.ToList();
            var allStudentCount = allStudentNames.Count;

            for (int i = 0; i < allStudentNames.Count; i++)
            {
                string name = allStudentNames[i];
                var studentSubmission = studentSumbissionDirs.GetValueOrDefault(name);

                Student student;

                if (studentSubmission == null)
                {
                    student = GetNotSubmittedStudent(name, AssignmentInfo.StudentNameIdPairs[name]);
                }
                else
                {
                    student = await _labScanner.GenerateStudentAsync(studentSubmission);
                }

                AssignmentInfo.Students.Add(student);

                if (progress != null)
                {
                    progress.Report((i + 1) * 100 / allStudentCount);
                }
            }

            if (AssignmentInfo.Options.SortByStudentId)
            {
                AssignmentInfo.Students.Sort((s1, s2) =>
                {
                    return s1.Id - s2.Id;
                });
            }
        }


        private List<Problem> _cachedProeblems = null;

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

                EvaluationContext evaluationContext = new EvaluationContext
                {
                    ProblemId = AssignmentInfo.ProblemIds[i],
                    TestCaseInputs = testCaseInputs,
                    AnswerCode = await File.ReadAllTextAsync(pythonFile.FullName),
                    BannedKeywords = (await File.ReadAllTextAsync(pythonFile.FullName)).Split('\n').ToList()
                };

                for (int j = 0; j < testCaseInputs.Count; j++)
                {
                    string testCaseInput = testCaseInputs[j];
                    var result = await _pythonExecuter.ExecuteAsync(pythonFile, testCaseInput);

                    evaluationContext.TestCaseResults.Add(result);

                    await File.WriteAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"{pythonFileNameWithoutExtension}_out_{j}.txt"), result);
                }

                AssignmentInfo.EvaluationContexts.Add(AssignmentInfo.ProblemIds[i], evaluationContext);
            }
        }

        private void LoadStudentInfosFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<StudentNameIdPair>();

                foreach (var record in records)
                {
                    AssignmentInfo.StudentNameIdPairs.Add(record.Name, record.Id);
                }
            }
        }
    }
}
