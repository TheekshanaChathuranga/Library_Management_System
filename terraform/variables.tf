# Terraform Variables for LMS K3s Cluster

variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

variable "cluster_name" {
  description = "Name of the K3s cluster"
  type        = string
  default     = "library-k3s"
}

variable "ssh_key_name" {
  description = "Name of the SSH key pair in AWS"
  type        = string
  default     = "library-k3s"
}

variable "master_instance_type" {
  description = "EC2 instance type for master node"
  type        = string
  default     = "t3.medium"
}

variable "worker_instance_type" {
  description = "EC2 instance type for worker nodes"
  type        = string
  default     = "t3.medium"
}

variable "worker_count" {
  description = "Number of worker nodes"
  type        = number
  default     = 2
}
