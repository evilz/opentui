# OpenTUI Git Tool

A standalone sample project that renders a Lazygit-inspired Git dashboard with OpenTUI. It keeps the black macOS-style chrome from the reference image, then adds practical panes for changed files, recent commits, and repository details.

## Run

```bash
cd csharp
dotnet run --project samples/OpenTui.GitTool -- /path/to/repository
```

If no repository path is provided, the tool opens the current working directory.

## What it shows

- Current repository, branch, upstream, and working tree summary.
- Changed files from `git status --short`.
- Recent commits from `git log`.
- A footer with future keyboard actions for an interactive version.
