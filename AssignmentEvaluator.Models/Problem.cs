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

        public int Score
        {
            get
            {
                int score = 0;

                foreach (var testCase in TestCases)
                {
                    if (testCase.IsPassed)
                    {
                        score += 3;
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
                if (TestCases.Count == 0)
                {
                    return 0;
                }

                double normalizedScore =  Score / TestCases.Count + AdditionalScore;

                if (normalizedScore < 1)
                {
                    normalizedScore = 1;
                }

                return normalizedScore;
            }
        }

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}
