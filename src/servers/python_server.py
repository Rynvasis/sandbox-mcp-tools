"""Python MCP Server for executing inline code and scripts in the Sandbox.

This module provides an MCP server (`sandbox-python`) with tools to
execute Python code safely within the Docker sandbox environment.
"""

from __future__ import annotations

import logging
from mcp.server.fastmcp import FastMCP

from sandbox import Executor, ExecutionResult, validate_command, validate_path
from sandbox.config import DEFAULT_TIMEOUT_PYTHON

mcp = FastMCP(
    "sandbox-python",
    instructions="Executes Python code safely within the isolated sandbox container."
)

def format_result(result: ExecutionResult) -> str:
    """Format the execution result as structured multi-section text.

    Args:
        result: The execution result dataclass.
        
    Returns:
        A formatted string with labeled sections for cursor to consume.
    """
    return (
        f"[stdout]\n{result.stdout}\n\n"
        f"[stderr]\n{result.stderr}\n\n"
        f"[exit_code]\n{result.exit_code}\n\n"
        f"[execution_time]\n{result.execution_time_ms}ms\n\n"
        f"[timed_out]\n{result.timed_out}"
    )

import uuid
import base64

logger = logging.getLogger(__name__)

@mcp.tool()
async def execute_python(code: str, timeout: int = DEFAULT_TIMEOUT_PYTHON) -> str:
    """Execute Python code within the isolated sandbox container.
    
    Args:
        code: Python source code to execute.
        timeout: Maximum execution time in seconds.
    """
    logger.info("Executing Python tool: execute_python", extra={"code_length": len(code)})
    
    if not code or not code.strip():
        logger.warning("execute_python failed: code is empty")
        return "Error: Python code to execute cannot be empty."
        
    executor = Executor()
    file_id = uuid.uuid4().hex[:8]
    temp_file = f"/tmp/_sandbox_{file_id}.py"
    
    # Safely write the code to the container using base64 decoding
    encoded_code = base64.b64encode(code.encode('utf-8')).decode('utf-8')
    write_cmd = f"echo {encoded_code} | base64 -d > {temp_file}"
    
    try:
        # Write the file
        write_result = await executor.execute(write_cmd, timeout=10)
        if write_result.exit_code != 0:
            logger.error("execute_python failed to write temp file")
            return f"Error: Failed to write temporary script to container.\n{write_result.stderr}"
            
        # Execute the script
        result = await executor.execute(f"python {temp_file}", timeout=timeout)
        logger.info("execute_python completed", extra={
            "exit_code": result.exit_code,
            "timed_out": result.timed_out,
            "execution_time_ms": result.execution_time_ms
        })
        return format_result(result)
    finally:
        # Clean up the file
        await executor.execute(f"rm -f {temp_file}", timeout=5)

@mcp.tool()
async def execute_python_file(filename: str, timeout: int = DEFAULT_TIMEOUT_PYTHON) -> str:
    """Execute a Python file that already exists in the container workspace.
    
    Args:
        filename: Path to the Python file relative to the workspace.
        timeout: Maximum execution time in seconds.
    """
    logger.info("Executing Python tool: execute_python_file", extra={"filename": filename})
    
    try:
        resolved_path = str(validate_path(filename))
    except ValueError as e:
        logger.warning(f"execute_python_file failed validation: {e}")
        return f"Error: {e}"
        
    executor = Executor()
    
    # Check if the file exists in the container
    test_result = await executor.execute(f"test -f {resolved_path}", timeout=5)
    if test_result.exit_code != 0:
        logger.warning(f"execute_python_file failed: file not found at {resolved_path}")
        return f"Error: File '{filename}' not found at {resolved_path} inside the workspace."
        
    # Execute the file
    result = await executor.execute(f"python {resolved_path}", timeout=timeout)
    logger.info("execute_python_file completed", extra={
        "exit_code": result.exit_code,
        "timed_out": result.timed_out,
        "execution_time_ms": result.execution_time_ms
    })
    return format_result(result)

if __name__ == "__main__":
    mcp.run(transport="stdio")
