using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTui.GitTool.Components;
using OpenTui.Razor.Hosting;

namespace OpenTui.GitTool;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        GitToolCliOptions options;
        try
        {
            options = GitToolCliOptions.Parse(args);
            if (options.ShowHelp)
            {
                GitToolHelp.Write(Console.Out);
                return 0;
            }

            if (options.CommandName != null)
            {
                return GitActionExecutor.Execute(options);
            }
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine(exception.Message);
            Console.Error.WriteLine();
            GitToolHelp.Write(Console.Error);
            return 1;
        }

        var snapshot = GitRepositorySnapshot.Load(options.RepositoryPath);
        var config = snapshot.Error == null ? GitToolConfig.Load(snapshot.RepositoryPath) : new GitToolConfig();
        var appState = new GitToolAppState(snapshot, config);

        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureServices((_, services) => services.AddSingleton(appState));
        var host = builder.UseOpenTuiRazor<GitToolApp>().Build();
        await host.RunAsync();

        return snapshot.Error == null ? 0 : 1;
    }
}

public sealed record FileChange(string Status, string Path);

public sealed record CommitEntry(string Hash, string Subject, string Author, string RelativeDate);

public sealed record GitRepositorySnapshot(
    string RepositoryPath,
    string RepositoryName,
    string Branch,
    string? Upstream,
    string StatusSummary,
    IReadOnlyList<FileChange> Changes,
    IReadOnlyList<CommitEntry> Commits,
    string? Error)
{
    public static GitRepositorySnapshot Load(string repositoryPath)
    {
        if (!Directory.Exists(repositoryPath))
        {
            return ErrorSnapshot(repositoryPath, $"Directory does not exist: {repositoryPath}");
        }

        var rootResult = GitCommand.Run(repositoryPath, "rev-parse", "--show-toplevel");
        if (!rootResult.Success)
        {
            return ErrorSnapshot(repositoryPath, rootResult.ErrorMessage);
        }

        var root = rootResult.Output.Trim();
        var repositoryName = Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? "repository";
        var branch = GitCommand.Run(root, "branch", "--show-current").Output.Trim();
        if (string.IsNullOrWhiteSpace(branch))
        {
            branch = GitCommand.Run(root, "rev-parse", "--short", "HEAD").Output.Trim();
            if (string.IsNullOrWhiteSpace(branch)) branch = "detached";
        }

        var upstreamResult = GitCommand.Run(root, "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}");
        var upstream = upstreamResult.Success ? upstreamResult.Output.Trim() : null;
        var changes = LoadChanges(root);
        var commits = LoadCommits(root);
        var summary = changes.Count == 0 ? "working tree clean" : $"{changes.Count} changed file{(changes.Count == 1 ? "" : "s")}";

        return new GitRepositorySnapshot(root, repositoryName, branch, upstream, summary, changes, commits, null);
    }

    private static GitRepositorySnapshot ErrorSnapshot(string repositoryPath, string message)
    {
        var repositoryName = Path.GetFileName(repositoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)) ?? "repository";
        return new GitRepositorySnapshot(repositoryPath, repositoryName, "not a git repository", null, "unavailable", [], [], message);
    }

    private static List<FileChange> LoadChanges(string root)
    {
        var statusResult = GitCommand.Run(root, "status", "--short");
        if (!statusResult.Success) return [];

        var changes = new List<FileChange>();
        foreach (var line in statusResult.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (line.Length < 4) continue;

            var status = line[..2];
            var path = line[3..];
            var renameSeparator = path.IndexOf(" -> ", StringComparison.Ordinal);
            if (renameSeparator >= 0) path = path[(renameSeparator + 4)..];
            changes.Add(new FileChange(status, path));
        }

        return changes;
    }

    private static List<CommitEntry> LoadCommits(string root)
    {
        var logResult = GitCommand.Run(root, "log", "--max-count=10", "--pretty=format:%h%x1f%s%x1f%an%x1f%cr");
        if (!logResult.Success || string.IsNullOrWhiteSpace(logResult.Output)) return [];

        var commits = new List<CommitEntry>();
        foreach (var line in logResult.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split('\u001f');
            if (parts.Length < 4) continue;
            commits.Add(new CommitEntry(parts[0], parts[1], parts[2], parts[3]));
        }

        return commits;
    }
}

internal sealed record GitCommandResult(bool Success, string Output, string Error)
{
    public string ErrorMessage => string.IsNullOrWhiteSpace(Error) ? Output.Trim() : Error.Trim();
}

internal static class GitCommand
{
    public static GitCommandResult Run(string workingDirectory, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        try
        {
            process.Start();
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return new GitCommandResult(false, string.Empty, $"Unable to start git: {exception.Message}");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(2_000))
        {
            process.Kill(entireProcessTree: true);
            return new GitCommandResult(false, string.Empty, "git command timed out");
        }

        var output = outputTask.GetAwaiter().GetResult();
        var error = errorTask.GetAwaiter().GetResult();

        return new GitCommandResult(process.ExitCode == 0, output, error);
    }
}
