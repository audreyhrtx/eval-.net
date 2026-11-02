# Evaluation - Ordonnancement d'un pipeline

La solution s'appelle `Pipeline` et contient :

- `Pipeline/Pipeline.csproj` : bibliothèque de classes ;
- `Pipeline.Worker/Pipeline.Worker.csproj` : Worker Service ;
- `Pipeline.Tests/Pipeline.Tests.csproj` : tests unitaires xUnit.

## Lancer les tests

```powershell
dotnet test .\Pipeline.sln
```

## Utiliser le Worker

```powershell
$env:PIPELINE_URL = "https://raw.githubusercontent.com/JLou/dotnet-course/main/eval-1/pipeline.json"
dotnet run --project .\Pipeline.Worker\Pipeline.Worker.csproj
```

L'URL peut aussi être fournie avec la configuration .NET :

```powershell
dotnet run --project .\Pipeline.Worker\Pipeline.Worker.csproj -- --PipelineUrl="https://raw.githubusercontent.com/JLou/dotnet-course/main/eval-1/pipeline.json"
```

```powershell
$env:PIPELINE_URL = "https://exemple.fr/pipeline.json"
dotnet run --project .\Pipeline.Worker\Pipeline.Worker.csproj
```

Le planning est calculé par groupes : les tâches d'un même groupe peuvent
être exécutées en parallèle, tandis que chaque groupe attend la fin du
précédent. Les dépendances circulaires déclenchent une
`InvalidOperationException` et un pipeline vide déclenche une
`ArgumentException`.
