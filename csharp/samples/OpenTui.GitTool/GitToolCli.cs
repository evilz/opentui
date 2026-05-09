using System.Diagnostics;

namespace OpenTui.GitTool;

internal sealed record GitToolCliOptions(string RepositoryPath, string? CommandName, IReadOnlyList<string> CommandArguments, bool ShowHelp)
{
    public static GitToolCliOptions Parse(string[] args)
    {
        var repositoryPath = Directory.GetCurrentDirectory();
        var commandArgs = new List<string>();
        var commandName = default(string);
        var showHelp = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (arg is "--help" or "-h")
            {
                showHelp = true;
                continue;
            }

            if (arg is "--repo" or "-C")
            {
                if (i + 1 >= args.Length) throw new ArgumentException("--repo requires a path");
                repositoryPath = Path.GetFullPath(args[++i]);
                continue;
            }

            if (commandName == null && GitActionCatalog.IsCommand(arg))
            {
                commandName = arg;
                continue;
            }

            if (commandName == null && args.Length == 1)
            {
                repositoryPath = Path.GetFullPath(arg);
                continue;
            }

            if (commandName == null)
            {
                throw new ArgumentException($"Unknown command: {arg}");
            }

            commandArgs.Add(arg);
        }

        return new GitToolCliOptions(repositoryPath, commandName, commandArgs, showHelp);
    }
}

internal sealed record GitAction(string Command, string Category, string Description, string Usage);

