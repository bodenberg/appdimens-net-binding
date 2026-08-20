#!/usr/bin/env bash
set -euo pipefail
VERSION="${1:-3.1.5}"
DEST="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Ssps.Binding/Jars/appdimens-ssps-${VERSION}.aar"
URL="https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-ssps/${VERSION}/appdimens-ssps-${VERSION}.aar"
JARS="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Ssps.Binding/Jars"
mkdir -p "$(dirname "${DEST}")"
curl -fsSL -o "${DEST}" "${URL}"
find "${JARS}" -maxdepth 1 -name 'appdimens-ssps-*.aar' ! -name "appdimens-ssps-${VERSION}.aar" -delete
echo "Atualizado: ${DEST}"
