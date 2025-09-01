using PumlSchemasManager.Application;
using PumlSchemasManager.Commands;
using PumlSchemasManager.Domain;
using PumlSchemasManager.Infrastructure;

namespace PumlSchemasManager;

/// <summary>
/// Main program demonstrating the usage of PumlSchemasManager
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            // Initialize services with dependency injection
            var storageService = new LiteDbStorageService();
            var parser = new PlantUmlParser();
            var discoveryService = new FileSystemDiscoveryService(parser);
            var rendererService = new PlantUmlRendererService();
            
            var schemaManager = new SchemaManager(storageService, discoveryService, rendererService, parser);
            var projectService = new ProjectService(storageService);

            // Initialize commands
            var parseCommand = new ParseCommand(parser);
            var mergeCommand = new MergeCommand(schemaManager);
            var discoverCommand = new DiscoverCommand(discoveryService);
            var generateCommand = new GenerateCommand(schemaManager);
            var createProjectCommand = new CreateProjectCommand(schemaManager);
            var addSchemasCommand = new AddSchemasCommand(schemaManager);
            var discoverAndAddCommand = new DiscoverAndAddCommand(schemaManager);

            Console.WriteLine("=== PumlSchemasManager Demo ===\n");

            // Example 1: Create a new project
            Console.WriteLine("1. Creating a new project...");
            var project = await createProjectCommand.ExecuteAsync("My PlantUML Project");
            Console.WriteLine($"   Created project: {project.Name} (ID: {project.Id})\n");

            // Example 2: Parse a single file (if provided as argument)
            if (args.Length > 0 && File.Exists(args[0]))
            {
                Console.WriteLine($"2. Parsing file: {args[0]}");
                var parseResult = await parseCommand.ExecuteAsync(args[0]);
                if (parseResult.IsSuccess)
                {
                    Console.WriteLine("   File parsed successfully!");
                    var schema = parseResult.Schema!;
                    schema.ProjectId = project.Id;
                    await storageService.SaveSchemaAsync(schema);
                    project.SchemaIds.Add(schema.Id);
                    await projectService.UpdateProjectAsync(project);
                    Console.WriteLine($"   Added schema to project (ID: {schema.Id})\n");
                }
                else
                {
                    Console.WriteLine($"   Parse failed: {parseResult.ErrorMessage}\n");
                }
            }

            // Example 3: Discover PlantUML files in current directory
            Console.WriteLine("3. Discovering PlantUML files in current directory...");
            var discoveredSchemas = await discoverCommand.ExecuteAsync(".");
            Console.WriteLine($"   Found {discoveredSchemas.Count} PlantUML files");

            if (discoveredSchemas.Any())
            {
                // Add discovered schemas to project
                project = await addSchemasCommand.ExecuteAsync(project.Id, discoveredSchemas);
                Console.WriteLine($"   Added {discoveredSchemas.Count} schemas to project\n");

                // Example 4: Generate outputs
                Console.WriteLine("4. Generating outputs (PNG and SVG)...");
                var formats = new List<SchemaOutputFormat> { SchemaOutputFormat.Png, SchemaOutputFormat.Svg };
                var generatedFiles = await generateCommand.ExecuteAsync(project.Id, formats);
                Console.WriteLine($"   Generated {generatedFiles.Count} files\n");

                // Example 5: Show project statistics
                Console.WriteLine("5. Project statistics:");
                var stats = await projectService.GetProjectStatisticsAsync(project.Id);
                Console.WriteLine($"   Project: {stats.ProjectName}");
                Console.WriteLine($"   Schemas: {stats.SchemaCount}");
                Console.WriteLine($"   Generated files: {stats.GeneratedFileCount}");
                Console.WriteLine($"   Total size: {stats.TotalFileSize} bytes");
                Console.WriteLine($"   Created: {stats.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"   Updated: {stats.UpdatedAt:yyyy-MM-dd HH:mm:ss}\n");
            }

            // Example 6: List all projects
            Console.WriteLine("6. All projects:");
            var allProjects = await projectService.ListProjectsAsync();
            foreach (var p in allProjects)
            {
                Console.WriteLine($"   - {p.Name} (ID: {p.Id})");
            }

            Console.WriteLine("\n=== Demo completed successfully! ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
