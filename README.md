
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

```
Project,ProjectPath,Namespace,FileName,FilePath,LineCount,CyclomaticComplexity
Client.Application.EnvDev,Client.Application.EnvDev\Client.Application.EnvDev.csproj,Isagri.Compta.Client.EnvDev.Properties,Settings.Designer.cs,Client.Application.EnvDev\Properties\Settings.Designer.cs,12,3
Client.Application.EnvDev,Client.Application.EnvDev\Client.Application.EnvDev.csproj,Isagri.Compta.Client.EnvDev.Properties,Total,E:\_TFS\Isagri.CO\Main\Isaco\DotNet\Sources\Client.Application.EnvDev,12,0
Client.Application.EnvDev,,,,,Total,12,0
```

## Structure du Projet

```
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
├── README.md
└── NBLignesCount.sln
```

## Contribuer

Les contributions sont les bienvenues ! Veuillez ouvrir une issue ou une pull request pour toute suggestion ou amélioration.

## Licence

Ce projet est sous licence MIT. Voir le fichier [LICENSE](LICENSE) pour plus de détails.
