#!/usr/bin/env python3
"""Generate a subset Chinese font for Bordy WeChat Mini Game fallback.

Usage:
    python3 tools/subset_font.py /path/to/NotoSansSC-Regular.ttf \
        Assets/Bordy/StreamingAssets/BordyFallback.ttf

Install dependency first:
    pip install fonttools
"""
import re
import sys
from pathlib import Path
from fontTools import subset
from fontTools.ttLib import TTFont

REPO_ROOT = Path(__file__).resolve().parent.parent
SCRIPTS_DIR = REPO_ROOT / "Assets" / "Bordy" / "Scripts"

EXTRA_CHARS = """0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ
=×·•，。！？、；：“”‘’（）《》【】—…\n 	"'"""


def collect_chinese_chars():
    """Scan C# source files and collect all Chinese characters used in strings."""
    chars = set(EXTRA_CHARS)
    pattern = re.compile(r'"([^"]*[\u4e00-\u9fff][^"]*)"')
    for path in SCRIPTS_DIR.rglob("*.cs"):
        try:
            text = path.read_text(encoding="utf-8")
        except Exception:
            continue
        for match in pattern.finditer(text):
            for ch in match.group(1):
                if ch >= "\u4e00" and ch <= "\u9fff":
                    chars.add(ch)
    # Also include common punctuation and digits needed by UI
    return chars


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        sys.exit(1)

    src = Path(sys.argv[1]).expanduser()
    dst = Path(sys.argv[2])
    if not src.is_absolute():
        src = REPO_ROOT / src
    if not dst.is_absolute():
        dst = REPO_ROOT / dst

    if not src.exists():
        print(f"ERROR: source font not found: {src}")
        sys.exit(1)

    chars = collect_chinese_chars()
    text = "".join(sorted(chars))
    print(f"[subset] collected {len(chars)} unique characters")
    print(f"[subset] sample: {text[:80]}")

    options = subset.Options()
    options.layout_features = "*"
    options.name_IDs = "*"
    options.notdef_outline = True
    options.recommended_glyphs = True
    options.desubroutinize = True
    options.hinting = False

    # TTC (TrueType Collection) may contain multiple fonts; pick the first one.
    font_number = 0 if str(src).lower().endswith(".ttc") else -1
    font = TTFont(str(src), fontNumber=font_number)
    subsetter = subset.Subsetter(options=options)
    subsetter.populate(text=text)
    subsetter.subset(font)

    dst.parent.mkdir(parents=True, exist_ok=True)
    font.save(str(dst))
    print(f"[subset] saved {dst} ({dst.stat().st_size / 1024:.1f} KB)")


if __name__ == "__main__":
    main()
