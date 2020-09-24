using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.Models
{
    public class Problem
    {
        public int Id { get; set; }
        public bool Submitted { get; set; } = true;
        public string Feedback { get; set; } = "";
        public bool HasNameError { get; set; }
        public string Code { get; set; } = "";
        public int _score = 0;
        public int Score
        {
            get
            {
                int score = _score;

                foreach (var testCase in TestCases)
                {
                    if (testCase.IsPassed)
                    {
                        score += 3;
                    }
                }

                return score;
            }

            set
            {
                _score = value;
            }
        }

        public List<TestCase> TestCases { get; set; } = new List<TestCase>();
    }
}
