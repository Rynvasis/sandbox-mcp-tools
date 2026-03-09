"""Integration tests for the Python MCP Server tools."""

import pytest
import pytest_asyncio
import uuid
from pathlib import Path

from sandbox_mcp_tools.sandbox.container import ContainerManager
from sandbox_mcp_tools.servers.python_server import mcp

pytestmark = pytest.mark.asyncio

@pytest_asyncio.fixture(autouse=True)
async def ensure_sandbox_running():
    """Ensure the sandbox container is running before tests."""
    manager = ContainerManager()
    await manager.start()
    # Clean up any test scripts
    from sandbox_mcp_tools.sandbox.executor import Executor
    await Executor().execute("rm -f /workspace/test_*.py")
    yield

from sandbox_mcp_tools.servers.python_server import execute_python, execute_python_file

@pytest.mark.asyncio
async def test_execute_python_success():
    """T015: Test successful inline python execution."""
    result = await execute_python("print('hello')")
    assert "[stdout]\nhello\n" in result
    assert "[exit_code]\n0" in result
    assert "[stderr]\n\n" in result

@pytest.mark.asyncio
async def test_execute_python_exception():
    """T016: Test inline python with exception."""
    result = await execute_python("1 / 0")
    assert "ZeroDivisionError: division by zero" in result
    assert "[exit_code]\n1" in result

@pytest.mark.asyncio
async def test_execute_python_pandas():
    """T017: Test inline python with pandas (pre-installed in container)."""
    code = "import pandas as pd\nprint(pd.DataFrame({'a': [1,2]}).shape)"
    result = await execute_python(code)
    assert "(2, 1)" in result
    assert "[exit_code]\n0" in result

@pytest.mark.asyncio
async def test_execute_python_timeout():
    """T018: Test inline python timeout."""
    result = await execute_python("import time; time.sleep(10)", timeout=2)
    assert "[timed_out]\nTrue" in result
    assert "[exit_code]\n124" in result

@pytest.mark.asyncio
async def test_execute_python_empty():
    """T019: Test inline python with empty code string."""
    result = await execute_python("   \n  ")
    assert "Error: Python code to execute cannot be empty" in result

@pytest.mark.asyncio
async def test_execute_python_file_success():
    """T024: Test successful file execution."""
    # Write test file
    from sandbox_mcp_tools.sandbox.executor import Executor
    await Executor().execute("echo \"print('from file')\" > /workspace/test_file.py")
    
    result = await execute_python_file("test_file.py")
    assert "[stdout]\nfrom file\n" in result
    assert "[exit_code]\n0" in result

@pytest.mark.asyncio
async def test_execute_python_file_not_found():
    """T025: Test file not found error."""
    result = await execute_python_file("nonexistent.py")
    assert "Error: File 'nonexistent.py' not found" in result

@pytest.mark.asyncio
async def test_execute_python_file_traversal():
    """T026: Test path traversal rejection."""
    result = await execute_python_file("../../etc/passwd")
    assert "Error:" in result
    assert "Path validation failed" in result or "traversal" in result
