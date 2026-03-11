# Implementation Plan: Sandbox MCP Tools for Cursor

**Date**: 2026-03-09
**Status**: Draft
**Version**: 1.0

---

## 1. System Architecture

### 1.1 Overview

Sandbox MCP Tools is a set of MCP (Model Context Protocol) servers that allow
Cursor AI to execute Python code, Bash commands, and file operations inside
isolated Docker containers. The system prevents untrusted AI-generated code
from running on the host machine.

### 1.2 Architecture Diagram

```
┌──────────────────────────────────────────────────────────┐
│                      Cursor IDE                          │
│                   (MCP Client)                           │
└──────────┬───────────┬───────────┬───────────────────────┘
           │ stdio     │ stdio     │ stdio
           ▼           ▼           ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Python MCP   │ │  Bash MCP    │ │  File MCP    │
│   Server     │ │   Server     │ │   Server     │
└──────┬───────┘ └──────┬───────┘ └──────┬───────┘
       │                │                │
       └────────┬───────┴────────┬───────┘
                ▼                │
       ┌─────────────────┐      │
       │ Sandbox Runtime │◄─────┘
       │   (Orchestrator)│
       └────────┬────────┘
                │ Docker SDK
                ▼
       ┌─────────────────┐
       │ Docker Container │
       │  (sandbox-env)   │
       │                  │
       │  ┌────────────┐  │
       │  │ /workspace │  │
       │  └────────────┘  │
       └──────────────────┘
                ▲
                │ bind mount
       ┌────────────────────┐
       │ Host Workspace Dir │
       │ ./workspaces/      │
       └────────────────────┘
```

### 1.3 Communication Flow

1. **Cursor → MCP Server**: Cursor starts each MCP server as a child process
   using **stdio transport**. It sends JSON-RPC requests over stdin and reads
   responses from stdout.

2. **MCP Server → Sandbox Runtime**: Each MCP server delegates execution to the
   shared Sandbox Runtime module, which manages Docker container lifecycle and
   command execution.

3. **Sandbox Runtime → Docker**: The runtime uses the Docker SDK for Python to
   execute commands inside the sandbox container via `container.exec_run()`.

4. **Results flow back** through the same chain: Docker → Runtime → MCP Server
   → Cursor.

### 1.4 Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Transport | stdio | Cursor's native MCP transport; simplest for local tools |
| MCP SDK | `mcp[cli]` (FastMCP) | Official Python SDK with decorator-based tool definitions |
| Container mgmt | Docker SDK for Python | Programmatic control over container lifecycle |
| Container strategy | Persistent, reused | Eliminates cold-start latency per execution |
| Workspace isolation | Bind mount to `/workspace` | Single controlled directory; container has no other host access |

---

## 2. MCP Server Design

### 2.1 MCP Protocol Basics

Each MCP server is a standalone Python process that:

- Communicates via **stdio** (stdin/stdout JSON-RPC)
- Declares **tools** with typed schemas (name, description, input parameters)
- Returns structured responses (text content, errors)
- Uses the `mcp` Python SDK with `FastMCP` for declarative tool definitions

### 2.2 Python MCP Server

**Purpose**: Execute Python scripts inside the sandbox container.

**Tools**:

| Tool Name | Description | Input Parameters | Output |
|-----------|-------------|------------------|--------|
| `execute_python` | Execute a Python script | `code: str`, `timeout: int = 30` | stdout, stderr, exit code |
| `execute_python_file` | Execute an existing Python file | `filename: str`, `timeout: int = 30` | stdout, stderr, exit code |

**Execution flow**:

1. Receive `code` string from Cursor
2. Write code to a temporary file in the workspace (`/workspace/_tmp_script.py`)
3. Execute via `docker exec sandbox python /workspace/_tmp_script.py`
4. Capture stdout and stderr
5. Return results as structured MCP response
6. Clean up temporary file

**Pre-installed libraries in container**: `pandas`, `numpy`, `matplotlib`,
`seaborn`, `scikit-learn`, `openpyxl`, `requests` (for data download).

### 2.3 Bash MCP Server

**Purpose**: Execute shell commands inside the sandbox container.

**Tools**:

| Tool Name | Description | Input Parameters | Output |
|-----------|-------------|------------------|--------|
| `execute_bash` | Execute a bash command | `command: str`, `timeout: int = 15` | stdout, stderr, exit code |

**Command validation**:

The server MUST validate commands before execution using an allowlist/blocklist
approach:

- **Blocked patterns**: `rm -rf /`, `mkfs`, `dd`, `:(){ :|:& };:`,
  `chmod 777`, `shutdown`, `reboot`, `mount`, `umount`,
  any command with `sudo`, `su`, `chown`, pipe to `/dev/sda` or similar
