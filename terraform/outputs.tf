# Terraform Outputs for LMS K3s Cluster

output "master_public_ip" {
  description = "Public IP of K3s master node"
  value       = aws_instance.k3s_master.public_ip
}

output "master_private_ip" {
  description = "Private IP of K3s master node"
  value       = aws_instance.k3s_master.private_ip
}

output "worker_public_ips" {
  description = "Public IPs of K3s worker nodes"
  value       = aws_instance.k3s_workers[*].public_ip
}

output "worker_private_ips" {
  description = "Private IPs of K3s worker nodes"
  value       = aws_instance.k3s_workers[*].private_ip
}

output "ssh_to_master" {
  description = "SSH command to connect to master"
  value       = "ssh -i ~/.ssh/${var.ssh_key_name} ubuntu@${aws_instance.k3s_master.public_ip}"
}

output "get_kubeconfig" {
  description = "Command to get kubeconfig"
  value       = "scp -i ~/.ssh/${var.ssh_key_name} ubuntu@${aws_instance.k3s_master.public_ip}:/etc/rancher/k3s/k3s.yaml ~/.kube/config && sed -i 's/127.0.0.1/${aws_instance.k3s_master.public_ip}/g' ~/.kube/config"
}

output "lms_url" {
  description = "LMS Application URL"
  value       = "http://${aws_instance.k3s_master.public_ip}:31063"
}

output "argocd_url" {
  description = "ArgoCD Web UI URL"
  value       = "http://${aws_instance.k3s_master.public_ip}:30080"
}

output "grafana_url" {
  description = "Grafana Web UI URL"
  value       = "http://${aws_instance.k3s_master.public_ip}:30081"
}

output "opensearch_url" {
  description = "OpenSearch Dashboards URL"
  value       = "http://${aws_instance.k3s_master.public_ip}:30601"
}

output "vault_url" {
  description = "Vault UI URL"
  value       = "http://${aws_instance.k3s_master.public_ip}:30820"
}

output "cluster_info" {
  description = "Complete cluster information"
  value       = <<-EOT
  
  ╔════════════════════════════════════════════════════════════════╗
  ║           K3s Cluster Created Successfully! 🎉                 ║
  ╚════════════════════════════════════════════════════════════════╝
  
  📍 Master Node:
     Public IP:  ${aws_instance.k3s_master.public_ip}
     Private IP: ${aws_instance.k3s_master.private_ip}
  
  📍 Worker Nodes:
     Public IPs:  ${join(", ", aws_instance.k3s_workers[*].public_ip)}
     Private IPs: ${join(", ", aws_instance.k3s_workers[*].private_ip)}
  
  ⏰ IMPORTANT: Wait 5-10 minutes for cluster initialization
  
  🌐 Access Points:
     - LMS App:     http://${aws_instance.k3s_master.public_ip}:31063
     - ArgoCD UI:   http://${aws_instance.k3s_master.public_ip}:30080
     - Grafana UI:  http://${aws_instance.k3s_master.public_ip}:30081
     - OpenSearch:  http://${aws_instance.k3s_master.public_ip}:30601
     - Vault UI:    http://${aws_instance.k3s_master.public_ip}:30820
  
  🔑 SSH Access:
     ssh -i ~/.ssh/${var.ssh_key_name} ubuntu@${aws_instance.k3s_master.public_ip}
  
  📥 Get Cluster Access:
     terraform output -raw get_kubeconfig | bash
     kubectl get nodes
  
  ✅ Next Steps:
     1. Wait 5-10 minutes for initialization
     2. Get kubeconfig: terraform output -raw get_kubeconfig | bash
     3. Test cluster: kubectl get nodes
     4. Deploy apps: cd ../ansible && ansible-playbook -i inventory.yml playbooks/deploy-lms-application.yml
  
  EOT
}
