#!/bin/bash
set -e

cd /opt/ms-logistic/

git pull
DROPLET_IP=$(curl -s --max-time 5 http://169.254.169.254/metadata/v1/interfaces/public/0/ipv4/address)

if [ -z "$DROPLET_IP" ]; then
    echo "ERROR: No se pudo obtener DROPLET_IP desde la metadata API"
    exit 1
fi

if ! grep -q "^INFRA_HOST=" .env; then
    echo "ERROR: INFRA_HOST no está configurado en .env"
    exit 1
fi

docker compose -f docker-compose.prod.yml down

export DROPLET_IP
docker compose -f docker-compose.prod.yml up -d --build
