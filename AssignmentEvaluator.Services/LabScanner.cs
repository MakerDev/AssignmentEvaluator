using AssignmentEvaluator.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var problem = new Problem
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
                var executionResult = await _pythonExecuter.ExecuteAsync(pythonFile, testCaseInput);

                string comment;
                bool isPassed;

                if (executionResult.HadError)
                {
                    isPassed = false;
                    comment = "실행 중 오류";
                    problem.Feedback += $"Case{i} 실행 중 오류";
                }
                else
                {
                    isPassed = CheckIfPassed(executionResult.Result, context, i, out comment);

                    if (isPassed == false)
                    {
                        problem.Feedback += $"Case{i} {comment}";
                    }
                }

                //TODO : Consider make this async
                var testCase = new TestCase
                {
                    Id = i,
                    Result = executionResult.Result,
                    IsPassed = isPassed,
                    Comment = comment,
                };

                await File.WriteAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"p{problemId}_out_{i}.txt"), executionResult.Result);

                problem.TestCases.Add(testCase);
            }

            return problem;
        }

        private bool CheckIfPassed(string result, EvaluationContext context, int caseNumber, out string comment)
        {
            comment = "";

            //TODO : Check must-have keywords
            foreach (var bannedKeyword in context.BannedKeywords)
            {
                if (!string.IsNullOrWhiteSpace(bannedKeyword) && result.Contains(bannedKeyword))
                {
                    comment += $"| 금지키워드 {bannedKeyword}포함 |";

                    return false;
                }
            }

            if (result.Replace(" ", string.Empty)
                == context.TestCaseResults[caseNumber].Replace(" ", string.Empty))
            {
                return true;
            }
            else
            {
                comment += "실행결과 불일치";

                return false;
            }
        }
    }
}
