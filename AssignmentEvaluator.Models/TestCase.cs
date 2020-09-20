using System;
using System.Collections.Generic;
using System.Text;

namespace AssignmentEvaluator.Models
{
    public class TestCase
    {
        public int Id { get; set; }
        public string Comment { get; set; } = "";
        public bool IsPassed { get; set; }
    }
}
