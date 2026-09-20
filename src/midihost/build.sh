#!/bin/bash
# Builds the universal (Apple silicon + Intel) MIDI helper into the plugin package.
set -euo pipefail
cd "$(dirname "$0")"
out="../package/bin/logicmidihost"
mkdir -p "$(dirname "$out")"
clang -arch arm64 -arch x86_64 -O2 -Wall \
  -framework CoreMIDI -framework CoreFoundation \
  -o "$out" logicmidihost.c
codesign --force --sign - "$out"
echo "built $out"
lipo -info "$out"
