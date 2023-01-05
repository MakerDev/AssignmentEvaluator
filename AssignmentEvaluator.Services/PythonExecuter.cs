using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    public class PythonExecutionResult
    {
        public string Result { get; set; } = "";
        public string Errors { get; set; } = "";
        public bool HadError { get; set; } = false;
    }

    public class PythonExecuter
    {
        private const int TIMEOUT_MILLISEC = 1000;

        /// <summary>
        /// Execute Python file with inputContent
        /// </summary>
        /// <param name="pythonFile"></param>
        /// <param name="inputContent"></param>
        /// <returns>Execution Result</returns>
        public static async Task<PythonExecutionResult> ExecuteAsync(FileInfo pythonFile, string inputContent = "\n")
        {
            //TODO : 지금 한 명 당 파이썬 프로세스 하나라 무거운가..? 풀링 사용하기..?
            var psi = new ProcessStartInfo
            {
                FileName = @"python.exe",
                Arguments = $"\"{pythonFile.FullName}\"",
                WorkingDirectory = @"C:\Test\2021-2.5\lab10",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
            };

            Process process = new()
            {
                StartInfo = psi,
                EnableRaisingEvents = true
            };

            bool infiteLoop = false;

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
                    infiteLoop = true;
                }
            });

            string errors = process.StandardError.ReadToEnd();

            if (infiteLoop)
            {
                if (string.IsNullOrEmpty(errors))
                {
                    errors += "무한루프";
                }
                else
                {
                    errors += "+무한루프";
                }
            }

            bool hadErrors = !string.IsNullOrWhiteSpace(errors);

            var result = new PythonExecutionResult
            {
                HadError = hadErrors,
                Result = process.StandardOutput.ReadToEnd()
            };

            if (hadErrors)
            {
                result.Errors = errors.ToString();
            }

            process.Close();

            return result;
        }
    }
}
