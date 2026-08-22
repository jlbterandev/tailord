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

## Monitoring and alerts

Tailord is also intended to support unattended monitoring when continuously
watching logs is more useful than keeping the desktop application visible.
Planned capabilities include:

- Visual alerts in the desktop application when a configured rule matches.
- Headless background monitoring of one or more log files.
- Alert rules that can reuse classification and pattern matching without being
  coupled to visibility filters or highlighting rules.
- Notification channels such as email, with explicit handling for repeated
  events, delivery failures, and sensitive configuration.
- Clear operational status when a monitored file disappears, rotates, or
  cannot be read.

Running unattended must remain cross-platform. Cron or systemd may be suitable
integration points on Linux, while launchd on macOS and Task Scheduler or a
Windows Service may serve the same purpose on their platforms. The exact host
and notification design will be selected and tested in a later stage; it is not
part of the core reader or the current CLI behavior.

## Settings experience

The desktop application will provide a settings window for user preferences
such as theme, log font family and size, optional level and exception colors,
the maximum number of visible lines, and session restoration.

Update behavior has two separate concerns and must not be represented as a
single ambiguous value:

- The file polling interval controls how often the core checks for appended
  content when following a log.
- The visual update interval controls how often accumulated lines are delivered
  to the desktop UI.

Safe defaults should work without configuration. Exact timing and batching
values may be exposed as advanced settings or through understandable
responsiveness presets after performance testing.

## Persistence

Future per-user configuration will use the standard application-data location
for each operating system. Settings, reusable filters, session state, and
workspaces will be stored separately using versioned JSON and atomic writes.

Tailord may remember open paths, tab order, filters, colors, and visual state.
It will not persist log contents, credentials, large line caches, or a paused
state as the permanent startup default.

Restoring the previous session will be optional. When enabled, Tailord will
reopen saved tabs and resume following rather than restoring them as paused. A
missing file will keep its tab and show a clear `File not found` state. User
configuration and session files remain local application data and must never be
committed to the Tailord repository.

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
