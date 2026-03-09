"""Integration tests for the executor component."""
import pytest
import asyncio
from sandbox.executor import Executor, ExecutionResult
from sandbox.config import MAX_OUTPUT_SIZE

@pytest.fixture
def executor():
    """Fixture to provide a clean Executor instance."""
    return Executor()

@pytest.mark.asyncio
async def test_execute_basic_command(executor):
    """Test basic command execution capturing stdout and exit code (T029)."""
    result = await executor.execute("echo 'hello world'")
    
    assert isinstance(result, ExecutionResult)
    assert result.stdout == "hello world\n"
    assert result.stderr == ""
    assert result.exit_code == 0
    assert result.timed_out is False
    assert result.execution_time_ms > 0

@pytest.mark.asyncio
async def test_execute_stderr_capture(executor):
    """Test standard error is captured separately (T030)."""
    # Write to stderr specifically
    result = await executor.execute("echo 'error message' >&2")
    
    assert result.stdout == ""
    assert result.stderr == "error message\n"
    assert result.exit_code == 0

@pytest.mark.asyncio
async def test_execute_non_zero_exit(executor):
    """Test non-zero exit code is recorded correctly (T031)."""
    result = await executor.execute("false")
    
    assert result.exit_code == 1
    assert result.stdout == ""
    
@pytest.mark.asyncio
async def test_execute_timeout_kills_process(executor):
    """Test that execution timeouts enforce strict limits (T036)."""
    # Sleep 10s, but we enforce a 2s timeout
    result = await executor.execute("sleep 10", timeout=2)
    
    assert result.timed_out is True
    assert result.exit_code == 124
    assert result.stdout == ""
    assert "Command execution timed out after 2 seconds" in result.stderr
    assert result.execution_time_ms >= 2000
    assert result.execution_time_ms < 3000

@pytest.mark.asyncio
async def test_execute_fast_command_under_timeout(executor):
    """Test fast commands aren't affected by large timeouts (T037)."""
    result = await executor.execute("echo 'quick'", timeout=30)
    
    assert result.timed_out is False
    assert result.stdout == "quick\n"
    assert result.exit_code == 0

@pytest.mark.asyncio
async def test_execute_output_truncation_stdout(executor):
    """Test oversized stdout is truncated correctly (T041)."""
    # Generate an output larger than MAX_OUTPUT_SIZE
    size_to_generate = MAX_OUTPUT_SIZE + 500
    cmd = f"python -c \"print('A' * {size_to_generate})\""
    
    result = await executor.execute(cmd)
    
    # +1 account for python's print \n
    expected_truncated_count = (size_to_generate + 1) - MAX_OUTPUT_SIZE 
    
    assert result.timed_out is False
    assert len(result.stdout) > MAX_OUTPUT_SIZE
    assert "A" * MAX_OUTPUT_SIZE in result.stdout
    assert f"... [truncated, {expected_truncated_count} characters omitted]" in result.stdout

@pytest.mark.asyncio
async def test_execute_output_truncation_stderr(executor):
    """Test oversized stderr is truncated correctly (T041)."""
    import sys
    # Generate large output directly to stderr
    size_to_generate = MAX_OUTPUT_SIZE + 500
    cmd = f"python -c \"import sys; sys.stderr.write('B' * {size_to_generate})\""
    
    result = await executor.execute(cmd)
    
    assert result.timed_out is False
    assert result.stdout == ""
    assert len(result.stderr) > MAX_OUTPUT_SIZE
    assert "B" * MAX_OUTPUT_SIZE in result.stderr
    assert f"... [truncated, 500 characters omitted]" in result.stderr

@pytest.mark.asyncio
async def test_execute_normal_output_not_truncated(executor):
    """Test normal outputs are fully returned (T042)."""
    cmd = f"python -c \"print('C' * {MAX_OUTPUT_SIZE - 100})\""
    result = await executor.execute(cmd)
    
    assert result.timed_out is False
    assert "truncated" not in result.stdout
    assert len(result.stdout) == MAX_OUTPUT_SIZE - 100 + 1 # +1 for newline