internal static class GitActionCatalog
{
    public static readonly IReadOnlyList<GitAction> Actions =
    [
        new("status", "Files & staging", "View changed files", "status"),
        new("stage", "Files & staging", "Stage files", "stage <path> [path...]"),
        new("unstage", "Files & staging", "Unstage files", "unstage <path> [path...]"),
        new("stage-hunks", "Files & staging", "Stage individual hunks or lines", "stage-hunks <path>"),
        new("unstage-hunks", "Files & staging", "Unstage individual hunks or lines", "unstage-hunks <path>"),
        new("discard", "Files & staging", "Discard file changes", "discard <path> [path...]"),
        new("discard-hunks", "Files & staging", "Discard individual hunks or lines", "discard-hunks <path>"),
        new("edit", "Files & staging", "Open files in $VISUAL/$EDITOR", "edit <path>"),
        new("difftool", "Files & staging", "Open external difftool", "difftool [path]"),
        new("file-view", "Files & staging", "Switch between flat/tree file view", "file-view <flat|tree>"),
        new("nuke", "Files & staging", "Remove everything shown by git status, including dirty submodules", "nuke"),
        new("commit", "Commit workflow", "Create a commit", "commit <message>"),
        new("amend", "Commit workflow", "Amend the previous commit", "amend [message]"),
        new("reword", "Commit workflow", "Reword a commit with interactive rebase", "reword <commit>"),
        new("squash", "Commit workflow", "Create a squash commit", "squash <commit>"),
        new("fixup", "Commit workflow", "Create a fixup commit", "fixup <commit>"),
        new("drop", "Commit workflow", "Drop commits with interactive rebase", "drop <commit>"),
        new("tag", "Commit workflow", "Create tags", "tag <name> [commit]"),
        new("revert", "Commit workflow", "Revert commits", "revert <commit>"),
        new("reset-soft", "Commit workflow", "Soft reset", "reset-soft <target>"),
        new("reset-mixed", "Commit workflow", "Mixed reset", "reset-mixed <target>"),
        new("reset-hard", "Commit workflow", "Hard reset", "reset-hard <target>"),
        new("cherry-pick", "Commit workflow", "Cherry-pick commits", "cherry-pick <commit>"),
        new("bisect-good", "Commit workflow", "Mark a commit as good for git bisect", "bisect-good <commit>"),
        new("bisect-bad", "Commit workflow", "Mark a commit as bad for git bisect", "bisect-bad <commit>"),
        new("bisect-reset", "Commit workflow", "Reset git bisect", "bisect-reset"),
        new("compare", "Commit workflow", "Compare two commits", "compare <left> <right>"),
        new("move-commits", "Commit workflow", "Move commits up/down during rebase", "move-commits <base>"),
        new("branch-checkout", "Branches", "Checkout branches", "branch-checkout <branch>"),
        new("branch-create", "Branches", "Create branches", "branch-create <branch> [start-point]"),
        new("branch-rename", "Branches", "Rename branches", "branch-rename <old> <new>"),
        new("branch-delete", "Branches", "Delete branches", "branch-delete <branch>"),
        new("merge", "Branches", "Merge branches", "merge <branch>"),
        new("rebase", "Branches", "Rebase branches", "rebase <branch>"),
        new("fast-forward", "Branches", "Fast-forward only merge", "fast-forward <branch>"),
        new("upstream", "Branches", "Configure upstream", "upstream <remote>/<branch>"),
        new("pr-create", "Branches", "Create GitHub pull requests with gh", "pr-create [gh args...]"),
        new("pr-open", "Branches", "Open GitHub pull requests in browser", "pr-open [branch|number]"),
        new("worktree-create", "Branches", "Create a worktree from a branch", "worktree-create <path> <branch>"),
        new("continue", "Rebase / merge support", "Continue merge/rebase operations", "continue"),
        new("abort", "Rebase / merge support", "Abort merge/rebase operations", "abort"),
        new("skip", "Rebase / merge support", "Skip merge/rebase operations", "skip"),
        new("conflict-ours", "Rebase / merge support", "Resolve conflicts by picking ours", "conflict-ours <path>"),
        new("conflict-theirs", "Rebase / merge support", "Resolve conflicts by picking theirs", "conflict-theirs <path>"),
        new("conflict-undo", "Rebase / merge support", "Undo conflict resolution", "conflict-undo <path>"),
        new("stash", "Stash", "Create stash", "stash [message]"),
        new("stash-apply", "Stash", "Apply stash", "stash-apply [stash]"),
        new("stash-pop", "Stash", "Pop stash", "stash-pop [stash]"),
        new("stash-drop", "Stash", "Drop stash", "stash-drop [stash]"),
        new("stash-rename", "Stash", "Rename stash entries", "stash-rename <stash> <message>"),
        new("stash-branch", "Stash", "Create branch from stash", "stash-branch <branch> [stash]"),
        new("undo", "Undo / redo", "Undo the previous Git history operation via reflog", "undo"),
        new("redo", "Undo / redo", "Redo using a reflog target", "redo <reflog-target>"),
        new("custom", "Custom commands", "Run configured custom commands", "custom <name> [args...]"),
        new("config-show", "Configuration", "Show merged configuration", "config-show"),
        new("schema", "Configuration", "Print JSON schema path", "schema"),
    ];

    private static readonly HashSet<string> CommandNames = Actions.Select(action => action.Command).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsCommand(string command) => CommandNames.Contains(command);
}

internal static class GitToolHelp
{
    public static void Write(TextWriter writer)
    {
        writer.WriteLine("OpenTUI Git Tool");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  dotnet run --project samples/OpenTui.GitTool -- [--repo <path>] [command] [args]");
        writer.WriteLine("  dotnet run --project samples/OpenTui.GitTool -- /path/to/repo");
        writer.WriteLine();
        writer.WriteLine("Commands:");

        foreach (var group in GitActionCatalog.Actions.GroupBy(action => action.Category))
        {
            writer.WriteLine($"  {group.Key}");
            foreach (var action in group)
            {
                writer.WriteLine($"    {action.Usage.PadRight(34)} {action.Description}");
            }
        }
    }
}

