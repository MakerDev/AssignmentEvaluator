using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssignmentEvaluator.Models
{
    public enum EditMode
    {
        Edit,
        CreateNew
    }

    public class AssignmentInfo
    {
        public string LabFolderPath { get; set; }
        public string SavefileName { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public int ProblemCount { get; set; }
        public EditMode EditMode { get; set; }
        public List<int> ProblemIds { get; set; }
        public Options Options { get; set; } = new Options();

        private string _savefilePath = null;

        public string SaveFilePath
        {
            get
            {
                if (_savefilePath == null)
                {
                    string fileName = SavefileName + "." + "json";
                    _savefilePath = Path.Combine(LabFolderPath, "채점결과", fileName);
                }

                return _savefilePath;
            }

            private set { _savefilePath = value; }
        }
    }
}
