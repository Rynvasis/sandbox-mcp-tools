"""
File MCP server for sandbox-mcp-tools.

Exposes three tools over stdio using FastMCP:
- read_file: read file contents from the sandbox workspace
- write_file: write text content to files, creating parent directories
- list_files: list directory contents with sizes
"""

from __future__ import annotations

import base64
import logging
import os
import shlex
from typing import Optional

from mcp.server.fastmcp import FastMCP

from sandbox.executor import Executor
from sandbox.validator import validate_path


mcp = FastMCP(
    "sandbox-file",
    instructions="Read, write, and list files within the isolated sandbox workspace.",
)

logger = logging.getLogger(__name__)

_EXECUTOR = Executor()


def _validate_file_path(path: str, label: str) -> Optional[str]:
    """Validate a file or directory path using sandbox rules.

    Returns the resolved path string on success, or None on validation error.
    """
    if not path or path.strip() == "":
        logger.warning("%s path is empty", label)
        return None

    try:
        resolved = validate_path(path)
    except ValueError as exc:
        logger.warning("%s path validation failed: %s", label, exc)
        return None

    return resolved


@mcp.tool()
async def read_file(path: str) -> str:
    """Read the contents of a file inside the sandbox workspace."""
    if not path or not path.strip():
        logger.warning("read_file failed: path is empty")
        return "Error: File path cannot be empty."

    try:
        resolved = validate_path(path)
    except ValueError as exc:
        logger.warning("read_file blocked by validator", extra={"reason": str(exc)})
        return f"Error: {exc}"

    command = f"test -f {shlex.quote(resolved)} && cat {shlex.quote(resolved)}"
    result = await _EXECUTOR.execute(command)

    if result.exit_code != 0:
        logger.warning(
            "read_file failed: file not found",
            extra={"path": path, "resolved_path": resolved},
        )
        return f"Error: File '{path}' not found at {resolved}."

    logger.info(
        "read_file completed successfully",
        extra={"path": path, "resolved_path": resolved},
    )
    return result.stdout


@mcp.tool()
async def write_file(path: str, content: str) -> str:
    """Write text content to a file inside the sandbox workspace."""
    if not path or not path.strip():
        logger.warning("write_file failed: path is empty")
        return "Error: File path cannot be empty."

    try:
        resolved = validate_path(path)
    except ValueError as exc:
        logger.warning("write_file blocked by validator", extra={"reason": str(exc)})
        return f"Error: {exc}"

    encoded = base64.b64encode(content.encode("utf-8")).decode("ascii")
    parent_dir = os.path.dirname(resolved) or "/workspace"

    command = (
        f"mkdir -p {shlex.quote(parent_dir)} && "
        f"echo {shlex.quote(encoded)} | base64 -d > {shlex.quote(resolved)}"
    )

    logger.info(
        "write_file started",
        extra={"path": path, "resolved_path": resolved, "bytes": len(content.encode('utf-8'))},
    )

    result = await _EXECUTOR.execute(command)

    if result.exit_code != 0:
        logger.warning(
            "write_file failed",
            extra={
                "path": path,
                "resolved_path": resolved,
                "stderr": result.stderr,
                "exit_code": result.exit_code,
            },
        )
        stderr = result.stderr.strip()
        detail = f" {stderr}" if stderr else ""
        return f"Error: Failed to write file.{detail}"

    bytes_written = len(content.encode("utf-8"))
    logger.info(
        "write_file completed successfully",
        extra={
            "path": path,
            "resolved_path": resolved,
            "bytes_written": bytes_written,
        },
    )
    return f"Successfully wrote {bytes_written} bytes to {resolved}."


@mcp.tool()
async def list_files(path: str = "/") -> str:
    """List files and directories inside the sandbox workspace."""
    if not path or not path.strip():
        logger.warning("list_files failed: path is empty")
        return "Error: Directory path cannot be empty."

    if path == "/":
        resolved = "/workspace"
    else:
        try:
            resolved = validate_path(path)
        except ValueError as exc:
            logger.warning(
                "list_files blocked by validator",
                extra={"reason": str(exc)},
            )
            return f"Error: {exc}"

    command = f"test -d {shlex.quote(resolved)} && ls -lhA {shlex.quote(resolved)}"
    result = await _EXECUTOR.execute(command)

    if result.exit_code != 0:
        logger.warning(
            "list_files failed: directory not found or not a directory",
            extra={"path": path, "resolved_path": resolved},
        )
        return f"Error: Directory '{path}' not found at {resolved}."

    logger.info(
        "list_files completed successfully",
        extra={"path": path, "resolved_path": resolved},
    )
    return result.stdout


if __name__ == "__main__":
    mcp.run(transport="stdio")

