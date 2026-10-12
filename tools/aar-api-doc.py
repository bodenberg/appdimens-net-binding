#!/usr/bin/env python3
"""Generate a Markdown API reference for an Android binding from its embedded AAR(s).

The public Java/Kotlin surface is read with `javap` and mapped to the C# names produced by the
.NET for Android binding generator:

* packages      com.appdimens.sdps.code   -> namespace Com.Appdimens.Sdps.Code
* methods       sdpPh(Context, int)       -> SdpPh(Context, int)
* getters       getFoo() / isFoo()        -> property Foo / IsFoo
* Kotlin object INSTANCE                  -> static property Instance
* enum consts   SMALL_WIDTH               -> SmallWidth
* interfaces    FoldingFeature            -> IFoldingFeature

Jetpack Compose packages (`*.compose*`) are listed separately: they need the Compose runtime and
are not usable from C#.

Usage: python3 tools/aar-api-doc.py --title Bodenberg.AppDimens.Sdps --out docs/X/API.md a.aar [b.aar ...]
"""
import argparse
import os
import re
import subprocess
import sys
import tempfile
import zipfile

PRIMITIVES = {"int": "int", "float": "float", "double": "double", "long": "long", "boolean": "bool",
              "void": "void", "byte": "byte", "char": "char", "short": "short"}
KNOWN = {
    "java.lang.String": "string", "java.lang.Object": "Java.Lang.Object", "java.lang.Integer": "Java.Lang.Integer",
    "java.lang.Float": "Java.Lang.Float", "java.lang.Boolean": "Java.Lang.Boolean", "java.lang.Double": "Java.Lang.Double",
    "android.content.Context": "Context", "android.content.res.Configuration": "Configuration",
    "android.content.res.Resources": "Resources", "android.util.DisplayMetrics": "DisplayMetrics",
    "android.app.Activity": "Activity", "android.view.View": "View",
    "androidx.window.layout.FoldingFeature": "AndroidX.Window.Layout.IFoldingFeature",
    "androidx.window.layout.WindowLayoutInfo": "AndroidX.Window.Layout.WindowLayoutInfo",
    "kotlin.Unit": "Kotlin.Unit", "kotlin.Pair": "Kotlin.Pair",
    "kotlin.jvm.internal.DefaultConstructorMarker": None,
}


def pascal(name: str) -> str:
    return name[:1].upper() + name[1:] if name else name


def const_to_pascal(name: str) -> str:
    if name.isupper() or "_" in name:
        return "".join(p[:1].upper() + p[1:].lower() for p in name.split("_") if p)
    return pascal(name)


def ns_of(pkg: str) -> str:
    return ".".join(pascal(p) for p in pkg.split("."))


def run_javap(jar: str, classes):
    out = []
    for i in range(0, len(classes), 200):
        chunk = classes[i:i + 200]
        r = subprocess.run(["javap", "-public", "-cp", jar] + chunk, capture_output=True, text=True)
        out.append(r.stdout)
    return "\n".join(out)


