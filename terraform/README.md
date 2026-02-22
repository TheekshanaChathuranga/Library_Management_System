# Terraform Infrastructure for LMS K3s Cluster

This directory contains Terraform configuration to provision AWS infrastructure for the Library Management System K3s cluster.

## Prerequisites

1. **AWS CLI** configured with credentials
2. **Terraform** v1.0 or later
3. **SSH Key** created and imported to AWS

## Quick Start

### 1. Initialize Terraform

```bash
cd terraform/
terraform init
```

### 2. Review the Plan

```bash
terraform plan
```

This will create:
- 1 Security Group (all necessary ports)
- 1 EC2 instance (t3.medium) - Master node
- 2 EC2 instances (t3.medium) - Worker nodes

### 3. Apply Configuration

```bash
terraform apply
```

Type `yes` when prompted.

**Wait 5-10 minutes** for cluster initialization to complete.

### 4. Get Cluster Access

```bash
# Get kubeconfig
terraform output -raw get_kubeconfig | bash

# Test connection
kubectl get nodes
```

### 5. View Cluster Information

```bash
# Show all outputs
terraform output

# Get specific output
terraform output master_public_ip
terraform output lms_url
terraform output grafana_url
```

## Files

| File | Purpose |
|------|---------|
| `main.tf` | Main infrastructure definition |
| `variables.tf` | Input variables |
| `outputs.tf` | Output values |
| `master-init.sh` | K3s master bootstrap script |
| `worker-init.sh` | K3s worker bootstrap script |

## Variables

You can customize the deployment by creating a `terraform.tfvars` file:

```hcl
region                = "us-east-1"
cluster_name          = "library-k3s"
ssh_key_name          = "library-k3s"
master_instance_type  = "t3.medium"
worker_instance_type  = "t3.medium"
worker_count          = 2
```

## Outputs

After `terraform apply`, you'll get:

- **Master/Worker IPs**: Public and private IPs
- **SSH Command**: Direct SSH access
- **Kubeconfig Command**: Get cluster access
- **Service URLs**: LMS, ArgoCD, Grafana, OpenSearch, Vault

## Access Points

| Service | URL | Credentials |
|---------|-----|-------------|
| **LMS App** | `http://<MASTER_IP>:31063` | admin@library.local / ChangeMe!123 |
| **ArgoCD** | `http://<MASTER_IP>:30080` | admin / (get from cluster) |
| **Grafana** | `http://<MASTER_IP>:30081` | admin / prom-operator |
| **OpenSearch** | `http://<MASTER_IP>:30601` | - |
| **Vault** | `http://<MASTER_IP>:30820` | (initialize first) |

## Destroy Infrastructure

```bash
terraform destroy
```

Type `yes` when prompted.

## Next Steps

After infrastructure is ready:

1. **Deploy Applications**:
   ```bash
   cd ../ansible
   ansible-playbook -i inventory.yml playbooks/deploy-lms-application.yml
   ```

2. **Deploy Monitoring**:
   ```bash
   ansible-playbook -i inventory.yml playbooks/deploy-monitoring.yml
   ```

3. **Deploy Security**:
   ```bash
   ansible-playbook -i inventory.yml playbooks/deploy-security.yml
   ```

## Troubleshooting

### Can't connect to cluster

```bash
# Check if instances are running
terraform output master_public_ip

# SSH to master
terraform output -raw ssh_to_master | bash

# Check cluster status
cat /home/ubuntu/cluster-info.txt
```

### Cluster not ready

Wait 5-10 minutes after `terraform apply`. The master node needs time to:
- Install K3s
- Install ArgoCD
- Install Prometheus/Grafana
- Install metrics-server

### Need to update infrastructure

```bash
# Modify variables.tf or create terraform.tfvars
terraform plan    # Review changes
terraform apply   # Apply changes
```

## Cost Estimate

**Monthly cost** (us-east-1):
- 3x t3.medium instances: ~$90/month
- EBS volumes (100GB total): ~$10/month
- **Total**: ~$100/month

**Note**: Costs may vary by region and usage.
