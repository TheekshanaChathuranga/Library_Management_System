#!/bin/bash
# K3s Worker Node Initialization

set -e

echo "Starting K3s Worker initialization..."

# Update system
export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get upgrade -y
apt-get install -y curl wget

# Wait for master to be ready
echo "Waiting for master node at ${master_ip}..."
for i in {1..60}; do
  if curl -k https://${master_ip}:6443 &> /dev/null; then
    echo "Master node is ready!"
    break
  fi
  echo "Attempt $i/60: Master not ready yet, waiting..."
  sleep 10
done

# Install K3s as worker
echo "Installing K3s worker node..."
curl -sfL https://get.k3s.io | INSTALL_K3S_VERSION=v1.28.5+k3s1 \
  K3S_URL=https://${master_ip}:6443 \
  K3S_TOKEN=${k3s_token} \
  sh -s - agent --node-name=$(hostname)

echo "✅ K3s Worker setup complete!"
