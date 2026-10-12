#!/usr/bin/env python3
"""Mirror package READMEs that already are complete guides into docs/<PackageId>/README.md.

Relative links are rewritten to absolute GitHub URLs so the mirrored page and the NuGet README
render the same everywhere. Run via scripts/generate-docs.sh.
"""
import os
import re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
BLOB = "https://github.com/bodenberg/appdimens-net-binding/blob/main"

MIRRORS = {
    "Bodenberg.AppDimens.Sdps": "appdimens-sdps-net-binding/README.md",
    "Bodenberg.AppDimens.Ssps": "appdimens-ssps-net-binding/README.md",
    "Bodenberg.AppDimens.Dynamic": "appdimens-dynamic-net-binding/README.md",
    "Bodenberg.AppDimens.Maui.Sdps": "appdimens-sdps-net/README.md",
    "Bodenberg.AppDimens.Sdps.Wpf": "appdimens-sdps-net/src/AppDimens.Sdps.Wpf/README.md",
    "Bodenberg.AppDimens.Sdps.WinForms": "appdimens-sdps-net/src/AppDimens.Sdps.WinForms/README.md",
    "Bodenberg.AppDimens.Sdps.WinUI": "appdimens-sdps-net/src/AppDimens.Sdps.WinUI/README.md",
}


def absolutize(text: str, src_dir: str) -> str:
    def fix(m):
        label, target = m.group(1), m.group(2)
        if re.match(r"^(https?:|#|mailto:)", target):
            return m.group(0)
        path = os.path.normpath(os.path.join(src_dir, target))
        rel = os.path.relpath(path, ROOT).replace(os.sep, "/")
        # links into docs/<pkg>/ stay relative to the mirrored page
        if rel.startswith("docs/"):
            return f"[{label}](../{rel[len('docs/'):]})"
        return f"[{label}]({BLOB}/{rel})"
    return re.sub(r"\[([^\]]*)\]\(([^)\s]+)\)", fix, text)


for pkg, src in MIRRORS.items():
    src_path = os.path.join(ROOT, src)
    text = open(src_path, encoding="utf-8").read()
    text = re.sub(r"^📖 \*\*Documentation:\*\*.*\n\n?", "", text, flags=re.M)   # self-link on the mirror
    text = absolutize(text, os.path.dirname(src_path))
    note = (f"> 📄 This guide is the package README of **{pkg}** (mirrored from [`{src}`]({BLOB}/{src}) by "
            f"`scripts/generate-docs.sh`). Complete API reference: [API.md](API.md) · [All packages](../README.md)\n\n")
    lines = text.split("\n", 1)
    out = lines[0] + "\n\n" + note + (lines[1].lstrip("\n") if len(lines) > 1 else "")
    dest = os.path.join(ROOT, "docs", pkg, "README.md")
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    with open(dest, "w", encoding="utf-8") as f:
        f.write(out)
    print(f"docs/{pkg}/README.md <- {src}")
