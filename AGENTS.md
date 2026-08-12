# Collaboration Guidelines

- Work in small, explicitly agreed increments.
- Before editing, explain the behavior being added and list affected files.
- Implement only the current increment; describe future work without starting it.
- Run `dotnet build` and `dotnet test` after every code or documentation change.
- Explain important modern .NET, Avalonia, and cross-platform decisions clearly.
- Keep `Tailord.Core` independent of Avalonia, console APIs, and UI concerns.
- Prefer readable code, immutable domain types where useful, and behavior-focused
  tests over speculative abstractions.
- Do not add packages, projects, patterns, or persistence mechanisms without an
  immediate explained need.
- Preserve bounded-memory, asynchronous, cancellable processing requirements.
- Do not include proprietary names, code, configuration, credentials, or data.
- Do not commit, push, publish, or release unless explicitly authorized.
- Preserve unrelated local changes and never use destructive Git commands without
  explicit approval.

Read `README.md`, `docs/PRODUCT.md`, `docs/ROADMAP.md`, and
`docs/architecture.md` before planning a substantial change.
