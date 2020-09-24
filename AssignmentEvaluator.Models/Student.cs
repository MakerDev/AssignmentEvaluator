using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.Models
{
    public enum SubmissionState
    {
        OnDate,
        Late,
        NotSubmitted,
    }

    public class Student
    {
        public Student(int id, string name, List<Problem> problems, bool hasFilenameError = false, SubmissionState submissionState=SubmissionState.NotSubmitted)
        {
            Id = id;
            Name = name;
            Problems = problems;
            HasFilenameError = hasFilenameError;
            SubmissionState = submissionState;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public List<Problem> Problems { get; set; } = new List<Problem>();
        public SubmissionState SubmissionState { get; set; } = SubmissionState.NotSubmitted;
        
        //압축파일 이름 오류를 말하는 것
        public bool HasFilenameError { get; set; } = false;
        public bool IsEvaluationCompleted { get; set; } = false;

        public double Score
        {
            get
            {
                double total = 0;

                foreach (var problem in Problems)
                {
                    total += problem.Score;
                }

                return total / Problems.Count;
            }
        }

        /// <summary>
        /// Normalize to 2
        /// </summary>
        public double NormalizedScore
        {
            get
            {
                return 3 * Score / (Problems.Count * 3);
            }
        }
    }
}
