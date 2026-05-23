#!/usr/bin/env python3
"""Generate AppDimens MAUI ResourceDictionary files (parity with Android generator-sdps.py).

Layout v2: Dimens.Base.xaml + Dimens.{N}.xaml with axis-neutral keys (_1, _16, _minus8).
Axis (sw/w/h) is chosen at runtime by bucket selection, not by duplicate keys.
"""
import json
import os
import shutil

BASE_DIR = os.path.join(
    os.path.dirname(__file__), "..", "src", "AppDimens.Maui.Resources", "Generated"
)

MIN_DP = 30
MAX_DP = 5120
STEP = 15
EXTRA_SIZES = [
    384, 393, 400, 410, 411, 412,
    427, 432, 440, 533,
    640, 667, 673, 685,
    768, 820,
    1024, 1280,
]
MAX_POSITIVE = 600
MAX_NEGATIVE = 300
BASE_DP = 300.0
LAYOUT_VERSION = 2


def build_size_list():
    auto_sizes = list(range(MIN_DP, MAX_DP + 1, STEP))
    return sorted(set(auto_sizes + EXTRA_SIZES))


def generate_xaml(scale: float) -> str:
    lines = [
        '<?xml version="1.0" encoding="utf-8"?>',
        '<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"',
        '                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">',
    ]
    for i in range(1, MAX_POSITIVE + 1):
        val = i * scale
        lines.append(f'    <x:Double x:Key="_{i}">{val:.2f}</x:Double>')
    for i in range(1, MAX_NEGATIVE + 1):
        val = -i * scale
        lines.append(f'    <x:Double x:Key="_minus{i}">{val:.2f}</x:Double>')
    lines.append("</ResourceDictionary>")
    return "\n".join(lines) + "\n"


def generate_lookup_table(sizes: list[int]) -> str:
    data = {
        "layoutVersion": LAYOUT_VERSION,
        "sizes": sizes,
        "baseDp": BASE_DP,
        "maxPositive": MAX_POSITIVE,
        "maxNegative": MAX_NEGATIVE,
    }
    return json.dumps(data, indent=2)


def remove_legacy_layout():
    for sub in ("sw", "w", "h"):
        path = os.path.join(BASE_DIR, sub)
        if os.path.isdir(path):
            shutil.rmtree(path)
    for q in ("sw", "w", "h"):
        legacy_base = os.path.join(BASE_DIR, f"Dimens.Base.{q}.xaml")
        if os.path.isfile(legacy_base):
            os.remove(legacy_base)


def main():
    sizes = build_size_list()
    os.makedirs(BASE_DIR, exist_ok=True)
    remove_legacy_layout()

    base_path = os.path.join(BASE_DIR, "Dimens.Base.xaml")
    with open(base_path, "w", encoding="utf-8") as f:
        f.write(generate_xaml(1.0))

    for size in sizes:
        scale = size / BASE_DP
        path = os.path.join(BASE_DIR, f"Dimens.{size}.xaml")
        with open(path, "w", encoding="utf-8") as f:
            f.write(generate_xaml(scale))

    lookup_path = os.path.join(BASE_DIR, "buckets.json")
    with open(lookup_path, "w", encoding="utf-8") as f:
        f.write(generate_lookup_table(sizes))

    print(
        f"Generated layout v{LAYOUT_VERSION}: {len(sizes)} buckets + Dimens.Base.xaml + buckets.json in {BASE_DIR}"
    )


if __name__ == "__main__":
    main()
