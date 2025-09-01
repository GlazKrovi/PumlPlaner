# PumlPlaner CLI

A powerful command-line interface for managing and analyzing PlantUML schemas.

## 🚀 Installation

### Option 1: Installation Globale (Recommandée)

```bash
# Installer depuis NuGet
dotnet tool install -g PumlPlanerCli

# Vérifier l'installation
pumlplaner --help
```

### Option 2: Installation Locale

```bash
# Cloner le repository
git clone https://github.com/yourusername/PumlPlaner.git
cd PumlPlaner

# Construire le projet
dotnet build PumlPlanerCli

# Exécuter directement
dotnet run --project PumlPlanerCli -- --help
```

### Option 3: Installation depuis le Package Local

```bash
# Construire et packager
dotnet pack PumlPlanerCli --output ./nupkg

# Installer depuis le package local
dotnet tool install -g --add-source ./nupkg PumlPlanerCli
```

## 📋 Commandes Disponibles

### `parse <file>`
Parse et analyse un fichier PlantUML.

```bash
pumlplaner parse diagram.puml
pumlplaner parse diagram.puml --verbose
```

### `discover <folder>`
Découvre les fichiers PlantUML dans un dossier.

```bash
pumlplaner discover ./src
pumlplaner discover ./src --recursive --pattern "*.puml"
```

### `create-project <name>`
Crée un nouveau projet PumlPlaner.

```bash
pumlplaner create-project "MonApplication"
pumlplaner create-project "MonApplication" --description "Description du projet" --output ./output
```

### `add-schemas <projectId> <schemas...>`
Ajoute des schémas existants à un projet.

```bash
pumlplaner add-schemas abc123 schema1.puml schema2.puml
```

### `discover-add <projectId> <folder>`
Découvre et ajoute automatiquement des schémas à un projet.

```bash
pumlplaner discover-add abc123 ./src
pumlplaner discover-add abc123 ./src --recursive
```

### `merge <schemas...>`
Fusionne plusieurs schémas PlantUML.

```bash
pumlplaner merge schema1.puml schema2.puml
pumlplaner merge schema1.puml schema2.puml --output merged.puml --format puml
```

### `generate <projectId> <formats...>`
Génère des fichiers de sortie depuis un projet.

```bash
pumlplaner generate abc123 png svg
pumlplaner generate abc123 png svg --output ./output --quality 90
```

## 🔧 Configuration

### Variables d'Environnement

```bash
# Clé API NuGet (pour la distribution)
export NUGET_API_KEY="your-api-key-here"

# Configuration de sortie par défaut
export PUMLPLANER_OUTPUT_PATH="./output"
export PUMLPLANER_DEFAULT_QUALITY="100"
```

### Fichier de Configuration

Créez un fichier `pumlplaner.config.json` dans votre répertoire utilisateur :

```json
{
  "defaultOutputPath": "./output",
  "defaultQuality": 100,
  "recursiveDiscovery": true,
  "filePatterns": ["*.puml", "*.plantuml"],
  "logging": {
    "level": "Info",
    "includeTimestamp": true
  }
}
```

## 🚀 Distribution

### Pour les Développeurs

1. **Construire le package :**
   ```bash
   .\build-and-distribute.ps1
   ```

2. **Tester localement :**
   ```bash
   dotnet tool install -g --add-source ./nupkg PumlPlanerCli
   pumlplaner --help
   ```

3. **Publier sur NuGet :**
   ```bash
   .\build-and-distribute.ps1 -PushToNuGet
   ```

### Pour les Utilisateurs

1. **Installer :**
   ```bash
   dotnet tool install -g PumlPlanerCli
   ```

2. **Utiliser :**
   ```bash
   pumlplaner --help
   pumlplaner parse mydiagram.puml
   ```

3. **Mettre à jour :**
   ```bash
   dotnet tool update -g PumlPlanerCli
   ```

4. **Désinstaller :**
   ```bash
   dotnet tool uninstall -g PumlPlanerCli
   ```

## 📦 Structure du Package

```
PumlPlanerCli/
├── bin/                    # Binaires compilés
├── nupkg/                  # Packages NuGet générés
├── build-and-distribute.ps1 # Script de distribution
├── README.md               # Ce fichier
├── Program.cs              # Code principal du CLI
└── PumlPlanerCli.csproj   # Configuration du projet
```

## 🐛 Dépannage

### Problèmes Courants

1. **"Command not found" après installation :**
   ```bash
   # Vérifier que le PATH inclut les outils .NET
   echo $PATH | grep -i dotnet
   
   # Réinstaller l'outil
   dotnet tool uninstall -g PumlPlanerCli
   dotnet tool install -g PumlPlanerCli
   ```

2. **Erreurs de dépendances :**
   ```bash
   # Nettoyer et restaurer
   dotnet clean
   dotnet restore
   dotnet build
   ```

3. **Problèmes de permissions :**
   ```bash
   # Sur Linux/macOS, utiliser sudo si nécessaire
   sudo dotnet tool install -g PumlPlanerCli
   ```

## 🤝 Contribution

1. Fork le repository
2. Créer une branche feature (`git checkout -b feature/AmazingFeature`)
3. Commit les changements (`git commit -m 'Add some AmazingFeature'`)
4. Push vers la branche (`git push origin feature/AmazingFeature`)
5. Ouvrir une Pull Request

## 📄 Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

## 🆘 Support

- **Issues :** [GitHub Issues](https://github.com/yourusername/PumlPlaner/issues)
- **Documentation :** [Wiki](https://github.com/yourusername/PumlPlaner/wiki)
- **Discussions :** [GitHub Discussions](https://github.com/yourusername/PumlPlaner/discussions)
