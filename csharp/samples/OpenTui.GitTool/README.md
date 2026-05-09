# OpenTUI Git Tool

A standalone sample project that renders a Lazygit-inspired Git dashboard with OpenTUI. It keeps the black macOS-style chrome from the reference image, then adds commands for day-to-day Git workflows.

## Run the dashboard

```bash
cd csharp
dotnet run --project samples/OpenTui.GitTool -- /path/to/repository
```

If no repository path is provided, the tool opens the current working directory.

## Run Git actions

```bash
cd csharp
dotnet run --project samples/OpenTui.GitTool -- --repo /path/to/repository status
dotnet run --project samples/OpenTui.GitTool -- --repo /path/to/repository stage src/App.cs
dotnet run --project samples/OpenTui.GitTool -- --repo /path/to/repository commit "add dashboard"
dotnet run --project samples/OpenTui.GitTool -- --repo /path/to/repository pr-open
```

Use `--help` to print every command.

## Supported workflows

- **Files & staging:** status, stage/unstage, patch staging for hunks and lines, discard, editor, difftool, flat/tree file views.
- **Commit workflow:** commit, amend, interactive reword/drop/move via rebase, squash/fixup commits, tags, revert, reset soft/mixed/hard, cherry-pick.
- **Branches:** checkout, create, rename, delete, merge, rebase, fast-forward, upstream configuration, GitHub PR create/open through `gh`.
- **Rebase / merge support:** continue, abort, skip, conflict ours/theirs, undo conflict resolution, edit conflicted files.
- **Stash:** create, apply, pop, drop, rename entries, create branches from stash entries.
- **Undo / redo:** reflog-based reset helpers for Git history operations.
- **Custom commands:** configure commands in JSON and run them by name.
- **Configuration:** global, parent-directory, repository-specific JSON configuration with schema support for VS Code IntelliSense.

## Configuration

Configuration is merged in this order:

1. `~/.config/opentui-git/config.json`
2. `.opentui-git.json` files from parent directories down to the repository
3. `.git/opentui-git.json`

Example:

```json
{
  "$schema": "./opentui-git.schema.json",
  "fileView": "tree",
  "customCommands": {
    "graph": {
      "command": "git",
      "arguments": ["log", "--oneline", "--graph", "--decorate", "--all"]
    }
  }
}
```
