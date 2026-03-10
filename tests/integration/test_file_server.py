import pytest
import pytest_asyncio

from sandbox.container import ContainerManager
from sandbox.executor import Executor
from servers.file_server import list_files, read_file, write_file

pytestmark = pytest.mark.asyncio


@pytest_asyncio.fixture(autouse=True)
async def ensure_sandbox_running():
    """Ensure the sandbox container is running before file server tests."""
    manager = ContainerManager()
    await manager.start()
    yield


@pytest.mark.asyncio
async def test_read_file_success():
    executor = Executor()
    await executor.execute("echo 'hello world' > test_read.txt")

    result = await read_file("test_read.txt")
    assert result == "hello world\n"


@pytest.mark.asyncio
async def test_read_file_not_found():
    result = await read_file("does_not_exist.txt")
    assert "Error:" in result


@pytest.mark.asyncio
async def test_read_file_path_traversal():
    result = await read_file("../../etc/passwd")
    assert "Error:" in result


@pytest.mark.asyncio
async def test_read_file_empty_path():
    result = await read_file("")
    assert result == "Error: File path cannot be empty."


@pytest.mark.asyncio
async def test_read_file_subdirectory():
    executor = Executor()
    await executor.execute("mkdir -p subdir && echo 'subdata' > subdir/data.txt")

    result = await read_file("subdir/data.txt")
    assert result == "subdata\n"


@pytest.mark.asyncio
async def test_write_file_success():
    message = "write success"
    result = await write_file("write_success.txt", message)
    assert "Successfully wrote" in result

    contents = await read_file("write_success.txt")
    assert contents == message


@pytest.mark.asyncio
async def test_write_file_overwrite():
    await write_file("overwrite.txt", "first")
    await write_file("overwrite.txt", "second")

    contents = await read_file("overwrite.txt")
    assert contents == "second"


@pytest.mark.asyncio
async def test_write_file_creates_directories():
    result = await write_file("newdir/deep/file.txt", "nested")
    assert "Successfully wrote" in result

    contents = await read_file("newdir/deep/file.txt")
    assert contents == "nested"


@pytest.mark.asyncio
async def test_write_file_path_traversal():
    result = await write_file("../../tmp/evil.txt", "evil")
    assert "Error:" in result


@pytest.mark.asyncio
async def test_write_file_empty_content():
    result = await write_file("empty_content.txt", "")
    assert "Successfully wrote 0 bytes" in result


@pytest.mark.asyncio
async def test_write_file_empty_path():
    result = await write_file("", "data")
    assert result == "Error: File path cannot be empty."


@pytest.mark.asyncio
async def test_list_files_root():
    executor = Executor()
    await executor.execute("echo 'rootfile' > rootfile.txt")

    output = await list_files()
    assert "rootfile.txt" in output


@pytest.mark.asyncio
async def test_list_files_subdirectory():
    executor = Executor()
    await executor.execute(
        "mkdir -p listed && echo 'a' > listed/a.txt && echo 'b' > listed/b.txt"
    )

    output = await list_files("listed")
    assert "a.txt" in output
    assert "b.txt" in output


@pytest.mark.asyncio
async def test_list_files_not_found():
    output = await list_files("no_such_dir")
    assert "Error:" in output


@pytest.mark.asyncio
async def test_list_files_path_traversal():
    output = await list_files("../../")
    assert "Error:" in output


@pytest.mark.asyncio
async def test_list_files_on_file():
    executor = Executor()
    await executor.execute("echo 'single' > single.txt")

    output = await list_files("single.txt")
    assert "Error:" in output

