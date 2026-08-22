#!/usr/bin/env bash
set -euo pipefail

# Baixa a AAR appdimens-sdps do Maven Central para Jars/, e os metadados
# (pom/module/javadoc) para Maven/. Requer: curl, Maven Central online.
#
# Uso:
#   ./sync-aar-from-maven.sh           # usa versão padrão contida abaixo
#   ./sync-aar-from-maven.sh 3.1.8    # versão explícita
#   AAR_VERSION=3.1.8 ./sync-aar-from-maven.sh

AAR_VERSION="${1:-${AAR_VERSION:-3.1.7}}"
ARTIFACT="appdimens-sdps"
GROUP_PATH="io/github/bodenberg"
BASE_URL="https://repo1.maven.org/maven2/${GROUP_PATH}/${ARTIFACT}/${AAR_VERSION}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
JARS_DIR="${SCRIPT_DIR}/../AppDimens.Sdps.Binding/Jars"
MAVEN_DIR="${SCRIPT_DIR}/../AppDimens.Sdps.Binding/Maven"

mkdir -p "${JARS_DIR}" "${MAVEN_DIR}"

echo "Syncing ${ARTIFACT} ${AAR_VERSION} from Maven Central..."

# Remove qualquer AAR antiga antes de baixar a nova (evita AARs obsoletas no pacote)
echo "Cleaning old ${ARTIFACT} AARs in ${JARS_DIR}..."
find "${JARS_DIR}" -maxdepth 1 -name "${ARTIFACT}-*.aar" -delete

curl -fsSL "${BASE_URL}/${ARTIFACT}-${AAR_VERSION}.aar" -o "${JARS_DIR}/${ARTIFACT}-${AAR_VERSION}.aar"

for ext in pom module; do
  curl -fsSL "${BASE_URL}/${ARTIFACT}-${AAR_VERSION}.${ext}" -o "${MAVEN_DIR}/${ARTIFACT}-${AAR_VERSION}.${ext}"
done
curl -fsSL "${BASE_URL}/${ARTIFACT}-${AAR_VERSION}-javadoc.jar" -o "${MAVEN_DIR}/${ARTIFACT}-${AAR_VERSION}-javadoc.jar"

echo "Done. Files in Jars/:"
ls -1 "${JARS_DIR}"
echo "Files in Maven/:"
ls -1 "${MAVEN_DIR}"
