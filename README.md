
# CodeLineCounter

[![.NET](https://github.com/magic5644/NBLignesCount/actions/workflows/dotnet.yml/badge.svg)](https://github.com/magic5644/DataMasker/actions/workflows/dotnet.yml)

<div align="center"><img src="./assets/logo.webp" alt="CodeLineCounter logo" width="200" height="200" center="true"></div>

## Description

The `CodeLineCounter` project is a tool that counts the number of lines of code per file, namespace, and project in a .NET solution. It also calculates the cyclomatic complexity of each file.

## Features

- Counts the number of lines of code per file, namespace, and project.
- Calculates the cyclomatic complexity of each file.
- Exports the results to a CSV file.

## Prerequisites

- .NET 8.0 SDK installed.
- The following NuGet packages:
  - `Microsoft.CodeAnalysis.CSharp`
  - `coverlet.collector`
  - `Microsoft.NET.Test.Sdk`
  - `xunit`
  - `xunit.runner.visualstudio`

## Installation

1. Clone the repository:

        ```sh
        git clone https://github.com/magic5644/NBLignesCount.git
        ```

2. Navigate to the project directory:

        ```sh
        cd NBLignesCount
        ```

3. Install the necessary NuGet packages:

        ```sh
        dotnet restore
        ```

## Usage

1. Build the project:

        ```sh
        dotnet build
        ```

2. Run the program by providing the directory path containing the solutions to analyze:

        ```sh
        dotnet run --project CodeLineCounter/CodeLineCounter.csproj -d "path/to/directory/with/solutions"
        ```

    or if you want verbose mode on :

        ```sh
        dotnet run --project CodeLineCounter/CodeLineCounter.csproj -d "path/to/directory/with/solutions" -v
        ```

3. Select the solution to analyze by entering the corresponding number.

## Generated Files

The program generates a CSV file named `CodeMetrics.csv` containing the following metrics:

- `Project`: Project name.
- `ProjectPath`: Relative path of the project.
- `Namespace`: Namespace name.
- `FileName`: File name.
- `FilePath`: Relative path of the file.
- `LineCount`: Number of lines of code.
- `CyclomaticComplexity`: Cyclomatic complexity of the file.

## Example Output

    ```csv
    Project,ProjectPath,Namespace,FileName,FilePath,LineCount,CyclomaticComplexity
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter,Program.cs,CodeLineCounter\Program.cs,56,7
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Models,NamespaceMetrics.cs,CodeLineCounter\Models\NamespaceMetrics.cs,13,1
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,NamespaceMetrics,CodeAnalyzer.cs,CodeLineCounter\Services\CodeAnalyzer.cs,101,10
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Services,CyclomaticComplexityCalculator.cs,CodeLineCounter\Services\CyclomaticComplexityCalculator.cs,65,12
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Utils,CsvExporter.cs,CodeLineCounter\Utils\CsvExporter.cs,32,5
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Utils,FileUtils.cs,CodeLineCounter\Utils\FileUtils.cs,33,3
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter,Total,.\CodeLineCounter,54,0
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Models,Total,.\CodeLineCounter,13,0
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Services,Total,.\CodeLineCounter,114,0
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,NamespaceMetrics,Total,.\CodeLineCounter,46,0
    CodeLineCounter,CodeLineCounter\CodeLineCounter.csproj,CodeLineCounter.Utils,Total,.\CodeLineCounter,62,0
    CodeLineCounter,Total,,,,300,
    CodeLineCounter.Tests,CodeLineCounter.Tests\CodeLineCounter.Tests.csproj,CodeLineCounter.Tests,CodeAnalyzerTests.cs,CodeLineCounter.Tests\CodeAnalyzerTests.cs,19,1
    CodeLineCounter.Tests,CodeLineCounter.Tests\CodeLineCounter.Tests.csproj,TestNamespace,CyclomaticComplexityCalculatorTests.cs,CodeLineCounter.Tests\CyclomaticComplexityCalculatorTests.cs,32,1
    CodeLineCounter.Tests,CodeLineCounter.Tests\CodeLineCounter.Tests.csproj,CodeLineCounter.Tests,Total,.\CodeLineCounter.Tests,27,0
    CodeLineCounter.Tests,CodeLineCounter.Tests\CodeLineCounter.Tests.csproj,TestNamespace,Total,.\CodeLineCounter.Tests,21,0
    CodeLineCounter.Tests,Total,,,,51,
    Total,,,,,351,
    ```

## Project Structure

    ```sh
    NBLignesCount/
    │
    ├── CodeLineCounter/
    │   ├── Models/
    │   │   └── NamespaceMetrics.cs
    │   ├── Services/
    │   │   ├── CodeAnalyzer.cs
    │   │   └── CyclomaticComplexityCalculator.cs
    │   ├── Utils/
    │   │   ├── CsvExporter.cs
    │   │   └── FileUtils.cs
    │   ├── Program.cs
    │   └── CodeLineCounter.csproj
    ├── CodeLineCounter.Tests/
    │   ├── CodeAnalyzerTests.cs
    │   ├── CyclomaticComplexityCalculatorTests.cs
    │   └── CodeLineCounter.Tests.csproj
    ├── .gitignore
    ├── README.md
    ├── LICENSE
    └── CodeLineCounter.sln
    ```

## Unit Tests

To run the unit tests, use the following command:

    ```sh
    dotnet test
    ```

## Contributing

Contributions are welcome! Please open an issue or a pull request for any suggestions or improvements.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
