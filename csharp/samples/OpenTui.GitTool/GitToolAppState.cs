namespace OpenTui.GitTool;

public enum GitToolScreen
{
    Files,
    Commits,
    Branches,
    CustomPatch,
    Help,
}

public enum GitToolScreenMode
{
    Split,
    Enlarged,
    Graph,
}

public sealed class GitToolAppState
{
    public GitToolAppState(GitRepositorySnapshot snapshot, GitToolConfig config)
    {
        Snapshot = snapshot;
        Config = config;
        Branches = BuildBranches(snapshot).ToArray();
        DiffLines = BuildDiffLines(snapshot).ToArray();
        CustomPatchLines = [];
    }

    public GitRepositorySnapshot Snapshot { get; }
    public GitToolConfig Config { get; }
    public GitToolScreen Screen { get; set; } = GitToolScreen.Files;
    public GitToolScreenMode ScreenMode { get; set; } = GitToolScreenMode.Split;
    public int SelectedFileIndex { get; set; }
    public int SelectedDiffLineIndex { get; set; }
    public int SelectedCommitIndex { get; set; }
    public int SelectedBranchIndex { get; set; }
    public bool SelectingLineRange { get; set; }
    public bool InteractiveRebase { get; set; }
    public bool DiffMode { get; set; }
    public bool Filtering { get; set; }
    public string Filter { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = "Ready";
    public string? CopiedCommit { get; set; }
    public string? MarkedBaseCommit { get; set; }
    public string? CompareFromCommit { get; set; }
    public IReadOnlyList<string> Branches { get; }
    public IReadOnlyList<string> DiffLines { get; }
    public IReadOnlyList<string> CustomPatchLines { get; private set; }

    public FileChange? SelectedFile => Snapshot.Changes.Count == 0 ? null : Snapshot.Changes[Math.Clamp(SelectedFileIndex, 0, Snapshot.Changes.Count - 1)];

    public CommitEntry? SelectedCommit => Snapshot.Commits.Count == 0 ? null : Snapshot.Commits[Math.Clamp(SelectedCommitIndex, 0, Snapshot.Commits.Count - 1)];

    public string? SelectedBranch => Branches.Count == 0 ? null : Branches[Math.Clamp(SelectedBranchIndex, 0, Branches.Count - 1)];

    public void MoveSelection(int delta)
    {
        switch (Screen)
        {
            case GitToolScreen.Files:
                SelectedFileIndex = Move(SelectedFileIndex, delta, Math.Max(1, Snapshot.Changes.Count));
                SelectedDiffLineIndex = Move(SelectedDiffLineIndex, delta, Math.Max(1, DiffLines.Count));
                break;
            case GitToolScreen.Commits:
                SelectedCommitIndex = Move(SelectedCommitIndex, delta, Math.Max(1, Snapshot.Commits.Count));
                break;
            case GitToolScreen.Branches:
                SelectedBranchIndex = Move(SelectedBranchIndex, delta, Math.Max(1, Branches.Count));
                break;
        }
    }

    public void CycleScreen(int delta)
    {
        var screens = Enum.GetValues<GitToolScreen>();
        var next = ((int)Screen + delta + screens.Length) % screens.Length;
        Screen = screens[next];
        Filtering = false;
    }

    public void CycleScreenMode(int delta)
    {
        var modes = Enum.GetValues<GitToolScreenMode>();
        var next = ((int)ScreenMode + delta + modes.Length) % modes.Length;
        ScreenMode = modes[next];
    }

    public void AddSelectedLineToPatch()
    {
        if (DiffLines.Count == 0) return;
        var line = DiffLines[Math.Clamp(SelectedDiffLineIndex, 0, DiffLines.Count - 1)];
        CustomPatchLines = [..CustomPatchLines, line];
        StatusMessage = $"Added line {SelectedDiffLineIndex + 1} to custom patch";
    }

    private static int Move(int current, int delta, int count) => Math.Clamp(current + delta, 0, count - 1);

    private static IEnumerable<string> BuildBranches(GitRepositorySnapshot snapshot)
    {
        yield return $"* {snapshot.Branch}";
        if (snapshot.Upstream != null) yield return $"  {snapshot.Upstream}";
        yield return "  feature/demo  GH:open";
        yield return "  docs_fix      GH:merged";
        yield return "  master";
    }

    private static IEnumerable<string> BuildDiffLines(GitRepositorySnapshot snapshot)
    {
        var file = snapshot.Changes.FirstOrDefault()?.Path ?? "docs/README.md";
        yield return $"diff --git a/{file} b/{file}";
        yield return $"index 32d7787..6b7fdc7 100644";
        yield return $"--- a/{file}";
        yield return $"+++ b/{file}";
        yield return "@@ -9,3 +9,6 @@ Simple terminal UI for git";
        yield return " ### Homebrew";
        yield return "+Just do brew install opentui-git and bada bing bada boom";
        yield return "+you have begun on the path of laziness.";
        yield return "+";
        yield return "-Simple terminal UI for git commands";
        yield return "+Simple terminal UI for git";
        yield return "+(Not too simple though)";
    }
}