- **Blocked binaries**: `curl`, `wget`, `nc`, `ncat`, `ssh`, `scp`
  (network tools — containers have `--network=none` but defense in depth)
- **Validation strategy**: Regex-based pattern matching on the command string
  before forwarding to Docker

### 2.4 File MCP Server

**Purpose**: Read, write, and list files in the workspace.

**Tools**:

| Tool Name | Description | Input Parameters | Output |
|-----------|-------------|------------------|--------|
| `read_file` | Read file contents | `path: str` | file contents (text) |
| `write_file` | Write content to a file | `path: str`, `content: str` | success/failure message |
| `list_files` | List files in a directory | `path: str = "/"` | list of file names with sizes |

**Security**:

- All paths MUST be resolved relative to the workspace root
- Path traversal MUST be blocked (reject `..`, absolute paths outside workspace)
- File operations execute via `docker exec` to ensure they stay within the
  container's filesystem view

### 2.5 Common MCP Server Pattern

Every MCP server follows the same structure:

```python
from mcp.server.fastmcp import FastMCP

mcp = FastMCP("server-name")

@mcp.tool()
async def tool_name(param: str) -> str:
    """Tool description for Cursor to understand."""
    result = await sandbox_runtime.execute(...)
    return result

if __name__ == "__main__":
    mcp.run(transport="stdio")
```

---

## 3. Sandbox Runtime Design

### 3.1 Purpose

The Sandbox Runtime is the shared orchestration layer that all MCP servers use
to interact with Docker. It encapsulates container management, command
execution, and result capture.

### 3.2 Core Components

```
sandbox/runtime/
├── __init__.py
├── container.py      # Container lifecycle management
├── executor.py       # Command execution inside container
├── validator.py      # Command validation and security checks
└── config.py         # Configuration constants
```

### 3.3 Container Manager (`container.py`)

Responsibilities:

- **Start**: Create and start the sandbox container if not running
- **Reuse**: Return existing container if already running
- **Health check**: Verify container is responsive before executing
- **Restart**: Recreate container if it becomes unresponsive
- **Cleanup**: Remove container on graceful shutdown

Key behavior:

- Uses a **singleton pattern** — one container shared across all MCP servers
- Container name: `sandbox-mcp-container`
- Container is created on first use (lazy initialization)
- Implements `async` interface for non-blocking operations

### 3.4 Executor (`executor.py`)

Responsibilities:

- Execute commands inside the running container
- Capture stdout and stderr separately
- Enforce timeout limits
- Return structured `ExecutionResult` objects

```python
@dataclass
class ExecutionResult:
    stdout: str
    stderr: str
    exit_code: int
    timed_out: bool
    execution_time_ms: int
```

### 3.5 Validator (`validator.py`)

Responsibilities:

- Validate bash commands against blocked patterns
- Validate file paths against traversal attacks
- Provide clear rejection messages when validation fails

### 3.6 Configuration (`config.py`)

All tunables in one place:

```python
CONTAINER_NAME = "sandbox-mcp-container"
CONTAINER_IMAGE = "sandbox-mcp-tools:latest"
WORKSPACE_HOST_PATH = "./workspaces/default"
WORKSPACE_CONTAINER_PATH = "/workspace"

# Resource limits
MEMORY_LIMIT = "512m"
CPU_LIMIT = 0.5
PIDS_LIMIT = 64

# Execution limits
DEFAULT_TIMEOUT_PYTHON = 30   # seconds
DEFAULT_TIMEOUT_BASH = 15     # seconds
MAX_OUTPUT_SIZE = 100_000     # characters
```

---

## 4. Docker Container Setup

### 4.1 Dockerfile

The sandbox image MUST be purpose-built with:

```dockerfile
FROM python:3.11-slim

# Create non-root user
RUN useradd -m -s /bin/bash sandbox

# Install data science libraries
RUN pip install --no-cache-dir \
    pandas numpy matplotlib seaborn scikit-learn openpyxl

# Set working directory
WORKDIR /workspace

# Switch to non-root user
USER sandbox

CMD ["sleep", "infinity"]
```

### 4.2 Container Launch Configuration

```python
container_config = {
    "image": "sandbox-mcp-tools:latest",
    "name": "sandbox-mcp-container",
    "detach": True,
    "mem_limit": "512m",
    "nano_cpus": 500_000_000,       # 0.5 CPUs
    "pids_limit": 64,
    "network_mode": "none",          # No network access
    "read_only": True,               # Read-only root filesystem
    "tmpfs": {"/tmp": "size=64m"},   # Writable /tmp for Python
    "volumes": {
        host_workspace_path: {
            "bind": "/workspace",
            "mode": "rw"
        }
    },
    "user": "sandbox",
}
```

