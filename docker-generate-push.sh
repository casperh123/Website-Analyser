set -euo pipefail  # Exit on error, undefined vars, and pipeline failures

set -a
source .env
set +a

# Check if NEW_RELIC_LICENSE_KEY is set
if [ -z "${NEW_RELIC_LICENSE_KEY:-}" ]; then
    echo "ERROR: NEW_RELIC_LICENSE_KEY is not set in .env file"
    exit 1
fi

IMAGE_TAG="clyppertechnology/websiteanalyzer:0.0.76"

echo "Building image ${IMAGE_TAG}..."
docker build --no-cache --build-arg NEW_RELIC_LICENSE_KEY="$NEW_RELIC_LICENSE_KEY" -t "${IMAGE_TAG}" .

echo "Pushing image ${IMAGE_TAG}..."
#docker push "${IMAGE_TAG}"