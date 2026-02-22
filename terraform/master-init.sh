#!/bin/bash
# K3s Master Node Initialization
# Minimal setup - Ansible will handle all service deployments

set -e

echo "Starting K3s Master initialization..."

# Update system
export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get upgrade -y
apt-get install -y curl wget git jq python3 python3-pip

# Install Ansible dependencies
pip3 install kubernetes

# Get public IP
PUBLIC_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4)
PRIVATE_IP=$(hostname -I | awk '{print $1}')

# Install K3s Master
echo "Installing K3s master node..."
curl -sfL https://get.k3s.io | INSTALL_K3S_VERSION=v1.28.5+k3s1 sh -s - server \
  --token="${k3s_token}" \
  --write-kubeconfig-mode=644 \
  --disable=traefik \
  --node-name=library-k3s-master \
  --tls-san=$PUBLIC_IP \
  --tls-san=$PRIVATE_IP

# Wait for K3s to start
echo "Waiting for K3s to start..."
until kubectl get nodes &> /dev/null; do 
  sleep 5
done

# Install Helm (needed by Ansible)
echo "Installing Helm..."
curl -fsSL https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Create cluster info file
cat > /home/ubuntu/cluster-info.txt << EOF
════════════════════════════════════════════════════════════════
                    K3S CLUSTER INFORMATION
════════════════════════════════════════════════════════════════

✅ Cluster Status: READY (Basic Setup)

📍 IP Addresses:
   Private IP: $PRIVATE_IP
   Public IP:  $PUBLIC_IP

� Installed Components:
   ✓ K3s v1.28.5 (Master)
   ✓ Helm 3
   ✓ kubectl

🔑 K3s Token (for workers): ${k3s_token}

📄 Kubeconfig: /etc/rancher/k3s/k3s.yaml

⚠️  Next Steps:
   1. Wait for worker nodes to join
   2. Run Ansible playbooks to deploy services:
      - ArgoCD (GitOps)
      - Prometheus + Grafana (Monitoring)
      - OpenSearch (Logging)
      - Vault (Secrets)
      - Linkerd (Service Mesh)
      - LMS Application

════════════════════════════════════════════════════════════════
EOF

chmod 644 /home/ubuntu/cluster-info.txt

echo "✅ K3s Master initialization complete!"
echo "📄 Cluster info saved to /home/ubuntu/cluster-info.txt"
echo "⚠️  Run Ansible playbooks to deploy all services"
