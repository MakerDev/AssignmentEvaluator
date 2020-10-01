using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    public class PythonExecuter
    {
        private const int TIMEOUT_MILLISEC = 5000;

        /// <summary>
        /// Execute Python file with inputContent
        /// </summary>
        /// <param name="pythonFile"></param>
        /// <param name="inputContent"></param>
        /// <returns>Execution Result</returns>
        public async Task<string> ExecuteAsync(FileInfo pythonFile, string inputContent = "\n")
        {
            //TODO : 지금 한 명 당 파이썬 프로세스 하나라 무거운가..? 풀링 사용하기..?
            var psi = new ProcessStartInfo
            {
                //psi.FileName = @"C:\Program Files\PsychoPy3\python.exe";
                //FileName = @"C:\Python38\python.exe",
                FileName = @"python.exe",
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
                hadErrors = !string.IsNullOrEmpty(e.Data);

                errors.Append(e.Data);
            };

            await Task.Run(() =>
            {
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
            });

            string result;

            if (hadErrors)
            {
                result = errors.ToString();
            }
            else
            {
                result = process.StandardOutput.ReadToEnd();
            }

            process.Close();

            return result;
        }
    }
}
