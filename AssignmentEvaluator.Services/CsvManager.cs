using AssignmentEvaluator.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    internal class StudentNameIdPair
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CsvManager
    {
        public async Task ExportCsvAsync(AssignmentInfo assignmentInfo)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(CreateHeader(assignmentInfo.EvaluationContexts.Values));

            double maxScore = 0;

            foreach (var context in assignmentInfo.EvaluationContexts.Values)
            {
                maxScore += context.MaxScore;
            }

            foreach (var student in assignmentInfo.Students)
            {
                List<string> contents = new List<string>();

                if (student.Name.Contains(","))
                {
                    contents.Add($"\"{student.Name}\"");
                }
                else
                {
                    contents.Add(student.Name);
                }

                contents.Add(student.Id.ToString());
                //TODO : Optimize this.
                contents.Add(student.NormalizeScore(assignmentInfo.EvaluationContexts.Count).ToString());

                string feedback = "";

                var evaluationContexts = assignmentInfo.EvaluationContexts;

                foreach (var problem in student.Problems)
                {
                    contents.Add(problem.NormalizedScore.ToString());

                    for (int i = 0; i < evaluationContexts[problem.Id].TestCaseInputs.Count; i++)
                    {
                        if (problem.TestCases.Count == 0)
                        {
                            contents.Add("Not Submitted");
                        }
                        else
                        {
                            contents.Add(problem.TestCases[i].IsPassed.ToString());
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(problem.Feedback))
                    {
                        feedback += $"p{problem.Id}-{problem.Feedback}  ";
                    }

                    contents.Add($"\"{problem.Feedback}\"");
                }

                contents.Add($"\"{feedback}\"");

                builder.AppendLine(string.Join(',', contents.ToArray(), 0, contents.Count));
            }

            string csvPath = Path.Combine(assignmentInfo.LabFolderPath, $"{assignmentInfo.SavefileName}.csv");

            await File.WriteAllTextAsync(csvPath, builder.ToString());
        }

        private string CreateHeader(IEnumerable<EvaluationContext> evaluationContexts)
        {
            List<string> headers = new List<string>
            {
                "Name", "Id", "Score"
            };

            foreach (var context in evaluationContexts)
            {
                headers.Add($"p{context.ProblemId}-score");

                for (int i = 0; i < context.TestCaseInputs.Count; i++)
                {
                    headers.Add($"case{i + 1}");
                }

                headers.Add($"p{context.ProblemId}-Feedback");
            }

            headers.Add("Collected-Feedback");

            return string.Join(',', headers.ToArray(), 0, headers.Count);
        }

        public Dictionary<string, int> LoadStudentInfosFromCsv(string filePath)
        {
            var studentNameIdPairs = new Dictionary<string, int>();

            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<StudentNameIdPair>();

                foreach (var record in records)
                {
                    studentNameIdPairs.Add(record.Name, record.Id);
                }
            }

            studentNameIdPairs = studentNameIdPairs.OrderBy(x=>x.Key).ToDictionary(x => x.Key, x => x.Value);

            return studentNameIdPairs;
        }
    }
}
