set -a
source .env
set +a

docker build --build-arg NEW_RELIC_LICENSE_KEY=$NEW_RELIC_LICENSE_KEY -t clyppertechnology/websiteanalyzer:0.0.63 .
docker push clyppertechnology/websiteanalyzer:0.0.63