### 4.3 Security Hardening Checklist

| Control | Implementation |
|---------|---------------|
| Non-root user | `USER sandbox` in Dockerfile |
| Network isolation | `--network=none` |
| Memory limit | `--memory=512m` |
| CPU limit | `--cpus=0.5` |
| Process limit | `--pids-limit=64` |
| Read-only rootfs | `--read-only` |
| Writable tmp | `--tmpfs /tmp:size=64m` |
| Workspace mount | Single bind mount to `/workspace` |
| No privileged mode | Default (not set) |
| No capabilities | Default (Docker drops most by default) |

---

## 5. Project Folder Structure

```
sandbox-mcp-tools/
├── docs/
│   └── implementation-plan.md      # This document
│
├── src/
│   ├── __init__.py
│   │
│   ├── servers/                    # MCP Server implementations
│   │   ├── __init__.py
│   │   ├── python_server.py        # Python execution MCP server
│   │   ├── bash_server.py          # Bash execution MCP server
│   │   └── file_server.py          # File operations MCP server
│   │
│   └── sandbox/                    # Sandbox runtime (shared)
│       ├── __init__.py
│       ├── container.py            # Container lifecycle management
│       ├── executor.py             # Command execution
│       ├── validator.py            # Command & path validation
│       └── config.py               # Configuration constants
│
├── docker/
│   └── Dockerfile                  # Sandbox container image
│
├── workspaces/                     # Host-side workspace directory
│   └── default/                    # Default workspace (mounted into container)
│
├── tests/
│   ├── __init__.py
│   ├── unit/
│   │   ├── test_validator.py       # Command validation tests
│   │   ├── test_executor.py        # Executor logic tests
│   │   └── test_config.py          # Configuration tests
│   ├── integration/
│   │   ├── test_python_server.py   # Python MCP server integration tests
│   │   ├── test_bash_server.py     # Bash MCP server integration tests
│   │   ├── test_file_server.py     # File MCP server integration tests
│   │   └── test_container.py       # Container lifecycle tests
│   └── conftest.py                 # Shared test fixtures
│
├── pyproject.toml                  # Project metadata and dependencies
├── README.md                       # Project documentation
├── .gitignore
└── LICENSE
```

---

## 6. Development Phases

### Phase 1: Project Foundation
**Goal**: Establish project structure, dependencies, and build tooling.

| Task | Description |
|------|-------------|
| 1.1 | Initialize `pyproject.toml` with dependencies (`mcp[cli]`, `docker`, `pytest`, `pytest-asyncio`) |
| 1.2 | Create project folder structure (`src/`, `tests/`, `docker/`, `docs/`) |
| 1.3 | Create `.gitignore` for Python projects |
| 1.4 | Set up `README.md` with project overview |
| 1.5 | Configure `pytest` for unit and integration test directories |

**Deliverable**: A buildable Python project with all dependencies installable.

---

### Phase 2: Docker Sandbox
**Goal**: Build and configure the sandbox container image.

| Task | Description |
|------|-------------|
| 2.1 | Write `docker/Dockerfile` with Python 3.11, non-root user, data science libraries |
| 2.2 | Create `workspaces/default/` directory with `.gitkeep` |
| 2.3 | Implement `src/sandbox/config.py` with all container configuration constants |
| 2.4 | Implement `src/sandbox/container.py` with Docker SDK container lifecycle (start, reuse, health check, cleanup) |
| 2.5 | Write integration test: build image, start container, exec `echo hello`, verify output, stop container |

**Deliverable**: Working Docker sandbox that can be started, used for
execution, and stopped programmatically.

---

### Phase 3: Sandbox Runtime
**Goal**: Implement the command execution and validation layer.

| Task | Description |
|------|-------------|
| 3.1 | Implement `src/sandbox/executor.py` with async command execution, stdout/stderr capture, timeout handling |
| 3.2 | Implement `src/sandbox/validator.py` with command blocklist and path traversal prevention |
| 3.3 | Write unit tests for validator (blocked commands, allowed commands, path traversal attempts) |
| 3.4 | Write integration tests for executor (Python script execution, bash command execution, timeout behavior) |

**Deliverable**: A tested runtime that can execute commands inside Docker and
reject dangerous inputs.

---

### Phase 4: Python MCP Server
**Goal**: First MCP server — Python code execution.

| Task | Description |
|------|-------------|
| 4.1 | Implement `src/servers/python_server.py` with `execute_python` and `execute_python_file` tools using FastMCP |
| 4.2 | Handle script file creation, execution, result capture, and cleanup |
| 4.3 | Add structured logging for executions |
| 4.4 | Write integration test: start server, send tool call, verify execution result |
| 4.5 | Test with Cursor: configure MCP server in Cursor settings, execute a Python script through Cursor AI |