internal static class GitActionExecutor
{
    public static int Execute(GitToolCliOptions options)
    {
        var snapshot = GitRepositorySnapshot.Load(options.RepositoryPath);
        if (snapshot.Error != null)
        {
            Console.Error.WriteLine(snapshot.Error);
            return 1;
        }

        var config = GitToolConfig.Load(snapshot.RepositoryPath);
        var repo = snapshot.RepositoryPath;
        var command = options.CommandName ?? "status";
        var args = options.CommandArguments.ToArray();

        return command switch
        {
            "status" => RunGit(repo, "status", "--short"),
            "stage" => RunGit(repo, PrependPathspec(["add"], args, command)),
            "unstage" => RunGit(repo, PrependPathspec(["restore", "--staged"], args, command)),
            "stage-hunks" => RunGit(repo, PrependPathspec(["add", "--patch"], args, command)),
            "unstage-hunks" => RunGit(repo, PrependPathspec(["restore", "--staged", "--patch"], args, command)),
            "discard" => RunGit(repo, PrependPathspec(["restore", "--worktree"], args, command)),
            "discard-hunks" => RunGit(repo, PrependPathspec(["restore", "--worktree", "--patch"], args, command)),
            "edit" => OpenEditor(repo, args),
            "difftool" => RunGit(repo, PrependOptionalPathspec(["difftool"], args)),
            "file-view" => RenderFileView(repo, args),
            "nuke" => NukeWorkingTree(repo),
            "commit" => RunGit(repo, ["commit", "-m", RequireJoined(args, command)]),
            "amend" => args.Length == 0 ? RunGit(repo, "commit", "--amend") : RunGit(repo, "commit", "--amend", "-m", string.Join(' ', args)),
            "reword" => RunGit(repo, "rebase", "-i", RequireSingle(args, command) + "^"),
            "squash" => RunGit(repo, "commit", "--squash", RequireSingle(args, command)),
            "fixup" => RunGit(repo, "commit", "--fixup", RequireSingle(args, command)),
            "drop" => RunGit(repo, "rebase", "-i", RequireSingle(args, command) + "^"),
            "tag" => RunGit(repo, RequireRange(["tag"], args, command, 1, 2)),
            "revert" => RunGit(repo, "revert", RequireSingle(args, command)),
            "reset-soft" => RunGit(repo, "reset", "--soft", RequireSingle(args, command)),
            "reset-mixed" => RunGit(repo, "reset", "--mixed", RequireSingle(args, command)),
            "reset-hard" => RunGit(repo, "reset", "--hard", RequireSingle(args, command)),
            "cherry-pick" => RunGit(repo, "cherry-pick", RequireSingle(args, command)),
            "bisect-good" => RunGit(repo, "bisect", "good", RequireSingle(args, command)),
            "bisect-bad" => RunGit(repo, "bisect", "bad", RequireSingle(args, command)),
            "bisect-reset" => RunGit(repo, "bisect", "reset"),
            "compare" => RunGit(repo, RequireRange(["diff"], args, command, 2, 2)),
            "move-commits" => RunGit(repo, "rebase", "-i", RequireSingle(args, command)),
            "branch-checkout" => RunGit(repo, "checkout", RequireSingle(args, command)),
            "branch-create" => RunGit(repo, RequireRange(["checkout", "-b"], args, command, 1, 2)),
            "branch-rename" => RunGit(repo, RequireRange(["branch", "-m"], args, command, 2, 2)),
            "branch-delete" => RunGit(repo, "branch", "-d", RequireSingle(args, command)),
            "merge" => RunGit(repo, "merge", RequireSingle(args, command)),
            "rebase" => RunGit(repo, "rebase", RequireSingle(args, command)),
            "fast-forward" => RunGit(repo, "merge", "--ff-only", RequireSingle(args, command)),
            "upstream" => RunGit(repo, "branch", "--set-upstream-to", RequireSingle(args, command)),
            "pr-create" => RunGh(repo, ["pr", "create", ..args]),
            "pr-open" => OpenPullRequest(repo, args),
            "worktree-create" => RunGit(repo, RequireRange(["worktree", "add"], args, command, 2, 2)),
            "continue" => ContinueOperation(repo),
            "abort" => AbortOperation(repo),
            "skip" => SkipOperation(repo),
            "conflict-ours" => RunGit(repo, "checkout", "--ours", "--", RequireSingle(args, command)),
            "conflict-theirs" => RunGit(repo, "checkout", "--theirs", "--", RequireSingle(args, command)),
            "conflict-undo" => RunGit(repo, "restore", "--staged", "--worktree", "--", RequireSingle(args, command)),
            "stash" => args.Length == 0 ? RunGit(repo, "stash", "push") : RunGit(repo, "stash", "push", "-m", string.Join(' ', args)),
            "stash-apply" => RunGit(repo, OptionalSingle(["stash", "apply"], args, command)),
            "stash-pop" => RunGit(repo, OptionalSingle(["stash", "pop"], args, command)),
            "stash-drop" => RunGit(repo, OptionalSingle(["stash", "drop"], args, command)),
            "stash-rename" => RenameStash(repo, args),
            "stash-branch" => RunGit(repo, RequireRange(["stash", "branch"], args, command, 1, 2)),
            "undo" => RunGit(repo, "reset", "--hard", "HEAD@{1}"),
            "redo" => RunGit(repo, "reset", "--hard", RequireSingle(args, command)),
            "custom" => RunCustom(repo, config, args),
            "config-show" => ShowConfig(config),
            "schema" => PrintSchemaPath(),
            _ => throw new ArgumentException($"Unknown command: {command}"),
        };
    }

