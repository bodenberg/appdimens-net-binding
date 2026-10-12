#!/usr/bin/env bash
set -euo pipefail

# Baixa a AAR appdimens-sdps do Maven Central para Jars/, e os metadados
# (pom/module/javadoc) para Maven/. Requer: curl, Maven Central online.
#
# Uso:
#   ./sync-aar-from-maven.sh           # usa versão padrão contida abaixo
#   ./sync-aar-from-maven.sh 3.2.0    # versão explícita
#   AAR_VERSION=3.2.0 ./sync-aar-from-maven.sh
#   MAVEN_REPO=https://maven-central.storage-download.googleapis.com/maven2 ./sync-aar-from-maven.sh  # mirror (ex.: 429 no Central)

AAR_VERSION="${1:-${AAR_VERSION:-3.2.0}}"
ARTIFACT="appdimens-sdps"
GROUP_PATH="io/github/bodenberg"
MAVEN_REPO="${MAVEN_REPO:-https://repo1.maven.org/maven2}"
BASE_URL="${MAVEN_REPO}/${GROUP_PATH}/${ARTIFACT}/${AAR_VERSION}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
JARS_DIR="${SCRIPT_DIR}/../AppDimens.Sdps.Binding/Jars"
MAVEN_DIR="${SCRIPT_DIR}/../AppDimens.Sdps.Binding/Maven"

mkdir -p "${JARS_DIR}" "${MAVEN_DIR}"

echo "Syncing ${ARTIFACT} ${AAR_VERSION} from Maven Central..."

# Remove qualquer AAR antiga antes de baixar a nova (evita AARs obsoletas no pacote)
echo "Cleaning old ${ARTIFACT} AARs in ${JARS_DIR}..."
find "${JARS_DIR}" -maxdepth 1 -name "${ARTIFACT}-*.aar" -delete
find "${MAVEN_DIR}" -maxdepth 1 -name "${ARTIFACT}-*" -delete

curl -fsSL --retry 8 --retry-delay 5 --retry-all-errors "${BASE_URL}/${ARTIFACT}-${AAR_VERSION}.aar" -o "${JARS_DIR}/${ARTIFACT}-${AAR_VERSION}.aar"

for ext in pom module; do
  curl -fsSL --retry 8 --retry-delay 5 --retry-all-errors "${BASE_URL}/${ARTIFACT}-${AAR_VERSION}.${ext}" -o "${MAVEN_DIR}/${ARTIFACT}-${AAR_VERSION}.${ext}"
done
curl -fsSL --retry 8 --retry-delay 5 --retry-all-errors "${BASE_URL}/${ARTIFACT}-${AAR_VERSION}-javadoc.jar" -o "${MAVEN_DIR}/${ARTIFACT}-${AAR_VERSION}-javadoc.jar"

echo "Done. Files in Jars/:"
ls -1 "${JARS_DIR}"
echo "Files in Maven/:"
ls -1 "${MAVEN_DIR}"
