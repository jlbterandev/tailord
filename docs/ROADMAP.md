# Tailord Roadmap

Development proceeds through small increments. Each increment should explain
its purpose, identify affected files, implement only the agreed scope, and pass
`dotnet build` and `dotnet test` before moving forward.

## 0. Buildable skeleton

- Create `Tailord.Core`, `Tailord.Cli`, `Tailord.Desktop`, and
  `Tailord.Core.Tests`.
- Establish shared .NET, package, editor, and VS Code configuration.
- Provide a minimal CLI and Avalonia application shell.

Status: complete.

## 1. Project documentation and source control

- Document the product, architecture, roadmap, and collaboration rules.
- Initialize Git and establish a clean published baseline.

Status: in progress.

## 2. Minimal domain model

Implement and test these as separate increments:

1. `LogLevel`, including explicit decisions about `Exception` and `Unknown`.
2. A small immutable `LogEntry` containing only currently required data.
3. Basic case-insensitive level classification, including unclassified lines.

Exit criterion: log text can become classified entries without UI dependencies.

## 3. Filtering engine

Add simple text rules, case sensitivity, inclusion and exclusion, `ANY` and
`ALL` composition, level filtering, and safe regular-expression handling. Keep
highlighting rules independent from visibility filters.

Exit criterion: collections of log entries can be evaluated predictably by
reusable rules.

## 4. Incremental file reader

Read existing content incrementally, emit complete lines, retain partial lines,
follow appended content, support pause and cancellation, and handle truncation,
replacement, and rotation. Choose polling and file-system notification behavior
only after tests establish the required reliability.

Exit criterion: the core can follow a real log continuously with bounded memory.

## 5. Functional CLI

Open and print a file, follow changes, filter by level, include or exclude text,
and stop cleanly on `Ctrl+C`. Add stdin and regular-expression support in later
small increments.

Exit criterion: a command such as
`tailord app.log --follow --level warning,error` uses only `Tailord.Core`.

## 6. Functional desktop viewer

Connect the existing Avalonia shell through MVVM, select a file, display and
follow lines, pause or resume, clear only the view, follow scrolling, and show
status and counters. Bound or virtualize displayed lines and batch UI updates.

Exit criterion: the desktop application can reliably observe one real file.

## 7. Multiple files

Add independent tabs, closing and reordering, active-tab state, missing-file
handling, and correct disposal of readers and cancellation resources.

## 8. Filters and highlighting UI

Expose global and per-tab filters, levels, inclusion and exclusion, composition,
regular expressions, color rules, and visible counters.

## 9. Persistence

Add platform-correct paths, schema-versioned JSON, atomic writes, workspaces,
session restoration, missing-file behavior, and recovery from invalid data.

## 10. Hardening and release

Test large files, long sessions, bounded memory, filter performance, application
errors, accessibility, and packaging. Prepare installation and usage docs before
the first authorized GitHub release.
