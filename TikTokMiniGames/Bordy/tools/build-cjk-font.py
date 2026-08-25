#!/usr/bin/env python3
"""
Rebuild Assets/Bordy/Resources/Bordy/BordyUI.otf — a Noto Sans CJK subset that
contains ONLY the glyphs used anywhere in the project (plus full kana + Latin +
punctuation as insurance). Run this whenever you add new Chinese/Japanese UI text,
otherwise the new characters render blank on device (WebGL has no OS font fallback).

Requirements: fonttools (pyftsubset) + a Noto Sans CJK source font.
    pip install fonttools
Source font: NotoSansCJK-Regular.ttc (Debian/Ubuntu: apt install fonts-noto-cjk),
or download NotoSansCJKsc-Regular.otf from https://github.com/notofonts/noto-cjk .

Usage:
    python3 tools/build-cjk-font.py [path-to-NotoSansCJK-Regular.ttc]
"""
import sys, os, glob
from fontTools.subset import main as pyftsubset

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)                       # Bordy/
ASSETS = os.path.join(ROOT, "Assets")
OUT = os.path.join(ASSETS, "Bordy", "Resources", "Bordy", "BordyUI.otf")

DEFAULT_SRCS = [
    "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
    os.path.join(HERE, "NotoSansCJKsc-Regular.otf"),
]

def collect_chars():
    chars = set()
    files = []
    for ext in ("*.cs", "*.jslib", "*.json"):
        files += glob.glob(os.path.join(ASSETS, "**", ext), recursive=True)
    for fp in files:
        try:
            t = open(fp, encoding="utf-8", errors="ignore").read()
        except Exception:
            continue
        for ch in t:
            o = ord(ch)
            if (0x2E80 <= o <= 0x9FFF) or (0x3000 <= o <= 0x30FF) or (0x3400 <= o <= 0x4DBF) \
               or (0xF900 <= o <= 0xFAFF) or (0xFF00 <= o <= 0xFFEF) or (0x2010 <= o <= 0x206F) \
               or (0x00A0 <= o <= 0x024F) \
               or (0x2190 <= o <= 0x21FF) or (0x2200 <= o <= 0x22FF) \
               or (0x2460 <= o <= 0x24FF) or (0x2500 <= o <= 0x27BF) \
               or o in (0x00D7, 0x00B7):
                # pyftsubset silently drops any of these not present in the source font,
                # so requesting a wide symbol range is safe.
                chars.add(ch)
    # insurance: full hiragana + katakana + common CJK punctuation
    for o in range(0x3040, 0x30FF + 1):
        chars.add(chr(o))
    for c in "。，、；：？！“”‘’（）《》【】—…·×":
        chars.add(c)
    return "".join(sorted(chars)), len(files)

def main():
    src = sys.argv[1] if len(sys.argv) > 1 else next((s for s in DEFAULT_SRCS if os.path.exists(s)), None)
    if not src or not os.path.exists(src):
        sys.exit("Source Noto Sans CJK font not found. Pass its path as an argument.")
    text, nfiles = collect_chars()
    tmp = os.path.join(HERE, "_cjk_chars.txt")
    open(tmp, "w", encoding="utf-8").write(text)
    print(f"scanned {nfiles} files, {len(text)} unique glyphs -> {OUT}")
    argv = [
        src,
        "--font-number=0",
        f"--text-file={tmp}",
        "--unicodes=U+0020-007E,U+00A0",
        f"--output-file={OUT}",
        "--layout-features=*", "--name-IDs=*",
        "--recalc-bounds", "--recalc-timestamp", "--no-hinting", "--desubroutinize",
    ]
    pyftsubset(argv)
    os.remove(tmp)
    print("done:", os.path.getsize(OUT), "bytes")

if __name__ == "__main__":
    main()
