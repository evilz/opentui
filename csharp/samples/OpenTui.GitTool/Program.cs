using System.Diagnostics;
using System.Text;
using OpenTui.Core.Ansi;
using OpenTui.Core.Buffer;

namespace OpenTui.GitTool;

internal static class Program
{
    private const int MinWidth = 80;
    private const int MinHeight = 24;

    public static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var repositoryPath = args.Length > 0 ? Path.GetFullPath(args[0]) : Directory.GetCurrentDirectory();
        var snapshot = GitRepositorySnapshot.Load(repositoryPath);
        var width = Math.Max(MinWidth, GetConsoleDimension(ConsoleDimension.Width));
        var height = Math.Max(MinHeight, GetConsoleDimension(ConsoleDimension.Height));

        using var buffer = CellBuffer.Create(width, height, "opentui-git-tool");
        GitDashboardRenderer.Render(buffer, snapshot);
        TerminalBufferWriter.Write(buffer);

        return snapshot.Error == null ? 0 : 1;
    }

    private static int GetConsoleDimension(ConsoleDimension dimension)
    {
        if (Console.IsOutputRedirected) return dimension == ConsoleDimension.Width ? 120 : 34;

        try
        {
            return dimension == ConsoleDimension.Width ? Console.WindowWidth : Console.WindowHeight;
        }
        catch (IOException)
        {
            return dimension == ConsoleDimension.Width ? 120 : 34;
        }
    }
}

internal enum ConsoleDimension
{
    Width,
    Height,
}

internal sealed record FileChange(string Status, string Path);

internal sealed record CommitEntry(string Hash, string Subject, string Author, string RelativeDate);

