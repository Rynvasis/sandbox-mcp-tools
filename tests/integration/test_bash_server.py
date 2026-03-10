"""Integration tests for the Bash MCP Server tools."""

import pytest
import pytest_asyncio

from sandbox.container import ContainerManager
from servers.bash_server import execute_bash


pytestmark = pytest.mark.asyncio


@pytest_asyncio.fixture(autouse=True)
async def ensure_sandbox_running():
    """Ensure the sandbox container is running before Bash server tests."""
    manager = ContainerManager()
    await manager.start()
    yield


@pytest.mark.asyncio
async def test_execute_bash_blocked_destructive():
    """T012: Block obviously destructive commands like rm -rf /."""
    result = await execute_bash("rm -rf /")
    assert "Error:" in result


@pytest.mark.asyncio
async def test_execute_bash_blocked_sudo():
    """T013: Block commands that attempt privilege escalation."""
    result = await execute_bash("sudo apt install something")
    assert "Error:" in result


@pytest.mark.asyncio
async def test_execute_bash_blocked_network():
    """T014: Block commands that use network tools like curl."""
    result = await execute_bash("curl https://example.com")
    assert "Error:" in result


@pytest.mark.asyncio
async def test_execute_bash_empty():
    """T015: Reject empty or whitespace-only commands."""
    result = await execute_bash("   \n")
    assert "Error: Bash command cannot be empty." in result


@pytest.mark.asyncio
async def test_execute_bash_success():
    """T016: Execute a simple echo command successfully."""
    result = await execute_bash("echo hello")
    assert "[stdout]\nhello\n" in result
    assert "[exit_code]\n0" in result


@pytest.mark.asyncio
async def test_execute_bash_stderr():
    """T017: Capture standard error output correctly."""
    result = await execute_bash("echo error >&2")
    assert "[stderr]\nerror\n" in result


@pytest.mark.asyncio
async def test_execute_bash_nonzero_exit():
    """T018: Preserve non-zero exit codes from the command."""
    result = await execute_bash("exit 42")
    assert "[exit_code]\n42" in result


@pytest.mark.asyncio
async def test_execute_bash_timeout():
    """T019: Mark long-running commands as timed out."""
    result = await execute_bash("sleep 30", timeout=2)
    assert "[timed_out]\nTrue" in result


@pytest.mark.asyncio
async def test_execute_bash_multiline():
    """T020: Support commands that produce multi-line stdout."""
    result = await execute_bash("echo a && echo b")
    assert "[stdout]\na\nb\n" in result