    private static int NukeWorkingTree(string repo)
    {
        var resetResult = RunGit(repo, "reset", "--hard");
        if (resetResult != 0) return resetResult;
        var cleanResult = RunGit(repo, "clean", "-ffd");
        if (cleanResult != 0) return cleanResult;
        return RunGit(repo, "submodule", "foreach", "--recursive", "git reset --hard && git clean -ffd");
    }

    private static int ContinueOperation(string repo)
    {
        if (IsRebaseInProgress(repo))
        {
            return RunGit(repo, "rebase", "--continue");
        }

        return RunGit(repo, "merge", "--continue");
    }

    private static int AbortOperation(string repo)
    {
        if (IsRebaseInProgress(repo))
        {
            return RunGit(repo, "rebase", "--abort");
        }

        return RunGit(repo, "merge", "--abort");
    }

    private static int SkipOperation(string repo)
    {
        if (IsRebaseInProgress(repo))
        {
            return RunGit(repo, "rebase", "--skip");
        }

        return RunGit(repo, "cherry-pick", "--skip");
    }

    private static bool IsRebaseInProgress(string repo)
    {
        return GitPathExists(repo, "rebase-merge") || GitPathExists(repo, "rebase-apply");
    }

    private static bool GitPathExists(string repo, string path)
    {
        var result = GitCommand.Run(repo, "rev-parse", "--git-path", path);
        if (!result.Success) return false;
        var resolvedPath = result.Output.Trim();
        return Directory.Exists(resolvedPath) || File.Exists(resolvedPath);
    }

    private static int OpenPullRequest(string repo, string[] args)
    {
        if (args.Length == 0) return RunGh(repo, ["pr", "view", "--web"]);
        return RunGh(repo, ["pr", "view", args[0], "--web"]);
    }

    private static int RenameStash(string repo, string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("stash-rename requires <stash> <message>");
        var stash = args[0];
        var message = string.Join(' ', args.Skip(1));
        var oidResult = GitCommand.Run(repo, "rev-parse", stash);
        if (!oidResult.Success)
        {
            Console.Error.WriteLine(oidResult.ErrorMessage);
            return 1;
        }

        var storeResult = RunGit(repo, "stash", "store", "-m", message, oidResult.Output.Trim());
        if (storeResult != 0) return storeResult;
        return RunGit(repo, "stash", "drop", stash);
    }

