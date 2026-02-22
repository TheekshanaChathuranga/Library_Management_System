# Library Management System - K3s on EC2
# Terraform Configuration

terraform {
  required_version = ">= 1.0"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

provider "aws" {
  region = var.region
}

# ============================================
# Ubuntu AMI (Hardcoded for us-east-1)
# ============================================
# Using specific AMI ID to avoid ec2:DescribeImages permission requirement
# Ubuntu 22.04 LTS (Jammy) - us-east-1
locals {
  ubuntu_ami = "ami-0e86e20dae9224db8" # Ubuntu 22.04 LTS in us-east-1
}

# ============================================
# Security Group
# ============================================
resource "aws_security_group" "k3s" {
  name        = "${var.cluster_name}-sg"
  description = "Security group for K3s cluster"

  # SSH
  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # K3s API Server
  ingress {
    description = "K3s API"
    from_port   = 6443
    to_port     = 6443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTP
  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS
  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # NodePort Range (30000-32767)
  ingress {
    description = "NodePort Services"
    from_port   = 30000
    to_port     = 32767
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Allow all traffic within security group
  ingress {
    description = "Internal cluster communication"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    self        = true
  }

  # Allow all outbound traffic
  egress {
    description = "Allow all outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.cluster_name}-sg"
  }
}

# ============================================
# Random Token for K3s
# ============================================
resource "random_password" "k3s_token" {
  length  = 32
  special = false
}

# ============================================
# K3s Master Node
# ============================================
resource "aws_instance" "k3s_master" {
  ami                    = local.ubuntu_ami
  instance_type          = var.master_instance_type
  key_name               = var.ssh_key_name
  vpc_security_group_ids = [aws_security_group.k3s.id]

  root_block_device {
    volume_size = 40
    volume_type = "gp3"
  }

  user_data = templatefile("${path.module}/master-init.sh", {
    k3s_token = random_password.k3s_token.result
  })

  tags = {
    Name = "${var.cluster_name}-master"
    Role = "master"
  }
}

# ============================================
# K3s Worker Nodes
# ============================================
resource "aws_instance" "k3s_workers" {
  count                  = var.worker_count
  ami                    = local.ubuntu_ami
  instance_type          = var.worker_instance_type
  key_name               = var.ssh_key_name
  vpc_security_group_ids = [aws_security_group.k3s.id]

  root_block_device {
    volume_size = 30
    volume_type = "gp3"
  }

  user_data = templatefile("${path.module}/worker-init.sh", {
    master_ip = aws_instance.k3s_master.private_ip
    k3s_token = random_password.k3s_token.result
  })

  depends_on = [aws_instance.k3s_master]

  tags = {
    Name = "${var.cluster_name}-worker-${count.index + 1}"
    Role = "worker"
  }
}
