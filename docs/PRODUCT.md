# Tailord Product

> A fast, cross-platform log viewer tailored to your workflow.

## Purpose

Tailord is an open-source log viewer for technical diagnosis on macOS, Windows,
and Linux. It combines a reusable processing engine, a command-line interface,
and an Avalonia desktop application.

Tailord is intended to remain useful with large and continuously changing log
files. The core must therefore process input incrementally, keep memory use
bounded, and remain independent of any user-interface framework.

## Core capabilities

Tailord will support:

- Opening and following large text log files without loading them completely.
- Reading existing content or starting at the end of a file.
- Handling partial lines, truncation, replacement, and rotation.
- Pausing, resuming, cancelling, and closing readers cleanly.
- Classifying entries and filtering by level, text, or regular expression.
- Combining inclusion rules with `ANY` or `ALL` semantics and applying
  exclusions afterwards.
- Keeping filtering and visual highlighting as separate concerns.
- Viewing several files in independent desktop tabs.
- Using the same processing behavior from the CLI and desktop application.

## Desktop experience

The desktop application will provide one tab per file, a collapsible filter
panel, a virtualized or bounded log view, and status information such as lines
read, lines visible, file path, encoding, and following state.

Clearing the view must never modify or truncate the source file. Updates from
the processing engine should be delivered to the UI in batches so that a busy
log does not saturate the UI thread.

## Command-line experience

The CLI is a real product interface and an early integration point for the core
engine. Its intended usage includes opening or following a file, reading stdin,
and applying level, inclusion, exclusion, and regular-expression filters.

## Persistence

Future per-user configuration will use the standard application-data location
for each operating system. Settings, reusable filters, session state, and
workspaces will be stored separately using versioned JSON and atomic writes.

Tailord may remember open paths, tab order, filters, colors, and visual state.
It will not persist log contents, credentials, large line caches, or a paused
state as the permanent startup default.

## Product constraints

- Target .NET 10, Avalonia 12, C#, and xUnit.
- Treat macOS and Windows as primary platforms while preserving Linux support.
- Keep `Tailord.Core` free from Avalonia, console, and operating-system UI
  dependencies.
- Prefer readable code and small verified increments over speculative
  abstractions.
- Design file reading for bounded memory and cancellation from the beginning.
- Do not add telemetry or a configuration database.
- Do not include proprietary code, names, configuration, or company data.
