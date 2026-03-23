using System.Text.Json;
using WinFormsE2E.Core;
using WinFormsE2E.Reporting;

namespace WinFormsE2E;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: WinFormsE2E <test-suite.json> [--verbose] [--output <results.json>]");
            return 2;
        }

        var jsonPath = args[0];
        var verbose = args.Contains("--verbose");
        string? outputPath = null;

        var outputIndex = Array.IndexOf(args, "--output");
        if (outputIndex >= 0 && outputIndex + 1 < args.Length)
        {
            outputPath = args[outputIndex + 1];
        }

        if (!System.IO.File.Exists(jsonPath))
        {
            Console.Error.WriteLine($"File not found: {jsonPath}");
            return 2;
        }

        try
        {
            var json = System.IO.File.ReadAllText(jsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };
            var suite = JsonSerializer.Deserialize<Models.TestSuite>(json, options);
            if (suite == null)
            {
                Console.Error.WriteLine("Failed to parse test suite JSON.");
                return 2;
            }

            var reporters = new List<IResultReporter> { new ConsoleReporter(verbose) };
            if (outputPath != null)
            {
                reporters.Add(new JsonReporter(outputPath));
            }

            var runner = new TestRunner(suite, reporters);
            var result = runner.Run();

            return result.Passed ? 0 : 1;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"JSON parse error: {ex.Message}");
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            if (verbose) Console.Error.WriteLine(ex.StackTrace);
            return 2;
        }
    }
}
