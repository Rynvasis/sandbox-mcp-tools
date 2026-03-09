import pytest
from sandbox_mcp_tools.sandbox.executor import ExecutionResult
from sandbox_mcp_tools.servers.python_server import format_result

def test_format_result_success():
    """Test formatting a successful execution result."""
    result = ExecutionResult(
        stdout="Hello, World!\n",
        stderr="",
        exit_code=0,
        timed_out=False,
        execution_time_ms=150
    )
    formatted = format_result(result)
    assert "[stdout]\nHello, World!\n" in formatted
    assert "[stderr]\n\n" in formatted
    assert "[exit_code]\n0" in formatted
    assert "[execution_time]\n150ms" in formatted
    assert "[timed_out]\nFalse" in formatted

def test_format_result_failure():
    """Test formatting a failed execution result."""
    result = ExecutionResult(
        stdout="",
        stderr="Traceback (most recent call last):\n  File \"<string>\", line 1\nSyntaxError: unexpected EOF while parsing\n",
        exit_code=1,
        timed_out=False,
        execution_time_ms=42
    )
    formatted = format_result(result)
    assert "[stdout]\n\n" in formatted
    assert "[stderr]\nTraceback" in formatted
    assert "SyntaxError: unexpected EOF" in formatted
    assert "[exit_code]\n1" in formatted
    assert "[execution_time]\n42ms" in formatted
    assert "[timed_out]\nFalse" in formatted

def test_format_result_timeout():
    """Test formatting a timed-out execution result."""
    result = ExecutionResult(
        stdout="Partial output...\n",
        stderr="",
        exit_code=124,
        timed_out=True,
        execution_time_ms=5000
    )
    formatted = format_result(result)
    assert "[stdout]\nPartial output...\n" in formatted
    assert "[stderr]\n\n" in formatted
    assert "[exit_code]\n124" in formatted
    assert "[execution_time]\n5000ms" in formatted
    assert "[timed_out]\nTrue" in formatted

