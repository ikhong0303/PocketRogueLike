---
name: unity-cli
description: Run Unity CLI commands to install Unity editors, manage modules, inspect project info, and trigger builds or editor operations from the command line.
---

# Unity CLI Skill

Use this skill when interacting with Unity projects or editor installations via the new standalone `unity` CLI binary (v1.0.0-beta.2+).

## Key Commands

### Environment Check & Path
- Path: `%LOCALAPPDATA%\Unity\bin\unity.exe`
- Version: `unity --version`
- Help: `unity --help`

### Editors Management
- List installed: `unity editors -i`
- List releases: `unity editors -r`
- Install Editor: `unity install <version> [-m module1 module2]`
- Install Modules: `unity install-modules -e <version> -m <module>`

### Projects & Operations
- List Projects: `unity projects list`
- Inspect Project Info: `unity projects info <path_or_name>`
- Open Project: `unity open <path>`
- Batch Run / Build / Test: `unity build <project>`, `unity test <project>`

### Output Customization
- Use `--format json` for structured JSON output in automation scripts.
