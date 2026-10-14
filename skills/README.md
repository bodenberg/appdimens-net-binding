# AppDimens .NET — agent skills

One [Agent Skill](https://docs.claude.com/en/docs/agents-and-tools/agent-skills/overview) per library set. Each
`SKILL.md` teaches an AI coding agent (Claude Code, etc.) how to **use** that set in an app: which package to
install, the one-time setup call, the functions and markup, and the common mistakes.

| Skill | Library set | Packages |
|-------|-------------|----------|
| [`appdimens-sdps-net`](appdimens-sdps-net/SKILL.md) | SDPS native (Android SDP parity) for MAUI, WPF, Windows Forms, WinUI 3 | `Bodenberg.AppDimens.Maui.Sdps`, `Bodenberg.AppDimens.Sdps.Wpf` / `.WinForms` / `.WinUI` |
| [`appdimens-net`](appdimens-net/SKILL.md) | AppDimens.Net — C# port of appdimens-kmp (core, 13 strategies, UI integrations) | `AppDimens.Net`, `AppDimens.Net.Sdk`, `AppDimens.Net.<Strategy>`, `AppDimens.Net.Maui` / `.Wpf` / `.WinForms` / `.WinUI` |
| [`appdimens-android-sdps-ssps`](appdimens-android-sdps-ssps/SKILL.md) | Android bindings with XML `@dimen` resources | `Bodenberg.AppDimens.Sdps`, `Bodenberg.AppDimens.Ssps` |
| [`appdimens-android-dynamic`](appdimens-android-dynamic/SKILL.md) | Android binding, code-only strategies | `Bodenberg.AppDimens.Dynamic` |

## Installing a skill

Copy the folder of the skill you need into your app repository (project skills) or your user skills folder:

```bash
# project-level (shared with your team through git)
mkdir -p .claude/skills
cp -r path/to/appdimens-net-binding/skills/appdimens-sdps-net .claude/skills/

# or user-level (every project on this machine)
cp -r path/to/appdimens-net-binding/skills/appdimens-net ~/.claude/skills/
```

The agent loads the skill automatically when the task matches its description (e.g. "make this WPF window
responsive with AppDimens").

Maintaining this repository (build, pack, AAR sync, release) is documented in [MAINTAINING.md](../MAINTAINING.md).

## Português

Uma skill por conjunto de bibliotecas: **SDPS nativo** (MAUI/WPF/WinForms/WinUI), **AppDimens.Net** (port KMP com
estratégias), **bindings Android Sdps/Ssps** (`@dimen/_16sdp`) e **binding Android Dynamic** (estratégias só em código).
Copie a pasta da skill para `.claude/skills/` do seu projeto (ou `~/.claude/skills/`) e o agente passa a saber instalar,
inicializar e usar a biblioteca corretamente. A manutenção do repositório está em [MAINTAINING.md](../MAINTAINING.md).
