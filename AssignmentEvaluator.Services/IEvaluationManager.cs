using AssignmentEvaluator.Models;
using System;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    public interface IEvaluationManager
    {
        AssignmentInfo AssignmentInfo { get; }

        void ClearEvaluationState();
        Task EvaluateAsync(IProgress<int> progress = null);
        Task ExportCsvAsync();
        Task<AssignmentInfo> LoadLastAssignmentInfo();
        Task<Student> ReevaluateStudent(string name);
        Task SaveAsJsonAsync();
    }
}