**Deliverable**: Working Python MCP server that Cursor can use to run Python
code in the sandbox.

---

### Phase 5: Bash MCP Server
**Goal**: Bash command execution with security validation.

| Task | Description |
|------|-------------|
| 5.1 | Implement `src/servers/bash_server.py` with `execute_bash` tool using FastMCP |
| 5.2 | Integrate command validator for blocked command detection |
| 5.3 | Add structured logging |
| 5.4 | Write unit tests for command validation edge cases |
| 5.5 | Write integration test: allowed commands succeed, blocked commands rejected |
| 5.6 | Test with Cursor |

**Deliverable**: Working Bash MCP server with command validation.

---

### Phase 6: File MCP Server
**Goal**: Safe file read/write/list operations.

| Task | Description |
|------|-------------|
| 6.1 | Implement `src/servers/file_server.py` with `read_file`, `write_file`, `list_files` tools |
| 6.2 | Integrate path validator for traversal prevention |
| 6.3 | Add structured logging |
| 6.4 | Write unit tests for path validation |
| 6.5 | Write integration tests for file operations |
| 6.6 | Test with Cursor |

**Deliverable**: Working File MCP server with path safety guarantees.

---

### Phase 7: Polish and Documentation
**Goal**: Production readiness and documentation.

| Task | Description |
|------|-------------|
| 7.1 | Write comprehensive `README.md` (architecture, setup, usage, Cursor configuration) |
| 7.2 | Add docstrings to all public modules and functions |
| 7.3 | Create a quickstart guide for first-time setup |
| 7.4 | End-to-end validation: all three MCP servers running in Cursor simultaneously |
| 7.5 | Performance validation: measure execution latency for typical operations |

**Deliverable**: A fully documented, tested, and validated project.

---

## 7. Recommended Development Workflow

### 7.1 Local Development Setup

```bash
# 1. Clone and set up the project
git clone <repo-url>
cd sandbox-mcp-tools

# 2. Create virtual environment
python -m venv .venv
.venv/Scripts/activate      # Windows
# source .venv/bin/activate  # Linux/macOS

# 3. Install dependencies
pip install -e ".[dev]"

# 4. Build the sandbox Docker image
docker build -t sandbox-mcp-tools:latest -f docker/Dockerfile .

# 5. Run tests
pytest tests/unit/           # Fast, no Docker needed
pytest tests/integration/    # Requires Docker running
```

### 7.2 Cursor MCP Configuration

Add each server to Cursor's MCP settings
(`Cursor Settings > Features > MCP > Add New MCP Server`):

**Python Server**:
```json
{
  "name": "sandbox-python",
  "type": "stdio",
  "command": "python",
  "args": ["src/servers/python_server.py"]
}
```

**Bash Server**:
```json
{
  "name": "sandbox-bash",
  "type": "stdio",
  "command": "python",
  "args": ["src/servers/bash_server.py"]
}
```

**File Server**:
```json
{
  "name": "sandbox-file",
  "type": "stdio",
  "command": "python",
  "args": ["src/servers/file_server.py"]
}
```

### 7.3 Development Order

Follow the phases sequentially:

```
Foundation → Docker Sandbox → Runtime → Python Server → Bash Server → File Server → Polish
    P1            P2             P3          P4             P5             P6          P7
```

Each phase builds on the previous one. Within a phase, tasks should be
completed in order unless marked as parallelizable.

### 7.4 Testing Strategy

| Layer | What | How | When |
|-------|------|-----|------|
| Unit | Validator logic, config parsing | `pytest tests/unit/` | Every change |
| Integration | Container lifecycle, MCP tool execution | `pytest tests/integration/` | Per phase |
| Manual | Cursor ↔ MCP server interaction | Run in Cursor, invoke tools | Per MCP server phase |

### 7.5 Git Workflow

- Create a branch per phase (e.g., `phase-2/docker-sandbox`)
- Commit after each task or logical group of tasks
- Write clear commit messages referencing the phase and task
  (e.g., `feat(sandbox): implement container lifecycle manager [P2-T2.4]`)

---

## 8. Dependencies

### Runtime Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `mcp[cli]` | `>=1.0` | MCP Python SDK with FastMCP |
| `docker` | `>=7.0` | Docker SDK for Python |

### Development Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `pytest` | `>=8.0` | Test framework |
| `pytest-asyncio` | `>=0.23` | Async test support |

### Container Dependencies (inside Docker image)

| Package | Purpose |
|---------|---------|
| `pandas` | Data manipulation |
| `numpy` | Numerical computing |
| `matplotlib` | Chart generation |
| `seaborn` | Statistical visualization |
| `scikit-learn` | Machine learning |
| `openpyxl` | Excel file support |


