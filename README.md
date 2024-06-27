
# CodeLineCounter

## Description

Le projet `CodeLineCounter` est un outil permettant de compter le nombre de lignes de code par fichier, namespace et projet dans une solution .NET. Il calcule également la complexité cyclomatique de chaque fichier.

## Fonctionnalités

- Compte le nombre de lignes de code par fichier, namespace et projet.
- Calcule la complexité cyclomatique de chaque fichier.
- Exporte les résultats dans un fichier CSV.

## Prérequis

- .NET 8.0 SDK installé.
- Les packages NuGet suivants :
  - `Microsoft.CodeAnalysis.CSharp`

## Installation

1. Clonez le dépôt :

    ```sh
    git clone https://github.com/magic5644/NBLignesCount.git
    ```

2. Accédez au répertoire du projet :

    ```sh
    cd NBLignesCount
    ```

3. Installez les packages NuGet nécessaires :

    ```sh
    dotnet restore
    ```

## Utilisation

1. Compilez le projet :

    ```sh
    dotnet build
    ```

2. Exécutez le programme en fournissant le chemin du répertoire contenant les solutions à analyser :

    ```sh
    dotnet run --project CodeLineCounter/CodeLineCounter.csproj "chemin/du/répertoire/avec/solutions"
    ```

3. Sélectionnez la solution à analyser en entrant le numéro correspondant.

## Fichiers générés

Le programme génère un fichier CSV nommé `CodeMetrics.csv` contenant les métriques suivantes :

- `Project`: Nom du projet.
- `ProjectPath`: Chemin relatif du projet.
- `Namespace`: Nom du namespace.
- `FileName`: Nom du fichier.
- `FilePath`: Chemin relatif du fichier.
- `LineCount`: Nombre de lignes de code.
- `CyclomaticComplexity`: Complexité cyclomatique du fichier.

## Exemple de sortie

``` csv
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

## Structure du Projet

``` cmd
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
├── README.md
└── NBLignesCount.sln
```

## Tests Unitaires

Pour exécuter les tests unitaires, utilisez la commande suivante :

```sh
dotnet test
```

## Contribuer

Les contributions sont les bienvenues ! Veuillez ouvrir une issue ou une pull request pour toute suggestion ou amélioration.

## Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.
