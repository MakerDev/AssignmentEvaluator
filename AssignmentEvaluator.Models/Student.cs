using System.Collections.Generic;

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
                    total += problem.NormalizedScore;
                }

                return SubmissionState == SubmissionState.Late ? total/2 : total;
            }
        }

        /// <summary>
        /// Normalize to max of 2
        /// </summary>
        /// <param name="The number of problems"></param>
        /// <returns>Normalized score</returns>
        public double NormalizeScore(int problemCount)
        {
            return 2 * Score / (problemCount*3);
        }
    }
}
