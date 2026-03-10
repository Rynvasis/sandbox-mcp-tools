"""Shared response formatting utilities for MCP servers.

This module provides helpers to format sandbox execution results as
structured multi-section text for consumption by MCP clients.
"""

from __future__ import annotations

from sandbox.executor import ExecutionResult


def format_result(result: ExecutionResult) -> str:
    """Format the execution result as structured multi-section text.

    Args:
        result: The execution result dataclass.

    Returns:
        A formatted string with labeled sections for Cursor to consume.
    """
    return (
        f"[stdout]\n{result.stdout}\n\n"
        f"[stderr]\n{result.stderr}\n\n"
        f"[exit_code]\n{result.exit_code}\n\n"
        f"[execution_time]\n{result.execution_time_ms}ms\n\n"
        f"[timed_out]\n{result.timed_out}"
    )

