using System.Diagnostics;
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
        private const int TIMEOUT_MILLISEC = 5000;

        /// <summary>
        /// Execute Python file with inputContent
        /// </summary>
        /// <param name="pythonFile"></param>
        /// <param name="inputContent"></param>
        /// <returns>Execution Result</returns>
        public async Task<PythonExecutionResult> ExecuteAsync(FileInfo pythonFile, string inputContent = "\n")
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
                RedirectStandardInput = true,
                RedirectStandardError = true,
            };

            Process process = new Process
            {
                StartInfo = psi,
                EnableRaisingEvents = true
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

            string errors = process.StandardError.ReadToEnd();
            bool hadErrors = !string.IsNullOrWhiteSpace(errors);

            var result = new PythonExecutionResult
            {
                HadError = hadErrors
            };

            result.Result = process.StandardOutput.ReadToEnd();

            if (hadErrors)
            {
                result.Errors = errors.ToString();
            }

            process.Close();

            return result;
        }
    }
}
