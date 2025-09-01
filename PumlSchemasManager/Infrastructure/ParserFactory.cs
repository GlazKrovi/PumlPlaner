using PumlSchemasManager.Core;

namespace PumlSchemasManager.Infrastructure;

/// <summary>
/// Factory for creating and managing PlantUML parsers
/// </summary>
public class ParserFactory
{
    private readonly Dictionary<ParsingMode, IParser> _parsers = new();
    private ParsingMode _defaultMode = ParsingMode.Remote;
    
    public ParserFactory()
    {
        InitializeParsers();
    }
    
    /// <summary>
    /// Gets the default parser
    /// </summary>
    public IParser GetDefaultParser() => GetParser(_defaultMode);
    
    /// <summary>
    /// Gets a parser by mode
    /// </summary>
    public IParser GetParser(ParsingMode mode)
    {
        if (_parsers.TryGetValue(mode, out var parser))
            return parser;
        
        // Fallback to default if requested mode not available
        return GetDefaultParser();
    }
    
    /// <summary>
    /// Sets the default parsing mode
    /// </summary>
    public void SetDefaultMode(ParsingMode mode)
    {
        if (_parsers.ContainsKey(mode))
            _defaultMode = mode;
    }
    
    /// <summary>
    /// Gets all available parsers
    /// </summary>
    public IEnumerable<IParser> GetAvailableParsers() => _parsers.Values.Where(p => p.Capabilities.IsAvailable);
    
    /// <summary>
    /// Gets parser status information
    /// </summary>
    public ParserStatus GetParserStatus()
    {
        var availableParsers = GetAvailableParsers().ToList();
        var defaultParser = GetDefaultParser();
        
        return new ParserStatus
        {
            DefaultMode = _defaultMode,
            AvailableModes = availableParsers.Select(p => p.Mode).ToList(),
            DefaultParser = defaultParser,
            AllParsers = _parsers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
    
    private void InitializeParsers()
    {
        // Add remote parser (always available)
        _parsers[ParsingMode.Remote] = new RemotePlantUmlParser();
        
        // Add local parser (may not be available)
        var localParser = new LocalPlantUmlParser();
        if (localParser.Capabilities.IsAvailable)
        {
            _parsers[ParsingMode.Local] = localParser;
        }
        
        // Add embedded parser
        _parsers[ParsingMode.Embedded] = new PlantUmlParser();
    }
}

/// <summary>
/// Status information about available parsers
/// </summary>
public class ParserStatus
{
    public ParsingMode DefaultMode { get; set; }
    public List<ParsingMode> AvailableModes { get; set; } = new();
    public IParser DefaultParser { get; set; } = null!;
    public Dictionary<ParsingMode, IParser> AllParsers { get; set; } = new();
}
