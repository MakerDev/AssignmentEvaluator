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
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Problem> Problems { get; set; } = new List<Problem>();
        public SubmissionState SubmissionState { get; set; } = SubmissionState.OnDate;
        
        //압축파일 이름 오류를 말하는 것
        public bool HasFilenameError { get; set; } = false;
        public bool IsEvaluationCompleted { get; set; } = false;

        public double Score
        {
            get
            {
                if (Problems.Count == 0)
                {
                    return 0;
                }

                double total = 0;

                foreach (var problem in Problems)
                {
                    total += problem.Score;
                }

                return total / Problems.Count;
            }
        }

        /// <summary>
        /// Normalize to max of 2
        /// </summary>
        /// <param name="maxScore"></param>
        /// <returns></returns>
        public double NormalizeScore(double maxScore)
        {
            return 2 * Score / maxScore;
        }
    }
}
