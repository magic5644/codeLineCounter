using CodeLineCounter.Services;
using CodeLineCounter.Utils;

namespace CodeLineCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryPath;
            if (args.Length == 0)
            {
                Console.WriteLine("Veuillez fournir le chemin du répertoire contenant les solutions à analyser.");
                return;
            }
            else
            {
                directoryPath = args[0];
            }

            var solutionFiles = FileUtils.GetSolutionFiles(directoryPath);

            if (solutionFiles.Count == 0)
            {
                Console.WriteLine("Aucune solution (.sln) trouvée dans le répertoire spécifié.");
                return;
            }

            Console.WriteLine("Solutions disponibles :");
            for (int i = 0; i < solutionFiles.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {Path.GetFileName(solutionFiles[i])}");
            }

            Console.Write("Choisissez une solution à analyser (entrez le numéro) : ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= solutionFiles.Count)
            {
                string solutionPath = solutionFiles[choice - 1];
                string csvFilePath = "CodeMetrics.csv";  // Vous pouvez modifier ce chemin selon vos besoins

                var analyzer = new CodeAnalyzer();
                var (metrics, projectTotals, totalLines) = analyzer.AnalyzeSolution(solutionPath);

                foreach (var metric in metrics)
                {
                    Console.WriteLine($"Project {metric.ProjectName} ({metric.ProjectPath}) - Namespace {metric.NamespaceName} in file {metric.FileName} ({metric.FilePath}) has {metric.LineCount} lines of code and a cyclomatic complexity of {metric.CyclomaticComplexity}.");
                }

                foreach (var projectTotal in projectTotals)
                {
                    Console.WriteLine($"Project {projectTotal.Key} has {projectTotal.Value} total lines of code.");
                }

                Console.WriteLine($"Total lines of code: {totalLines}");

                // Exporter les données au format CSV
                CsvExporter.ExportToCsv(csvFilePath, metrics, projectTotals, totalLines);
                Console.WriteLine($"Les données ont été exportées vers {csvFilePath}");
            }
            else
            {
                Console.WriteLine("Sélection invalide. Veuillez relancer le programme et choisir une option valide.");
            }
        }
    }
}
