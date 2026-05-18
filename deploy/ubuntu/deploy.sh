#!/usr/bin/env bash
set -euo pipefail

APP_NAME="consensus"
PROJECT_FILE="WebActionResults.csproj"
APP_DIR="/var/www/${APP_NAME}"
ENV_DIR="/etc/${APP_NAME}"
ENV_FILE="${ENV_DIR}/${APP_NAME}.env"
LOCAL_ENV_FILE=".env"
SERVICE_FILE="/etc/systemd/system/${APP_NAME}.service"
NGINX_SITE="/etc/nginx/sites-available/${APP_NAME}"
NGINX_ENABLED="/etc/nginx/sites-enabled/${APP_NAME}"
PUBLISH_DIR="./publish"

if [[ ! -f "${PROJECT_FILE}" ]]; then
  echo "Run this script from the project root where ${PROJECT_FILE} exists."
  exit 1
fi

if [[ ! -f "${ENV_FILE}" ]]; then
  if [[ -f "${LOCAL_ENV_FILE}" ]]; then
    echo "Installing env file from ${LOCAL_ENV_FILE} to ${ENV_FILE}..."
    sudo mkdir -p "${ENV_DIR}"
    sudo cp "${LOCAL_ENV_FILE}" "${ENV_FILE}"
    sudo chmod 640 "${ENV_FILE}"
    sudo chown root:www-data "${ENV_FILE}"
  else
    echo "Missing required env file: ${ENV_FILE}"
    echo "No local ${LOCAL_ENV_FILE} found to install."
    echo "This deploy script requires an env file to run."
    exit 1
  fi
fi

echo "Publishing ${APP_NAME}..."
rm -rf "${PUBLISH_DIR}"
dotnet publish "${PROJECT_FILE}" -c Release -o "${PUBLISH_DIR}" /p:UseAppHost=false

echo "Installing app files to ${APP_DIR}..."
sudo mkdir -p "${APP_DIR}" "${ENV_DIR}"
sudo cp -a "${PUBLISH_DIR}/." "${APP_DIR}/"
sudo chown -R www-data:www-data "${APP_DIR}"

echo "Installing systemd service..."
sudo cp deploy/ubuntu/consensus.service "${SERVICE_FILE}"
sudo systemctl daemon-reload
sudo systemctl enable "${APP_NAME}"
sudo systemctl restart "${APP_NAME}"

echo "Installing Nginx site..."
sudo cp deploy/ubuntu/nginx-consensus.conf "${NGINX_SITE}"
sudo ln -sfn "${NGINX_SITE}" "${NGINX_ENABLED}"
sudo nginx -t
sudo systemctl reload nginx

echo "Deployment completed."
echo "Service status: sudo systemctl status ${APP_NAME}"
echo "Logs: sudo journalctl -u ${APP_NAME} -f"
