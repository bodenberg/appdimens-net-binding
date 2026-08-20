#!/usr/bin/env bash
set -euo pipefail
VERSION="${1:-3.1.5}"
DEST="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Dynamic.Binding/Jars/appdimens-dynamic-${VERSION}.aar"
URL="https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-dynamic/${VERSION}/appdimens-dynamic-${VERSION}.aar"
JARS="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Dynamic.Binding/Jars"
mkdir -p "$(dirname "${DEST}")"
curl -fsSL -o "${DEST}" "${URL}"
find "${JARS}" -maxdepth 1 -name 'appdimens-dynamic-*.aar' ! -name "appdimens-dynamic-${VERSION}.aar" -delete
echo "Atualizado: ${DEST}"
