namespace AssignmentEvaluator.Models
{
    public class TestCase
    {
        public int Id { get; set; }
        public string Comment { get; set; } = "";
        public string Result { get; set; }
        public bool IsPassed { get; set; }
    }
}