internal sealed record GitRepositorySnapshot(
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

internal static class GitDashboardRenderer
{
    private static readonly Rgba Background = Rgba.FromInts(0, 0, 0);
    private static readonly Rgba Foreground = Rgba.FromInts(238, 238, 238);
    private static readonly Rgba Muted = Rgba.FromInts(126, 133, 145);
    private static readonly Rgba Border = Rgba.FromInts(58, 65, 77);
    private static readonly Rgba Accent = Rgba.FromInts(92, 200, 255);
    private static readonly Rgba Green = Rgba.FromInts(88, 204, 105);
    private static readonly Rgba Yellow = Rgba.FromInts(255, 202, 88);
    private static readonly Rgba Red = Rgba.FromInts(255, 95, 87);

    public static void Render(CellBuffer buffer, GitRepositorySnapshot snapshot)
    {
        buffer.Clear(Background);
        DrawWindowChrome(buffer);

        if (snapshot.Error != null)
        {
            DrawCenteredError(buffer, snapshot);
            return;
        }

        DrawRepositoryHeader(buffer, snapshot);
        DrawPanes(buffer, snapshot);
        DrawFooter(buffer);
    }

    private static void DrawWindowChrome(CellBuffer buffer)
    {
        buffer.DrawText("●", 2, 1, Red, Background);
        buffer.DrawText("●", 6, 1, Yellow, Background);
        buffer.DrawText("●", 10, 1, Green, Background);
        DrawCenteredText(buffer, 1, "Lazygit", Foreground, TextAttributes.Bold);
    }

    private static void DrawRepositoryHeader(CellBuffer buffer, GitRepositorySnapshot snapshot)
    {
        var upstream = snapshot.Upstream == null ? "no upstream" : $"upstream {snapshot.Upstream}";
        DrawText(buffer, 3, 3, $"{snapshot.RepositoryName}", Accent, TextAttributes.Bold);
        DrawText(buffer, 3, 4, $"branch {snapshot.Branch} · {upstream} · {snapshot.StatusSummary}", Muted);
    }

    private static void DrawPanes(CellBuffer buffer, GitRepositorySnapshot snapshot)
    {
        var top = 6;
        var bottomMargin = 3;
        var paneHeight = Math.Max(8, buffer.Height - top - bottomMargin);
        var gap = 2;
        var leftWidth = Math.Clamp(buffer.Width / 3, 28, 44);
        var rightX = leftWidth + gap + 2;
        var rightWidth = buffer.Width - rightX - 2;
        var commitsHeight = Math.Max(8, paneHeight / 2);
        var detailsY = top + commitsHeight + 1;
        var detailsHeight = Math.Max(6, buffer.Height - detailsY - bottomMargin);

        DrawChangesPane(buffer, 2, top, leftWidth, paneHeight, snapshot.Changes);
        DrawCommitsPane(buffer, rightX, top, rightWidth, commitsHeight, snapshot.Commits);
        DrawDetailsPane(buffer, rightX, detailsY, rightWidth, detailsHeight, snapshot);
    }

    private static void DrawChangesPane(CellBuffer buffer, int x, int y, int width, int height, IReadOnlyList<FileChange> changes)
    {
        buffer.DrawBox(x, y, width, height, Border, Background, BorderStyle.Rounded, BorderSides.All, fill: true, title: " Files ");
        if (changes.Count == 0)
        {
            DrawText(buffer, x + 2, y + 2, "Nothing to commit", Green);
            DrawText(buffer, x + 2, y + 3, "Working tree clean", Muted);
            return;
        }

        var visible = Math.Min(changes.Count, height - 3);
        for (var i = 0; i < visible; i++)
        {
            var change = changes[i];
            var color = StatusColor(change.Status);
            DrawText(buffer, x + 2, y + 2 + i, change.Status.Trim().PadRight(2), color, TextAttributes.Bold);
            DrawText(buffer, x + 6, y + 2 + i, change.Path, Foreground, TextAttributes.None, width - 8);
        }
    }

    private static void DrawCommitsPane(CellBuffer buffer, int x, int y, int width, int height, IReadOnlyList<CommitEntry> commits)
    {
        buffer.DrawBox(x, y, width, height, Border, Background, BorderStyle.Rounded, BorderSides.All, fill: true, title: " Recent commits ");
        if (commits.Count == 0)
        {
            DrawText(buffer, x + 2, y + 2, "No commits yet", Muted);
            return;
        }

        var visible = Math.Min(commits.Count, height - 3);
        for (var i = 0; i < visible; i++)
        {
            var commit = commits[i];
            var row = y + 2 + i;
            DrawText(buffer, x + 2, row, commit.Hash, Yellow, TextAttributes.Bold);
            DrawText(buffer, x + 11, row, commit.Subject, Foreground, TextAttributes.None, width - 27);
            DrawText(buffer, x + width - 14, row, Truncate(commit.RelativeDate, 12), Muted);
        }
    }

    private static void DrawDetailsPane(CellBuffer buffer, int x, int y, int width, int height, GitRepositorySnapshot snapshot)
    {
        buffer.DrawBox(x, y, width, height, Border, Background, BorderStyle.Rounded, BorderSides.All, fill: true, title: " Repository ");
        DrawText(buffer, x + 2, y + 2, "Path", Muted, TextAttributes.Bold);
        DrawText(buffer, x + 13, y + 2, snapshot.RepositoryPath, Foreground, TextAttributes.None, width - 15);
        DrawText(buffer, x + 2, y + 4, "Branch", Muted, TextAttributes.Bold);
        DrawText(buffer, x + 13, y + 4, snapshot.Branch, Accent);
        DrawText(buffer, x + 2, y + 5, "Status", Muted, TextAttributes.Bold);
        DrawText(buffer, x + 13, y + 5, snapshot.StatusSummary, snapshot.Changes.Count == 0 ? Green : Yellow);
    }

    private static void DrawFooter(CellBuffer buffer)
    {
        var text = "OpenTUI Git Tool · q quit · r refresh · enter inspect";
        DrawCenteredText(buffer, buffer.Height - 2, text, Muted);
    }

    private static void DrawCenteredError(CellBuffer buffer, GitRepositorySnapshot snapshot)
    {
        DrawCenteredText(buffer, buffer.Height / 2 - 1, snapshot.RepositoryName, Accent, TextAttributes.Bold);
        DrawCenteredText(buffer, buffer.Height / 2 + 1, snapshot.Error ?? "Unknown git error", Red);
    }

    private static Rgba StatusColor(string status)
    {
        if (status.Contains('D')) return Red;
        if (status.Contains('?')) return Yellow;
        if (status.Contains('M')) return Accent;
        if (status.Contains('A')) return Green;
        return Foreground;
    }

    private static void DrawCenteredText(CellBuffer buffer, int y, string text, Rgba fg, TextAttributes attributes = TextAttributes.None)
    {
        var x = Math.Max(0, (buffer.Width - text.Length) / 2);
        DrawText(buffer, x, y, text, fg, attributes);
    }

    private static void DrawText(CellBuffer buffer, int x, int y, string text, Rgba fg, TextAttributes attributes = TextAttributes.None, int? maxWidth = null)
    {
        if (y < 0 || y >= buffer.Height || x >= buffer.Width) return;
        var available = Math.Min(maxWidth ?? buffer.Width - x, buffer.Width - x);
        if (available <= 0) return;
        buffer.DrawText(Truncate(text, available), Math.Max(0, x), y, fg, Background, attributes);
    }

    private static string Truncate(string text, int maxWidth)
    {
        if (maxWidth <= 0) return string.Empty;
        if (text.Length <= maxWidth) return text;
        if (maxWidth == 1) return "…";
        return text[..(maxWidth - 1)] + "…";
    }
}

internal static class TerminalBufferWriter
{
    public static void Write(CellBuffer buffer)
    {
        var currentFg = Rgba.FromInts(255, 255, 255);
        var currentBg = Rgba.FromInts(0, 0, 0);
        var currentAttributes = TextAttributes.None;

        for (var y = 0; y < buffer.Height; y++)
        {
            for (var x = 0; x < buffer.Width; x++)
            {
                var cell = buffer.GetCell(x, y);
                if (cell == null || cell.Value.Codepoint == 0)
                {
                    Console.Write(' ');
                    continue;
                }

                var value = cell.Value;
                if (value.Attributes != currentAttributes)
                {
                    Console.Write(AnsiCodes.Reset);
                    if (value.Attributes != TextAttributes.None)
                    {
                        AnsiCodes.WriteAttributes(Console.Out, value.Attributes);
                    }
                    currentAttributes = value.Attributes;
                    currentFg = Rgba.FromInts(255, 255, 255);
                    currentBg = Rgba.FromInts(0, 0, 0);
                }

                if (value.Fg != currentFg)
                {
                    Console.Write(AnsiCodes.FgColor(value.Fg.RedByte, value.Fg.GreenByte, value.Fg.BlueByte));
                    currentFg = value.Fg;
                }

                if (value.Bg != currentBg)
                {
                    Console.Write(AnsiCodes.BgColor(value.Bg.RedByte, value.Bg.GreenByte, value.Bg.BlueByte));
                    currentBg = value.Bg;
                }

                Console.Write(char.ConvertFromUtf32(value.Codepoint));
            }

            Console.WriteLine(AnsiCodes.Reset);
            currentFg = Rgba.FromInts(255, 255, 255);
            currentBg = Rgba.FromInts(0, 0, 0);
            currentAttributes = TextAttributes.None;
        }
    }
}
