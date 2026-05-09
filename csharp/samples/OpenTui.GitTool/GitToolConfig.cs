using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenTui.GitTool;

internal sealed class GitToolConfig
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    public string FileView { get; set; } = "flat";
    public Dictionary<string, CustomCommand> CustomCommands { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public List<string> Sources { get; } = [];

    public static GitToolConfig Load(string repositoryPath)
    {
        var config = new GitToolConfig();
        foreach (var path in EnumerateConfigPaths(repositoryPath))
        {
            if (!File.Exists(path)) continue;
            var loaded = JsonSerializer.Deserialize<GitToolConfig>(File.ReadAllText(path), JsonOptions);
            if (loaded == null) continue;
            config.FileView = loaded.FileView ?? config.FileView;
            foreach (var command in loaded.CustomCommands)
            {
                config.CustomCommands[command.Key] = command.Value;
            }
            config.Sources.Add(path);
        }

        return config;
    }

    public string ToJson() => JsonSerializer.Serialize(new
    {
        fileView = FileView,
        customCommands = CustomCommands,
        sources = Sources,
    }, JsonOptions);

    private static IEnumerable<string> EnumerateConfigPaths(string repositoryPath)
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home))
        {
            yield return Path.Combine(home, ".config", "opentui-git", "config.json");
        }

        var current = new DirectoryInfo(repositoryPath);
        var chain = new Stack<string>();
        while (current != null)
        {
            chain.Push(Path.Combine(current.FullName, ".opentui-git.json"));
            current = current.Parent;
        }

        while (chain.Count > 0)
        {
            yield return chain.Pop();
        }

        yield return Path.Combine(repositoryPath, ".git", "opentui-git.json");
    }
}

internal sealed class CustomCommand
{
    public string Command { get; set; } = "git";
    public List<string> Arguments { get; set; } = [];
}
