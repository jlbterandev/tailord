# Tailord

> A fast, cross-platform log viewer tailored to your workflow.

Tailord is an open-source log viewer with a reusable processing engine, a
command-line interface, and a desktop application for macOS, Windows, and
Linux.

## Status

Tailord is in active development. Its core can read and follow logs while
handling partial lines, truncation, and file rotation. The CLI can print and
follow a single log file. CLI filtering and the live desktop viewer are the
next milestones.

Future releases are intended to add rule-based visual alerts, unattended
background monitoring, and notification channels such as email. Operating
system integration may include cron or systemd on Linux, launchd on macOS, and
Task Scheduler or a Windows Service on Windows. These capabilities are planned,
not implemented yet.

## Monitoring approach

Tailord is intended to provide a calm, continuous view of system activity, not
another source of notification noise. Its planned monitoring workflow will
make it possible to:

- Observe several independent log sources from one workspace.
- Combine saved filters and severity selections into focused operational views.
- See line rates, severity counts, and the state of each source at a glance.
- Identify logs that are silent, missing, disconnected, truncated, or rotated.
- Review a daily summary with counts, first and last occurrences, and recurring
  matches without storing copies of the original logs.
- Move from a summary or counter directly to the relevant raw log lines.

Visual indicators and summaries will be the default feedback. External
notifications will be optional and based on explicit rules, with grouping,
deduplication, and cooldown periods to avoid repeated alerts for the same
condition.

## Documentation

- [Product vision and requirements](docs/PRODUCT.md)
- [Development roadmap and current progress](docs/ROADMAP.md)
- [Architecture and dependency rules](docs/architecture.md)
- [Collaboration guidelines](AGENTS.md)

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
dotnet run --project src/Tailord.Cli -- sample.log
dotnet run --project src/Tailord.Cli -- sample.log --follow
dotnet run --project src/Tailord.Cli -- sample.log --follow --from-end
dotnet run --project src/Tailord.Cli -- sample.log --level warning,error
dotnet run --project src/Tailord.Cli -- sample.log --include "timeout"
dotnet run --project src/Tailord.Cli -- sample.log --exclude "healthcheck"
dotnet run --project src/Tailord.Desktop
```

Press `Ctrl+C` to stop a CLI session running with `--follow`.
Add `--from-end` to ignore existing content and display only lines appended
after monitoring starts.
Use `--level` with one or more comma-separated values: `unknown`, `debug`,
`information`, `warning`, `error`, or `critical`. Level names are
case-insensitive.
Use `--include` to show only lines containing the supplied text. Matching is
case-insensitive and can be combined with `--level` and `--follow`.
Use `--exclude` to hide matching lines. Exclusions are evaluated after level
and inclusion filters, so an excluded line always remains hidden.

The repository uses `global.json` to require .NET 10 while allowing newer .NET
10 feature bands.

## Development direction

Development proceeds through small, tested increments. See the
[roadmap](docs/ROADMAP.md) for completed work, the current milestone, and
planned capabilities.

## License

[MIT](LICENSE)
