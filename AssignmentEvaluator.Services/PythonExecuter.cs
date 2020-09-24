using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AssignmentEvaluator.Services
{
    public class PythonExecuter
    {
        private const int TIMEOUT_MILLISEC = 5000;

        /// <summary>
        /// Execute Python file
        /// </summary>
        /// <param name="pythonFile"></param>
        /// <param name="result"></param>
        /// <param name="inputContent"></param>
        /// <returns>Error messages</returns>
        public string Execute(FileInfo pythonFile, out string result, string inputContent = null)
        {
            var psi = new ProcessStartInfo
            {
                //psi.FileName = @"C:\Program Files\PsychoPy3\python.exe";
                FileName = @"C:\Python38\python.exe",
                Arguments = pythonFile.FullName,

                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };

            var errors = new StringBuilder();
            bool hadErrors = false;

            Process process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!hadErrors)
                {
                    hadErrors = !string.IsNullOrEmpty(e.Data);
                }

                errors.Append(e.Data);
            };

            process.Start();

            if (inputContent != null)
            {
                process.StandardInput.WriteLine(inputContent);
                process.StandardInput.Flush();
            }

            if (!process.WaitForExit(TIMEOUT_MILLISEC))
            {
                process.Kill();
            }

            result = process.StandardOutput.ReadToEnd();

            process.Close();

            return errors.ToString();
        }
    }
}
