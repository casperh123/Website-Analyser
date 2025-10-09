set -euo pipefail  # Exit on error, undefined vars, and pipeline failures

set -a
source .env
set +a

IMAGE_TAG="clyppertechnology/websiteanalyzer:0.0.76"

echo "Building image ${IMAGE_TAG}..."
docker build --no-cache -t "${IMAGE_TAG}" .

echo "Pushing image ${IMAGE_TAG}..."
#docker push "${IMAGE_TAG}"