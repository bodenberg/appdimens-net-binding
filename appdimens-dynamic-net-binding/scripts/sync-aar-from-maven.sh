#!/usr/bin/env bash
set -euo pipefail

# Baixa os AARs modulares appdimens-dynamic do Maven Central para Jars/, e os
# metadados (pom/module/javadoc) para Maven/. Requer: curl, Maven Central online.
#
# Uso:
#   ./sync-aar-from-maven.sh           # usa versão padrão contida abaixo
#   ./sync-aar-from-maven.sh 3.1.9    # versão explícita
#   AAR_VERSION=3.1.9 ./sync-aar-from-maven.sh

AAR_VERSION="${1:-${AAR_VERSION:-3.1.9}}"
GROUP_PATH="io/github/bodenberg"
BASE_URL="https://repo1.maven.org/maven2/${GROUP_PATH}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
JARS_DIR="${SCRIPT_DIR}/../AppDimens.Dynamic.Binding/Jars"
MAVEN_DIR="${SCRIPT_DIR}/../AppDimens.Dynamic.Binding/Maven"

mkdir -p "${JARS_DIR}" "${MAVEN_DIR}"

# Artifact principal (core + scaled) + módulos de estratégia
MODULES=(
  appdimens-dynamic
  appdimens-dynamic-logarithmic
  appdimens-dynamic-fluid
  appdimens-dynamic-fill
  appdimens-dynamic-diagonal
  appdimens-dynamic-power
  appdimens-dynamic-density
  appdimens-dynamic-fit
  appdimens-dynamic-resize
  appdimens-dynamic-units
  appdimens-dynamic-perimeter
  appdimens-dynamic-interpolated
  appdimens-dynamic-percent
  appdimens-dynamic-auto
)

echo "Syncing appdimens-dynamic* ${AAR_VERSION} from Maven Central..."

# Remove qualquer AAR antiga antes de baixar as novas (evita AARs obsoletas no pacote)
echo "Cleaning old appdimens-dynamic AARs in ${JARS_DIR}..."
find "${JARS_DIR}" -maxdepth 1 -name "appdimens-dynamic-*.aar" -delete

for m in "${MODULES[@]}"; do
  curl -fsSL "${BASE_URL}/${m}/${AAR_VERSION}/${m}-${AAR_VERSION}.aar" -o "${JARS_DIR}/${m}-${AAR_VERSION}.aar"
  # Metadados de referência (exceto o principal, que também baixa javadoc)
  curl -fsSL "${BASE_URL}/${m}/${AAR_VERSION}/${m}-${AAR_VERSION}.pom" -o "${MAVEN_DIR}/${m}-${AAR_VERSION}.pom"
  curl -fsSL "${BASE_URL}/${m}/${AAR_VERSION}/${m}-${AAR_VERSION}.module" -o "${MAVEN_DIR}/${m}-${AAR_VERSION}.module"
done
curl -fsSL "${BASE_URL}/appdimens-dynamic/${AAR_VERSION}/appdimens-dynamic-${AAR_VERSION}-javadoc.jar" -o "${MAVEN_DIR}/appdimens-dynamic-${AAR_VERSION}-javadoc.jar"

echo "Done. Files in Jars/:"
ls -1 "${JARS_DIR}"
echo "Files in Maven/:"
ls -1 "${MAVEN_DIR}"
