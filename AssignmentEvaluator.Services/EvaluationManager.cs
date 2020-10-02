﻿using AssignmentEvaluator.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{

    public class EvaluationManager
    {
        private readonly PythonExecuter _pythonExecuter;
        private readonly LabScanner _labScanner;
        private readonly JsonManager _jsonManager;
        private readonly CsvManager _csvManager;

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

                if (AssignmentInfo.Options.GenerateAnswerFiles)
                {
                    for (int j = 0; j < testCaseInputs.Count; j++)
                    {
                        string testCaseInput = testCaseInputs[j];
                        var executionResult = await _pythonExecuter.ExecuteAsync(pythonFile, testCaseInput);

                        evaluationContext.TestCaseResults.Add(executionResult.Result);

                        await File.WriteAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"{pythonFileNameWithoutExtension}_ans_{j}.txt"), executionResult.Result);
                    }
                }
                else
                {
                    //Read from existing files
                    for (int j = 0; j < testCaseInputs.Count; j++)
                    {
                        string testCaseInput = testCaseInputs[j];
                        var result = await File.ReadAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"{pythonFileNameWithoutExtension}_ans_{j}.txt"));

                        evaluationContext.TestCaseResults.Add(result);
                    }
                }

                AssignmentInfo.EvaluationContexts.Add(AssignmentInfo.ProblemIds[i], evaluationContext);
            }
        }


    }
}
