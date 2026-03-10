import pytest

from servers.bash_server import mcp


def test_bash_server_mcp_name() -> None:
    """Ensure the Bash MCP server is configured with the correct name."""
    assert mcp.name == "sandbox-bash"

