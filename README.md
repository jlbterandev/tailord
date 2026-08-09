# Tailord

> A fast, cross-platform log viewer tailored to your workflow.

Tailord is an open-source log viewer with a reusable processing engine, a
command-line interface, and a desktop application for macOS, Windows, and
Linux.

## Status

Tailord is in its first development stage. The repository currently contains
the buildable application skeleton; log following and filtering are the next
milestones.

## Projects

| Project | Purpose |
| --- | --- |
| `Tailord.Core` | Platform-independent log processing engine |
| `Tailord.Cli` | Terminal and pipeline interface |
| `Tailord.Desktop` | Avalonia desktop interface |
| `Tailord.Core.Tests` | Unit tests for the processing engine |

## Requirements

- .NET 10 SDK
- macOS, Windows, or Linux for the desktop application

## Build and run

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/Tailord.Cli -- --help
dotnet run --project src/Tailord.Desktop
```

The repository uses `global.json` to require .NET 10 while allowing newer .NET
10 feature bands.

## Roadmap

1. Define log entries, levels, and filter rules in `Tailord.Core`.
2. Follow large files incrementally, including truncation and rotation.
3. Expose the engine through the CLI.
4. Display a live, virtualized log in the desktop application.
5. Add tabs, global and local filters, token highlighting, and workspaces.

## License

[MIT](LICENSE)

