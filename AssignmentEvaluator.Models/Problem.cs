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

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}
