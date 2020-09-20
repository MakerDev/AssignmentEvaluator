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
        public bool IsEvaluationCompleted { get; set; } = false;
        public bool HasFilenameError { get; set; } = false;
        public SubmissionState SubmissionState { get; set; } = SubmissionState.OnDate;
        public List<Problem> Problems { get; set; } = new List<Problem>();
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
