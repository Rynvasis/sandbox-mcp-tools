"""Centralized configuration for the Docker sandbox container."""

from __future__ import annotations

from pathlib import Path

# Container identity
CONTAINER_NAME: str = "sandbox-mcp-container"
CONTAINER_IMAGE: str = "sandbox-mcp-tools:latest"

# Workspace mount
WORKSPACE_HOST_PATH: str = str(Path("workspaces/default").resolve())
WORKSPACE_CONTAINER_PATH: str = "/workspace"

# Resource limits
MEMORY_LIMIT: str = "512m"
CPU_LIMIT: float = 0.5
PIDS_LIMIT: int = 64

# Security controls
NETWORK_MODE: str = "none"
READ_ONLY: bool = True
TMPFS_SIZE: str = "64m"

# Timeouts and limits
HEALTH_CHECK_TIMEOUT: int = 5
DEFAULT_TIMEOUT_PYTHON: int = 30
DEFAULT_TIMEOUT_BASH: int = 15
MAX_OUTPUT_SIZE: int = 100_000

