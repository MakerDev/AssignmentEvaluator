using System.Collections.Generic;

namespace AssignmentEvaluator.Models
{
    public class EvaluationContext
    {
        public int ProblemId { get; set; }
        public string AnswerCode { get; set; }
        public int MaxScore { get { return TestCaseInputs.Count * 3; } private set { } }
        public List<string> BannedKeywords { get; set; } = new List<string>();
        public List<string> TestCaseInputs { get; set; } = new List<string>();
        public List<string> TestCaseResults { get; set; } = new List<string>();
    }
}
