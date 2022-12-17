using System.Collections.Generic;

namespace AssignmentEvaluator.Models
{
    public class Problem
    {
        public int Id { get; set; }
        public bool Submitted { get; set; } = true;
        public string Feedback { get; set; } = "";
        public bool HasNameError { get; set; } = false;
        public string Code { get; set; } = "";
        public double AdditionalScore { get; set; } = 0;
        public double MaxScore { get; set; } = 3.0;

        public double Score
        {
            get
            {
                double score = 0;

                foreach (var testCase in TestCases)
                {
                    if (testCase.IsPassed)
                    {
                        score += MaxScore/TestCases.Count;
                    }
                }

                return score;
            }
        }

        /// <summary>
        /// Max normalized score is 3.0
        /// </summary>
        public double NormalizedScore
        {
            get
            {
                if (!Submitted || TestCases.Count == 0)
                {
                    return 0;
                }

                return Score + AdditionalScore;
            }
        }

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}
