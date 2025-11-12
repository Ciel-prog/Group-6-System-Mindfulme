using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MindfulMe.Services
{
    public class PrologService
    {
        private readonly string _swiPrologPath;
        private readonly string _scriptPath;

        public PrologService(string swiPrologPath = "swipl")
        {
            _swiPrologPath = swiPrologPath;

            // Find the prolog_runner.pl script in the project directory
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _scriptPath = Path.Combine(baseDir, "prolog_runner.pl");

            if (!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException($"Prolog runner script not found at: {_scriptPath}");
            }
        }

        public async Task<List<string>> GetRecommendationsAsync(int stressScore, int moodScore, int sleepHours)
        {
            var recommendations = new List<string>();

            try
            {
                // Build the swipl command - FIXED: Pass arguments separately, not as a list
                var startInfo = new ProcessStartInfo
                {
                    FileName = _swiPrologPath,
                    Arguments = $"-q -s \"{_scriptPath}\" -- {stressScore} {moodScore} {sleepHours}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(_scriptPath)
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                // Read output
                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(error))
                {
                    throw new Exception($"Prolog error: {error}");
                }

                // Parse JSON output
                if (!string.IsNullOrWhiteSpace(output))
                {
                    var jsonDoc = JsonDocument.Parse(output);
                    if (jsonDoc.RootElement.TryGetProperty("recommendations", out var recsArray))
                    {
                        foreach (var rec in recsArray.EnumerateArray())
                        {
                            recommendations.Add(rec.GetString() ?? string.Empty);
                        }
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("error", out var errorProp))
                    {
                        throw new Exception($"Prolog returned error: {errorProp.GetString()}");
                    }
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute Prolog: {ex.Message}", ex);
            }
        }
    }
}