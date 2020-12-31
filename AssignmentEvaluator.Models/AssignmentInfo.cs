using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace AssignmentEvaluator.Models
{
    public class AssignmentInfo
    {
        [JsonIgnore]
        public static string CacheFolder
        {
            get
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "savefiles");
                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public string LabFolderPath { get; set; } = "";
        public string ResultFolderPath { get { return LabFolderPath; } }
        public string StudentsCsvFile { get; set; }
        public string SavefileName { get; set; } = "";

        public List<Student> Students { get; set; } = new List<Student>();
        public Dictionary<string, int> StudentNameIdPairs { get; set; } = new Dictionary<string, int>();

        public List<int> ProblemIds { get; set; } = new List<int>();

        /// <summary>
        /// Key : problem Id
        /// </summary>        
        [JsonIgnore]
        public Dictionary<int, EvaluationContext> EvaluationContexts { get; set; }
            = new Dictionary<int, EvaluationContext>();

        public int MaxScore
        {
            get
            {
                int maxScore = 0;

                foreach (var context in EvaluationContexts.Values)
                {
                    maxScore += context.MaxScore;
                }

                return maxScore;
            }
        }

        public Options Options { get; set; } = new Options();

        private string _savefilePath = null;
        public string SaveFilePath
        {
            get
            {
                if (_savefilePath == null)
                {
                    string fileName = SavefileName + ".json";
                    _savefilePath = Path.Combine(LabFolderPath, fileName);
                }

                return _savefilePath;
            }

            private set { _savefilePath = value; }
        }
    }
}
