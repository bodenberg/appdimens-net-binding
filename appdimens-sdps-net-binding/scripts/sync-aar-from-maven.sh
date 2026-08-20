#!/usr/bin/env bash
# Baixa o .aar oficial do Maven Central (+ pom / module / javadoc de referência).
set -euo pipefail
VERSION="${1:-3.1.7}"
ROOT="$(dirname "$(dirname "$(realpath "$0")")")"
JARS="${ROOT}/AppDimens.Sdps.Binding/Jars"
MAVEN="${ROOT}/AppDimens.Sdps.Binding/Maven"
BASE="https://repo1.maven.org/maven2/io/github/bodenberg/appdimens-sdps/${VERSION}"

mkdir -p "${JARS}" "${MAVEN}"

curl -fsSL -o "${JARS}/appdimens-sdps-${VERSION}.aar" "${BASE}/appdimens-sdps-${VERSION}.aar"
curl -fsSL -o "${MAVEN}/appdimens-sdps-${VERSION}.pom" "${BASE}/appdimens-sdps-${VERSION}.pom"
curl -fsSL -o "${MAVEN}/appdimens-sdps-${VERSION}.module" "${BASE}/appdimens-sdps-${VERSION}.module"
# Maven usa o classificador -javadoc (hífen), não .javadoc
curl -fsSL -o "${MAVEN}/appdimens-sdps-${VERSION}-javadoc.jar" "${BASE}/appdimens-sdps-${VERSION}-javadoc.jar"

# Remove AARs antigas para evitar bind acidental
find "${JARS}" -maxdepth 1 -name 'appdimens-sdps-*.aar' ! -name "appdimens-sdps-${VERSION}.aar" -delete

echo "Atualizado: ${JARS}/appdimens-sdps-${VERSION}.aar"
echo "Metadados:  ${MAVEN}/appdimens-sdps-${VERSION}.{pom,module} + -javadoc.jar"
echo "Lembrete: atualize <AndroidLibrary Include=...> e <Version> no .csproj se a versão mudou."