    private static int RunCustom(string repo, GitToolConfig config, string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("custom requires a command name");
        if (!config.CustomCommands.TryGetValue(args[0], out var customCommand))
        {
            throw new ArgumentException($"Custom command is not configured: {args[0]}");
        }

        return ProcessCommand.Run(repo, customCommand.Command, [..customCommand.Arguments, ..args.Skip(1)]);
    }

    private static int ShowConfig(GitToolConfig config)
    {
        Console.WriteLine(config.ToJson());
        return 0;
    }

    private static int PrintSchemaPath()
    {
        Console.WriteLine(Path.Combine(AppContext.BaseDirectory, "opentui-git.schema.json"));
        return 0;
    }

    private static int RenderFileView(string repo, string[] args)
    {
        var mode = args.Length == 0 ? "flat" : args[0];
        var result = GitCommand.Run(repo, "status", "--short");
        if (!result.Success)
        {
            Console.Error.WriteLine(result.ErrorMessage);
            return 1;
        }

        var paths = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Length > 3 ? line[3..] : line)
            .ToArray();

        if (mode.Equals("tree", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var group in paths.GroupBy(path => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0]))
            {
                Console.WriteLine(group.Key);
                foreach (var path in group)
                {
                    Console.WriteLine($"  {path}");
                }
            }

            return 0;
        }

        foreach (var path in paths) Console.WriteLine(path);
        return 0;
    }

    private static int OpenEditor(string repo, string[] args)
    {
        var file = RequireSingle(args, "edit");
        var editor = Environment.GetEnvironmentVariable("VISUAL") ?? Environment.GetEnvironmentVariable("EDITOR");
        if (string.IsNullOrWhiteSpace(editor)) editor = OperatingSystem.IsWindows() ? "notepad" : "vi";
        return ProcessCommand.Run(repo, editor, [file]);
    }

    private static int RunGh(string repo, IReadOnlyList<string> args) => ProcessCommand.Run(repo, "gh", args);

    private static int RunGit(string repo, params string[] args) => ProcessCommand.Run(repo, "git", args);

    private static int RunGit(string repo, IReadOnlyList<string> args) => ProcessCommand.Run(repo, "git", args);

    private static string RequireSingle(string[] args, string command)
    {
        if (args.Length != 1) throw new ArgumentException($"{command} requires exactly one argument");
        return args[0];
    }

    private static string RequireJoined(string[] args, string command)
    {
        if (args.Length == 0) throw new ArgumentException($"{command} requires a message");
        return string.Join(' ', args);
    }

    private static string[] RequireRange(IReadOnlyList<string> prefix, string[] args, string command, int min, int max)
    {
        if (args.Length < min || args.Length > max) throw new ArgumentException($"{command} expects {min}-{max} arguments");
        return [..prefix, ..args];
    }

    private static string[] OptionalSingle(IReadOnlyList<string> prefix, string[] args, string command)
    {
        if (args.Length > 1) throw new ArgumentException($"{command} expects zero or one argument");
        return [..prefix, ..args];
    }

    private static string[] PrependPathspec(IReadOnlyList<string> prefix, string[] args, string command)
    {
        if (args.Length == 0) throw new ArgumentException($"{command} requires at least one path");
        return [..prefix, "--", ..args];
    }

    private static string[] PrependOptionalPathspec(IReadOnlyList<string> prefix, string[] args)
    {
        return args.Length == 0 ? [..prefix] : [..prefix, "--", ..args];
    }
}

internal static class ProcessCommand
{
    public static int Run(string workingDirectory, string executable, IReadOnlyList<string> arguments)
    {
        using var process = new Process();
        process.StartInfo.FileName = executable;
        process.StartInfo.WorkingDirectory = workingDirectory;
        process.StartInfo.UseShellExecute = false;

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
            Console.Error.WriteLine($"Unable to start {executable}: {exception.Message}");
            return 1;
        }

        process.WaitForExit();
        return process.ExitCode;
    }
}
