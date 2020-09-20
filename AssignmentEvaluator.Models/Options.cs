using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.Models
{
    public class Options
    {
        public bool SortByStudentId { get; set; } = true;
        public bool CompareAnswers { get; set; } = true;
        public bool GenerateAnswerFiles { get; set; } = true;
    }
}
