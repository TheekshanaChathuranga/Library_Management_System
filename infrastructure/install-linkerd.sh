#!/bin/bash

# Linkerd Installation Script for LMS

# Installs Linkerd service mesh on K3s cluster

set -e

# Step 1: Check if kubectl is configured

if ! kubectl cluster-info &> /dev/null; then
echo "kubectl is not configured properly"
exit 1
fi

# Step 2: Install Linkerd CLI if not already installed

if ! command -v linkerd &> /dev/null; then
curl --proto '=https' --tlsv1.2 -sSfL [https://run.linkerd.io/install](https://run.linkerd.io/install) | sh
export PATH=$PATH:$HOME/.linkerd2/bin
echo 'export PATH=$PATH:$HOME/.linkerd2/bin' >> ~/.bashrc
fi

# Step 3: Pre-flight check

linkerd check --pre

# Step 4: Install Linkerd CRDs

linkerd install --crds | kubectl apply -f -

# Step 5: Install Linkerd control plane

linkerd install | kubectl apply -f -

# Step 6: Wait for Linkerd control plane to be ready

kubectl wait --for=condition=available --timeout=300s deployment -n linkerd -l linkerd.io/control-plane-ns=linkerd

# Step 7: Verify installation

linkerd check

# Step 8: Install Linkerd Viz (dashboard and metrics)

linkerd viz install | kubectl apply -f -
kubectl wait --for=condition=available --timeout=300s deployment -n linkerd-viz -l linkerd.io/extension=viz

# Step 9: Annotate LMS namespace for auto-injection

kubectl annotate namespace lms linkerd.io/inject=enabled --overwrite

# Step 10: Restart deployments in LMS namespace to inject Linkerd proxies

kubectl rollout restart deployment -n lms catalog-api
kubectl rollout restart deployment -n lms inventory-api
kubectl rollout restart deployment -n lms useridentity-api
kubectl rollout restart deployment -n lms borrowingreturns-api
kubectl rollout restart deployment -n lms frontend

# Wait for rollout to complete

kubectl rollout status deployment/catalog-api -n lms --timeout=300s
kubectl rollout status deployment/inventory-api -n lms --timeout=300s
kubectl rollout status deployment/useridentity-api -n lms --timeout=300s
kubectl rollout status deployment/borrowingreturns-api -n lms --timeout=300s
kubectl rollout status deployment/frontend -n lms --timeout=300s

# Step 11: Final verification of proxies

linkerd check --proxy

# Step 12: Dashboard and status commands

echo "Access Linkerd Dashboard: linkerd viz dashboard"
echo "Check service mesh status: linkerd viz stat deploy -n lms"
echo "View service topology: linkerd viz top deploy -n lms"
echo "View live traffic tap: linkerd viz tap deploy/catalog-api -n lms"