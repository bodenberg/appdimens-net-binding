#!/usr/bin/env bash
set -euo pipefail
VERSION="${1:-3.1.5}"
DEST="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Dynamic.Binding/Jars/appdimens-dynamic-${VERSION}.aar"
URL="https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-dynamic/${VERSION}/appdimens-dynamic-${VERSION}.aar"
mkdir -p "$(dirname "${DEST}")"
curl -fsSL -o "${DEST}" "${URL}"
echo "Atualizado: ${DEST}"
