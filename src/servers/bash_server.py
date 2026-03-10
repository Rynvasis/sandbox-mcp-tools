"""Bash MCP Server for executing commands in the Sandbox container."""

from __future__ import annotations

import logging

from mcp.server.fastmcp import FastMCP

from sandbox import Executor, validate_command
from sandbox.config import DEFAULT_TIMEOUT_BASH
from servers.response import format_result

mcp = FastMCP(
    "sandbox-bash",
    instructions="Executes bash commands safely within the isolated sandbox container.",
)

logger = logging.getLogger(__name__)


@mcp.tool()
async def execute_bash(command: str, timeout: int = DEFAULT_TIMEOUT_BASH) -> str:
    """Execute a bash command within the isolated sandbox container.

    Args:
        command: The bash command to execute inside the sandbox.
        timeout: Maximum execution time in seconds.
    """
    logger.info(
        "Executing Bash tool: execute_bash",
        extra={"command_preview": command[:100], "timeout": timeout},
    )

    if not command or not command.strip():
        logger.warning("execute_bash failed: command is empty")
        return "Error: Bash command cannot be empty."

    try:
        validate_command(command)
    except ValueError as exc:
        logger.warning("execute_bash blocked by validator", extra={"reason": str(exc)})
        return f"Error: {exc}"

    executor = Executor()
    result = await executor.execute(command, timeout=timeout)

    logger.info(
        "execute_bash completed",
        extra={
            "exit_code": result.exit_code,
            "timed_out": result.timed_out,
            "execution_time_ms": result.execution_time_ms,
        },
    )
    return format_result(result)


if __name__ == "__main__":
    mcp.run(transport="stdio")

