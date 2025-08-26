# PumlSchemasManager

Un gestionnaire de schémas PlantUML avec stockage LiteDB et génération de fichiers de sortie.

## Fonctionnalités

- **Découverte automatique** de fichiers PlantUML (.puml, .plantuml, .uml)
- **Parsing intelligent** utilisant la logique PumlPlaner
- **Stockage persistant** avec LiteDB (base de données NoSQL embarquée)
- **Génération multi-formats** (PNG, SVG, PDF, etc.) via PlantUml.Net
- **Gestion de projets** avec organisation des schémas
- **Architecture SOLID** avec injection de dépendances
- **Commandes modulaires** pour chaque opération

## Architecture

Le projet suit les principes SOLID et utilise une architecture en couches :

```
PumlSchemasManager/
├── Domain/           # Modèles de domaine
├── Core/             # Interfaces et contrats
├── Infrastructure/   # Implémentations techniques
├── Application/      # Services métier
├── Commands/         # Commandes d'exécution
└── Program.cs        # Point d'entrée
```

### Technologies utilisées

- **[LiteDB](https://www.litedb.org/docs/)** : Base de données NoSQL embarquée
- **[PlantUml.Net](https://github.com/KevReed/PlantUml.Net)** : Wrapper .NET pour PlantUML
- **[PumlPlaner](../PumlPlaner/)** : Logique de parsing et traitement PlantUML

## Installation

1. **Prérequis** :
   - .NET 9.0
   - Java (pour PlantUML)
   - GraphViz (optionnel, pour certains formats)

2. **Dépendances** :
   ```bash
   dotnet restore
   ```

3. **Compilation** :
   ```bash
   dotnet build
   ```

## Utilisation

### Exemple de base

```csharp
// Initialisation des services
var storageService = new LiteDbStorageService();
var parser = new PlantUmlParser();
var discoveryService = new FileSystemDiscoveryService(parser);
var rendererService = new PlantUmlRendererService();

var schemaManager = new SchemaManager(storageService, discoveryService, rendererService, parser);

// Créer un projet
var project = await schemaManager.CreateProjectAsync("Mon Projet");

// Découvrir des schémas
var schemas = await schemaManager.DiscoverAndAddSchemasAsync(project.Id, "./diagrams");

// Générer des sorties
var formats = new List<OutputFormat> { OutputFormat.Png, OutputFormat.Svg };
var generatedFiles = await schemaManager.GenerateOutputsAsync(project.Id, formats);
```

### Commandes disponibles

- **ParseCommand** : Parser un fichier PlantUML
- **DiscoverCommand** : Découvrir des fichiers PlantUML
- **GenerateCommand** : Générer des fichiers de sortie
- **CreateProjectCommand** : Créer un nouveau projet
- **MergeCommand** : Fusionner plusieurs schémas

## Formats de sortie supportés

- **PNG** : Images bitmap
- **SVG** : Graphiques vectoriels
- **PDF** : Documents PDF
- **EPS** : PostScript encapsulé
- **HTML** : Pages web
- **TXT** : Texte brut
- **LaTeX** : Code LaTeX

## Structure de la base de données

LiteDB stocke les données dans les collections suivantes :

- **projects** : Projets avec leurs métadonnées
- **schemas** : Schémas PlantUML avec contenu
- **generatedFiles** : Fichiers générés avec références
- **FileStorage** : Contenu binaire des fichiers générés

## Exécution

```bash
# Exécuter le programme de démonstration
dotnet run

# Avec un fichier PlantUML spécifique
dotnet run path/to/diagram.puml
```

## Tests

```bash
# Exécuter les tests
dotnet test
```

## Contribution

1. Respecter les principes SOLID
2. Ajouter des docstrings en anglais
3. Suivre les conventions DRY
4. Tester les nouvelles fonctionnalités

## Licence

MIT License
