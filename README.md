# Sandbox MCP Tools

Sandbox MCP Tools is a learning-oriented project that provides **MCP tool servers**
and a shared **sandbox runtime** for running tools in isolated environments.

## Prerequisites

- Python 3.10+
- pip (included with Python)
- Docker (not required for this foundation phase, but used in later phases)

## Setup

```bash
# Clone
git clone https://github.com/Rynvasis/sandbox-mcp-tools.git
cd sandbox-mcp-tools

# Create a virtual environment
python -m venv .venv

# Activate it
# Windows:
.venv\Scripts\activate
# Linux/macOS:
source .venv/bin/activate

# Install (editable + dev dependencies)
pip install -e ".[dev]"
```

## Running tests

```bash
# All tests
pytest

# Unit tests only
pytest tests/unit/

# Integration tests only
pytest tests/integration/
```

## Project structure

```text
src/sandbox_mcp_tools/     # Importable package root
├── servers/               # MCP server implementations (future phases)
└── sandbox/               # Sandbox runtime code (future phases)

tests/
├── unit/                  # Fast tests (no Docker)
└── integration/           # Slower tests (Docker in future phases)

docker/                    # Docker assets (future phases)
workspaces/default/        # Default sandbox workspace
```

