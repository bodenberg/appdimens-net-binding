#!/usr/bin/env bash
set -euo pipefail
VERSION="${1:-3.1.5}"
DEST="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Ssps.Binding/Jars/appdimens-ssps-${VERSION}.aar"
URL="https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-ssps/${VERSION}/appdimens-ssps-${VERSION}.aar"
mkdir -p "$(dirname "${DEST}")"
curl -fsSL -o "${DEST}" "${URL}"
echo "Atualizado: ${DEST}"
