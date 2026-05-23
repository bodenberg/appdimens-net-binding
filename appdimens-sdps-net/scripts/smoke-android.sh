#!/usr/bin/env bash
# Smoke test AppDimens sample on Android emulator via adb + uiautomator.
set -euo pipefail

PKG=com.bodenberg.appdimens.sample
ACTIVITY=${PKG}/crc64fcdac5cc55e0a84a.MainActivity
APK="${1:-samples/AppDimens.Maui.Sample/bin/Release/net10.0-android/android-x64/publish/com.bodenberg.appdimens.sample-Signed.apk}"

log() { echo "[smoke] $*"; }
failures=0

tap_text() {
  local text="$1"
  adb shell uiautomator dump /sdcard/ui.xml >/dev/null 2>&1
  adb pull /sdcard/ui.xml /tmp/ui.xml >/dev/null 2>&1
  python3 - "$text" <<'PY'
import re, sys, subprocess
text = sys.argv[1]
xml = open("/tmp/ui.xml", encoding="utf-8", errors="ignore").read()
# match text or content-desc containing target (case-sensitive substring)
pat = re.compile(r'text="([^"]*)"[^>]*bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"')
pat2 = re.compile(r'content-desc="([^"]*)"[^>]*bounds="\[(\d+),(\d+)\]\[(\d+),(\d+)\]"')
candidates = []
for m in pat.finditer(xml):
    if text in m.group(1):
        x1,y1,x2,y2 = map(int, m.groups()[1:])
        candidates.append(((x1+x2)//2, (y1+y2)//2, m.group(1)))
for m in pat2.finditer(xml):
    if text in m.group(1):
        x1,y1,x2,y2 = map(int, m.groups()[1:])
        candidates.append(((x1+x2)//2, (y1+y2)//2, m.group(1)))
if not candidates:
    print(f"NOTFOUND:{text}")
    sys.exit(2)
x,y,label = candidates[0]
print(f"TAP:{text}@{x},{y}")
subprocess.check_call(["adb", "shell", "input", "tap", str(x), str(y)])
PY
}

check_crashes() {
  local step="$1"
  sleep 1
  if adb logcat -d -t 40 | grep -q "FATAL EXCEPTION.*${PKG}\|XamlParseException\|KeyNotFoundException"; then
    log "FAIL at: $step"
    adb logcat -d -t 40 | grep -E "FATAL EXCEPTION|XamlParse|KeyNotFound|JavaProxyThrowable: \[" | tail -5
    failures=$((failures + 1))
    return 1
  fi
  if ! adb shell pidof "$PKG" >/dev/null 2>&1; then
    log "FAIL (process died) at: $step"
    failures=$((failures + 1))
    return 1
  fi
  log "OK: $step"
}

rotate() {
  local deg="$1"
  log "Rotating to $deg"
  adb shell settings put system accelerometer_rotation 0 >/dev/null
  case "$deg" in
    portrait) adb shell settings put system user_rotation 0 ;;
    landscape) adb shell settings put system user_rotation 1 ;;
    reverse) adb shell settings put system user_rotation 3 ;;
  esac
  sleep 2
}

log "Installing $APK"
adb install -r "$APK" >/dev/null
adb shell am force-stop "$PKG" >/dev/null
adb logcat -c
adb shell am start -n "$ACTIVITY" >/dev/null
sleep 4
check_crashes "launch home"

log "Tap refresh metrics"
tap_text "Atualizar" || true
sleep 2
check_crashes "refresh metrics"

for tab in "Dimensões" "Inversores" "Avançado" "Benchmark" "Início"; do
  log "Tab: $tab"
  tap_text "$tab" || { log "WARN: tab not found $tab"; continue; }
  sleep 2
  check_crashes "tab $tab" || true
done

log "Home nav cards"
tap_text "Início" || true
sleep 1
for card in "Dimensões base" "Inversores" "Avançado" "Benchmark"; do
  tap_text "$card" || { log "WARN: card $card"; continue; }
  sleep 2
  check_crashes "card $card" || true
  log "Back button"
  tap_text "Voltar" || tap_text "Navigate up" || tap_text "Início" || true
  sleep 2
  check_crashes "back from $card" || true
done

log "Benchmark run"
tap_text "Benchmark" || true
sleep 2
tap_text "Executar" || true
sleep 8
check_crashes "benchmark run" || true

log "Rotation tests"
rotate landscape
check_crashes "landscape inverters" || true
tap_text "Inversores" || true
sleep 2
check_crashes "landscape inverters tab" || true
rotate portrait
sleep 2
check_crashes "back to portrait" || true

rotate reverse
sleep 2
tap_text "Avançado" || true
sleep 2
check_crashes "reverse landscape advanced" || true
rotate portrait

log "Re-enable auto rotation"
adb shell settings put system accelerometer_rotation 1 >/dev/null

if [ "$failures" -eq 0 ]; then
  log "All smoke checks passed"
  exit 0
else
  log "$failures failure(s)"
  exit 1
fi
