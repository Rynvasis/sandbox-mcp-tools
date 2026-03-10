from servers import file_server


def test_server_name():
    assert hasattr(file_server, "mcp")
    assert file_server.mcp.name == "sandbox-file"