def parse(text: str):
    """Yield (header, members) for each class block in javap output."""
    blocks = []
    cur = None
    for line in text.splitlines():
        if line.startswith("Compiled from") or not line.strip():
            continue
        if not line.startswith(" ") and line.rstrip().endswith("{"):
            cur = [line.rstrip()[:-1].strip(), []]
            blocks.append(cur)
        elif line.strip() == "}":
            cur = None
        elif cur is not None:
            cur[1].append(line.strip().rstrip(";"))
    return blocks


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--title", required=True)
    ap.add_argument("--out", required=True)
    ap.add_argument("--intro", default=None)
    ap.add_argument("aars", nargs="+")
    a = ap.parse_args()

    all_blocks = []
    with tempfile.TemporaryDirectory() as tmp:
        for aar in a.aars:
            jar = os.path.join(tmp, os.path.basename(aar) + ".classes.jar")
            with zipfile.ZipFile(aar) as z, open(jar, "wb") as f:
                f.write(z.read("classes.jar"))
            with zipfile.ZipFile(jar) as z:
                classes = sorted(n[:-6].replace("/", ".") for n in z.namelist()
                                 if n.endswith(".class") and "$$" not in n and not re.search(r"\$\d", n))
            for header, members in parse(run_javap(jar, classes)):
                all_blocks.append((os.path.basename(aar), header, members))

    interfaces = set()
    for _, header, _ in all_blocks:
        m = re.search(r"\binterface\s+([\w.$]+)", header)
        if m:
            interfaces.add(m.group(1))

    def cs_type(t: str) -> str:
        t = t.strip()
        arr = ""
        while t.endswith("[]"):
            arr += "[]"
            t = t[:-2]
        if t.endswith("..."):
            return "params " + cs_type(t[:-3]) + "[]"
        generic = ""
        if "<" in t:
            base, args = t[:t.index("<")], t[t.index("<") + 1:t.rindex(">")]
            parts, depth, buf = [], 0, ""
            for ch in args:
                if ch == "<": depth += 1
                if ch == ">": depth -= 1
                if ch == "," and depth == 0:
                    parts.append(buf); buf = ""
                else:
                    buf += ch
            parts.append(buf)
            generic = "<" + ", ".join(cs_type(p.replace("? extends ", "").replace("? super ", "").replace("?", "Java.Lang.Object")) for p in parts) + ">"
            t = base
        if t in PRIMITIVES:
            return PRIMITIVES[t] + arr
        if t in KNOWN:
            return (KNOWN[t] or t) + generic + arr
        if t == "java.util.List":
            return "IList" + generic + arr
        if t == "java.util.Map":
            return "IDictionary" + generic + arr
        if t.startswith("kotlin.jvm.functions.Function"):
            return "Kotlin.Jvm.Functions.I" + t.rsplit(".", 1)[1] + arr
        pkg, _, cls = t.rpartition(".")
        cls = cls.replace("$", ".")
        if t in interfaces:
            parts = cls.split(".")
            parts[-1] = "I" + parts[-1]
            cls = ".".join(parts)
        if pkg.startswith("com.appdimens"):
            return cls + generic + arr
        return ns_of(pkg) + "." + cls + generic + arr

    def is_compose(block):
        return ".compose" in block[1] or "androidx.compose" in " ".join(block[2])

    usable = [b for b in all_blocks if not is_compose(b)]
    compose = [b for b in all_blocks if is_compose(b)]

    md = [f"# {a.title} — API reference", "",
          "> Generated by `tools/aar-api-doc.py` from the embedded AAR(s) "
          f"({', '.join(sorted(set(b[0] for b in all_blocks)))}).",
          "> Java/Kotlin signatures are shown with the C# names produced by the .NET for Android binding generator.",
          "> Methods marked *static* are called on the type: `DimenSdp.Sdp(context, 16)`. Do not edit by hand.", ""]
    if a.intro:
        md += [a.intro, ""]

    by_ns = {}
    for aar, header, members in usable:
        m = re.search(r"(class|interface|enum)\s+([\w.$]+)", header)
        if not m:
            continue
        kind, full = m.group(1), m.group(2)
        if "extends java.lang.Enum" in header:
            kind = "enum"
        pkg, _, cls = full.rpartition(".")
        by_ns.setdefault(ns_of(pkg), []).append((cls.replace("$", "."), kind, header, members, full))

    md += ["## Namespaces", "", "| Namespace | Types |", "|-----------|------:|"]
    for ns in sorted(by_ns):
        md.append(f"| `{ns}` | {len(by_ns[ns])} |")
    md.append("")

    for ns in sorted(by_ns):
        md += [f"## Namespace `{ns}`", ""]
        for cls, kind, header, members, full in sorted(by_ns[ns], key=lambda x: x[0]):
            if kind == "interface":
                parts = cls.split("."); parts[-1] = "I" + parts[-1]; cls = ".".join(parts)
            md += [f"### `{cls}` ({kind})", ""]
            rows = {"Enum values": [], "Constructors": [], "Properties": [], "Static methods": [], "Methods": [], "Fields": []}
            simple = full.rsplit(".", 1)[1]
            for mem in members:
                if "$" in mem.split("(")[0] or "DefaultConstructorMarker" in mem:
                    continue
                static = " static " in f" {mem} "
                mem = re.sub(r"^(?:(?:public|protected|static|final|abstract|synchronized|native|default|transient|volatile)\s+)+", "", mem)
                msig = re.match(r"(?:<[^>]+>\s+)?([\w.$<>\[\], ?]+?)\s+(\w+)\((.*)\)(?:\s+throws.*)?$", mem)
                ctor = re.match(r"([\w.$]+)\((.*)\)", mem)
                if kind == "enum" and static and "(" not in mem and mem.split()[-1].isupper():
                    rows["Enum values"].append(f"`{const_to_pascal(mem.split()[-1])}`")
                    continue
                if "(" not in mem:
                    fm = re.match(r".*?([\w.$<>\[\]]+)\s+(\w+)$", mem)
                    if fm:
                        name = fm.group(2)
                        if name == "INSTANCE":
                            rows["Properties"].append(f"`static {cs_type(fm.group(1))} Instance {{ get; }}`")
                        elif name != "Companion":
                            rows["Fields"].append(f"`{'static ' if static else ''}{cs_type(fm.group(1))} {const_to_pascal(name) if name.isupper() else pascal(name)}`")
                    continue
                if ctor and ctor.group(1).rsplit(".", 1)[-1].replace("$", ".").endswith(simple.replace("$", ".")) and not msig:
                    ps = [cs_type(p) for p in ctor.group(2).split(",") if p.strip()]
                    rows["Constructors"].append(f"`{cls.split('.')[-1]}({', '.join(ps)})`")
                    continue
                if not msig:
                    continue
                ret, name, params = msig.group(1), msig.group(2), msig.group(3)
                if kind == "enum" and name in ("values", "valueOf", "getEntries"):
                    continue
                ps = [cs_type(p) for p in re.split(r",(?![^<]*>)", params) if p.strip()]
                if not ps and re.match(r"(get|is)[A-Z]", name) and ret != "void":
                    prop = name[3:] if name.startswith("get") else name
                    rows["Properties"].append(f"`{'static ' if static else ''}{cs_type(ret)} {pascal(prop)} {{ get; }}`")
                    continue
                key = "Static methods" if static else "Methods"
                rows[key].append(f"`{'static ' if static else ''}{cs_type(ret)} {pascal(name)}({', '.join(ps)})`")
            if rows["Enum values"]:
                md += ["Values: " + ", ".join(rows["Enum values"]), ""]
            for group in ("Constructors", "Properties", "Fields", "Static methods", "Methods"):
                items = sorted(set(rows[group]))
                if not items:
                    continue
                md += [f"**{group}** ({len(items)})", ""]
                md += [f"- {i}" for i in items]
                md.append("")

    if compose:
        md += ["## Jetpack Compose types (not usable from C#)", "",
               "These Kotlin types require the Compose runtime (`androidx.compose.*`) and are not generated as usable "
               "C# APIs. Use the `*.Code` APIs above instead.", ""]
        names = sorted(set(re.search(r"(?:class|interface)\s+([\w.$]+)", h).group(1) for _, h, _ in compose
                           if re.search(r"(?:class|interface)\s+([\w.$]+)", h)))
        md += [f"- `{n}`" for n in names]
        md.append("")

    os.makedirs(os.path.dirname(os.path.abspath(a.out)), exist_ok=True)
    with open(a.out, "w") as f:
        f.write("\n".join(md) + "\n")
    print(f"{a.out}: {len(usable)} types (+{len(compose)} compose)")


if __name__ == "__main__":
    sys.exit(main())
