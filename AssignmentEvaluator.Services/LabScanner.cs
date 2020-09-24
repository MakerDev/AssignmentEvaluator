using AssignmentEvaluator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        public List<Student> GenerateStudents()
        {
            List<Student> students = new List<Student>();



            return students;
        }

        /// <summary>
        /// id는 직접 넣어야함
        /// </summary>
        /// <param name="submissionDir"></param>
        /// <returns></returns>
        public Student GenerateStudent(DirectoryInfo submissionDir)
        {
            // 1. 폴더 이름에서 이름 추출
            // 2. 파이썬 파일들을 실행시키고 결과 생성 -> 아직 정답 비교 X
            var splitResults = submissionDir.Name.Split('_');

            string name = splitResults[0];
            int id = _assignmentInfo.StudentNameIdPairs[name];            
            bool hasFilenameError = splitResults.Last() == id.ToString();
            SubmissionState submissionState =
                _assignmentInfo.StudentNameIdPairs.ContainsKey(name)
                ? SubmissionState.OnDate : SubmissionState.NotSubmitted;

            var problems = new List<Problem>();

            var pythonFiles = submissionDir.GetFiles()
                .Where(f => f.Extension == "py")
                .ToDictionary(f => f.Name);

            var pythonNames = _assignmentInfo.ProblemIds.Select(x => $"p{x}").ToList();

            var student = new Student(id, name, problems, hasFilenameError, submissionState);

            return student;
        }

        private Problem GenerateProblem(int problemId, FileInfo pythonFile)
        {
            int testCaseCount = _assignmentInfo.EvaluationContexts[problemId].TestCaseInputs.Count;

            Problem problem = new Problem();

            problem.Id = problemId;
            problem.Code = File.ReadAllText(pythonFile.FullName);



            return problem;
        }
    }
}
