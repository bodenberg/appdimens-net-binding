#!/usr/bin/env bash
# Baixa o .aar oficial do Maven Central (mesma artefato Gradle publicado pela library Android).
set -euo pipefail
VERSION="${1:-3.1.5}"
DEST="$(dirname "$(dirname "$(realpath "$0")")")/AppDimens.Sdps.Binding/Jars/appdimens-sdps-${VERSION}.aar"
URL="https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-sdps/${VERSION}/appdimens-sdps-${VERSION}.aar"
mkdir -p "$(dirname "${DEST}")"
curl -fsSL -o "${DEST}" "${URL}"
echo "Atualizado: ${DEST}"
