# 🌱 PumlPlaner

> A powerful command-line tool suite for organizing, merging PlantUML schemas and planning your code optimally! 🚀

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![PlantUML](https://img.shields.io/badge/PlantUML-Supported-green.svg)](https://plantuml.com/)

## ✨ Features

- 🔍 **Parse & Analyze** - Deep analysis of PlantUML files with detailed reporting
- 📁 **Project Management** - Organize your schemas into manageable projects
- 🔗 **Schema Merging** - Combine multiple PlantUML files intelligently
- 🎯 **Discovery** - Automatically find PlantUML files in directories
- ⚙️ **Configurable** - Customize parsing modes and settings
- 📊 **Multiple Output Formats** - Generate various output formats from your projects

## 🚀 Quick Start

### Installation

Download the latest release from [GitHub Releases](https://github.com/GlazKrovi/PumlPlaner/releases) 📦

```bash
dotnet add package PumlPlanerCli --source <local-path-to-nupkg>
```

### Basic Usage

```bash
# Parse a single PlantUML file
pumlplaner parse path/to/file.puml

# Create a new project
pumlplaner create-project MyProject

# Discover all PlantUML files in a folder
pumlplaner discover path/to/folder

# Merge multiple schemas
pumlplaner merge schema1.puml schema2.puml
```

## 📋 Available Commands

| Command | Description | Example |
|---------|-------------|---------|
| `parse <file>` | 📄 Parse and analyze a single PlantUML file | `pumlplaner parse diagram.puml` |
| `config` | ⚙️ Configure parser settings and test parsers | `pumlplaner config --show` |
| `list-modes` | 📝 List available parsing modes | `pumlplaner list-modes --verbose` |
| `discover <folder>` | 🔍 Find all PlantUML files in a directory | `pumlplaner discover ./diagrams` |
| `create-project <name>` | 🆕 Create a new PumlPlaner project | `pumlplaner create-project MyProject` |
| `add-schemas <projectId> <schemas...>` | ➕ Add existing schemas to a project | `pumlplaner add-schemas MyProject *.puml` |
| `discover-add <projectId> <folder>` | 🔍➕ Discover and add files to a project | `pumlplaner discover-add MyProject ./diagrams` |
| `merge <schemas...>` | 🔗 Merge multiple PlantUML schemas | `pumlplaner merge schema1.puml schema2.puml` |
| `generate <projectId> <formats...>` | 📊 Generate output files from a project | `pumlplaner generate MyProject pdf,svg` |

### Global Options

- `-h, --help` - 📖 Display help information
- `-v, --version` - 🏷️ Display version information

## 🛠️ Development

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code (recommended)

### Building from Source

```bash
git clone https://github.com/GlazKrovi/PumlPlaner.git
cd PumlPlaner
dotnet build
```

### Running Tests

```bash
dotnet test
```

## 🤝 Contributing

We welcome contributions! 🎉 Here's how you can help:

1. 🐛 **Report Issues** - Found a bug? [Open an issue](https://github.com/GlazKrovi/PumlPlaner/issues) with detailed information
2. 💡 **Suggest Features** - Have an idea? [Create a feature request](https://github.com/GlazKrovi/PumlPlaner/issues)
3. 🔧 **Submit Pull Requests** - Fixed a bug or added a feature? We'd love to see your PR!

### Development Guidelines

- Follow SOLID principles 🏗️
- Write comprehensive docstrings and comments 📝
- Respect DRY principles 🔄
- Add tests for new features 🧪

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [ANTLR4](https://www.antlr.org/) for powerful parsing capabilities
- Inspired by the PlantUML community 🌱
- Thanks to all future contributors who will make this project better! 💚

