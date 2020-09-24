using AssignmentEvaluator.Models;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    internal class StudentNameIdPair
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class EvaluationManager
    {
        private readonly AssignmentInfo _assignmentInfo;
        private readonly PythonExecuter _pythonExecuter;

        public EvaluationManager(AssignmentInfo assignmentInfo, PythonExecuter pythonExecuter)
        {
            _assignmentInfo = assignmentInfo;
            _pythonExecuter = pythonExecuter;
        }

        public async Task EvaluateAsync()
        {
            LoadStudentInfosFromCsv(_assignmentInfo.StudentsCsvFile);
            await CreateEvaluationContextsAsync();

            //Evaluate!
        }

        private async Task CreateEvaluationContextsAsync()
        {
            var filesInsideAnswerFolder = new DirectoryInfo(Path.Combine(_assignmentInfo.LabFolderPath, "answers")).GetFiles();
            var pythonFiles = filesInsideAnswerFolder.Where(f => f.Extension == "py").ToList();

            for (int i = 0; i < _assignmentInfo.ProblemIds.Count; i++)
            {
                var pythonFile = pythonFiles[i];
                var testCaseInputs = filesInsideAnswerFolder.Where(x => x.Name.Contains(pythonFile.Name + "_in"))
                                                            .Select(x => File.ReadAllText(x.FullName))
                                                            .ToList();

                var bannedKeywordFile = filesInsideAnswerFolder
                    .FirstOrDefault(f => f.Name.Contains(pythonFile.Name + "_banned"));

                EvaluationContext evaluationContext = new EvaluationContext();
                evaluationContext.ProblemId = _assignmentInfo.ProblemIds[i];
                evaluationContext.TestCaseInputs = testCaseInputs;
                evaluationContext.AnswerCode = await File.ReadAllTextAsync(pythonFile.FullName);
                evaluationContext.BannedKeywords = (await File.ReadAllTextAsync(pythonFile.FullName)).Split('\n').ToList();

                for (int j = 0; j < testCaseInputs.Count; j++)
                {
                    string testCaseInput = testCaseInputs[j];
                    _pythonExecuter.Execute(pythonFile, out string result, testCaseInput);

                    evaluationContext.TestCaseResults.Add(result);

                    await File.WriteAllTextAsync(Path.Combine(pythonFile.DirectoryName, $"{pythonFile.Name}_out_{j}.txt"), result);
                }

                _assignmentInfo.EvaluationContexts.Add(_assignmentInfo.ProblemIds[i], evaluationContext);
            }
        }

        private void LoadStudentInfosFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<StudentNameIdPair>();

                foreach (var record in records)
                {
                    _assignmentInfo.StudentNameIdPairs.Add(record.Name, record.Id);
                }
            }
        }
    }
}
