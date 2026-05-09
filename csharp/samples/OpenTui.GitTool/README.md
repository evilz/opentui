# OpenTUI Git Tool

A standalone Razor sample project that renders a Lazygit-inspired multi-screen Git dashboard with OpenTUI components. It keeps the black terminal chrome from the reference images, adds files/commits/branches/custom-patch/help screens, and keeps command-mode actions for day-to-day Git workflows.

## Run the dashboard

```bash
cd csharp
dotnet run --project samples/OpenTui.GitTool -- /path/to/repository
```

If no repository path is provided, the tool opens the current working directory. The dashboard uses Razor components; press `tab` to cycle files, commits, branches, custom patch, and help screens. Press `+` or `_` to cycle split, enlarged, and graph screen modes.

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

- **Files & staging:** status, stage/unstage, patch staging for hunks and lines, discard, editor, difftool, flat/tree file views. In the Razor dashboard, `space` stages the selected line, `v` starts a range selection, `a` stages the hunk, and `shift+d` opens the reset/nuke workflow.
- **Commit workflow:** commit, amend, interactive reword/drop/move via rebase, squash/fixup commits, tags, revert, reset soft/mixed/hard, cherry-pick. In the commits screen, `i` starts rebase mode, `s`/`f`/`d`/`e` mark TODO actions, `ctrl+k`/`ctrl+j` reorder, `m` opens rebase options, `shift+a` amends an old commit, `shift+c` copies, and `shift+v` cherry-picks.
- **Branches:** checkout, create, rename, delete, merge, rebase, fast-forward, upstream configuration, GitHub PR create/open through `gh`. In the branches screen, `/` filters, `enter` views commits, `w` creates a worktree, `shift+b` marks a base commit, `r` rebases from it, `shift+p` pushes, and `shift+g` opens a GitHub PR.
- **Rebase / merge support:** continue, abort, skip, conflict ours/theirs, undo conflict resolution, edit conflicted files.
- **Stash:** create, apply, pop, drop, rename entries, create branches from stash entries.
- **Undo / redo:** reflog-based reset helpers for Git history operations, surfaced as `z` and `shift+z` in the dashboard.
- **Compare and graph:** `shift+w` marks a commit/ref for compare mode; `+` and `_` cycle to the enlarged commit graph view with colored author lanes.
- **Custom patches:** the custom-patch screen lets you add lines with `space` and open patch options with `ctrl+p` for rebase-magic style patch removal/splitting/reverse apply flows.
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
