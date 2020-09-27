using AssignmentEvaluator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    public class LabScanner
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly PythonExecuter _pythonExecuter;

        //TODO : Refactor this class
        public LabScanner(AssignmentInfo assignmentInfo, PythonExecuter pythonExecuter)
        {
            _assignmentInfo = assignmentInfo;
            _pythonExecuter = pythonExecuter;
        }

        public async Task<Student> GenerateStudentAsync(DirectoryInfo submissionDir)
        {
            var splitResults = submissionDir.Name.Split('_');

            string name = splitResults[0];
            int id = _assignmentInfo.StudentNameIdPairs[name];
            bool hasFilenameError = splitResults.Last() != id.ToString();
            SubmissionState submissionState =
                _assignmentInfo.StudentNameIdPairs.ContainsKey(name)
                ? SubmissionState.OnDate : SubmissionState.NotSubmitted;


            var pythonFiles = submissionDir.GetFiles()
                .Where(f => f.Extension == ".py")
                .ToList();

            var problems = new List<Problem>();

            foreach (int problemId in _assignmentInfo.ProblemIds)
            {
                var pythonFile = pythonFiles.FirstOrDefault(f => f.Name == $"p{problemId}.py");
                var problem = await GenerateProblemAsync(problemId, pythonFile);
                problems.Add(problem);
            }

            var student = new Student
            {
                Id = id,
                Name = name,
                HasFilenameError = hasFilenameError,
                SubmissionState = submissionState,
                IsEvaluationCompleted = false,
                Problems = problems
            };

            return student;
        }

        /// <summary>
        /// Create Problem Object from the given python file. This function returns "Unsubmitted" Problem result 
        /// in case "pythonFile" parameter is null.
        /// </summary>
        /// <param name="problemId"></param>
        /// <param name="pythonFile"></param>
        /// <returns></returns>
        private async Task<Problem> GenerateProblemAsync(int problemId, FileInfo pythonFile)
        {
            if (pythonFile == null)
            {
                return new Problem
                {
                    Id = problemId,
                    Submitted = false,
                    Code = "",
                    Feedback = "미제출",
                    HasNameError = true
                };
            }

            Problem problem = new Problem
            {
                Id = problemId,
                Submitted = true,
                Code = File.ReadAllText(pythonFile.FullName),
                HasNameError = pythonFile.Name != $"p{problemId}.py"
            };

            var context = _assignmentInfo.EvaluationContexts[problemId];

            for (int i = 0; i < context.TestCaseInputs.Count; i++)
            {
                string testCaseInput = context.TestCaseInputs[i];
                var result = await _pythonExecuter.ExecuteAsync(pythonFile, testCaseInput);

                //TODO : TestCaseResults들은 미리 빈칸 없애놓기
                var isPassed = result.Replace(" ", string.Empty) == context.TestCaseResults[i].Replace(" ", string.Empty);

                if (!isPassed)
                {
                    problem.Feedback += $"Case{i} 실행결과 불일치 ";
                }

                //TODO : Consider make this async
                TestCase testCase = new TestCase
                {
                    Id = i,
                    Result = result,
                    IsPassed = isPassed,
                    Comment = isPassed ? "" : "실행결과 불일치",
                };

                await File.WriteAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"p{problemId}_out_{i}.txt"), result);

                problem.TestCases.Add(testCase);
            }

            return problem;
        }
    }
}
