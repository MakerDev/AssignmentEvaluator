using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AssignmentEvaluator.Services
{
    public class JsonManager
    {
        public Task<bool> DeleteIfExistsAsync(string fullpath, bool appendExtenstion = true)
        {           
            if (appendExtenstion)
                fullpath += ".json";

            if (File.Exists(fullpath))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<T> LoadAsync<T>(string fullpath, bool appendExtenstion = true) where T : class, new()
        {
            if (appendExtenstion)
                fullpath += ".json";

            if (!File.Exists(fullpath))
            {
                File.Create(fullpath);
                return Task.FromResult<T>(null);
            }

            string jsonString = File.ReadAllText(fullpath);
            T instance = JsonSerializer.Deserialize<T>(jsonString);

            return Task.FromResult(instance);
        }

        public Task SaveAsync<T>(T instance, string fullpath, bool appendExtenstion = true) where T : class
        {
            if (appendExtenstion)
                fullpath += ".json";

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            string jsonString = JsonSerializer.Serialize(instance, options);
            File.WriteAllText(fullpath, jsonString);

            return Task.CompletedTask;
        }
    }
}
