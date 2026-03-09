import pytest
import pytest_asyncio
import asyncio
from sandbox_mcp_tools.sandbox.container import ContainerManager

@pytest_asyncio.fixture
async def sandbox():
    manager = ContainerManager()
    await manager.cleanup()
    container = await manager.start()
    yield container
    await manager.cleanup()

@pytest.mark.asyncio
async def test_security_non_root_user(sandbox):
    exit_code, output = sandbox.exec_run("whoami")
    assert exit_code == 0
    assert output.decode().strip() == "sandbox"

@pytest.mark.asyncio
async def test_security_network_disabled(sandbox):
    # Try to reach outside network (Google DNS)
    # The command should fail (non-zero exit code)
    exit_code, output = sandbox.exec_run("ping -c 1 -W 1 8.8.8.8")
    assert exit_code != 0

@pytest.mark.asyncio
async def test_security_memory_limit(sandbox):
    # Verify memory limit is set to 512MB
    # Note: Inside the container, we can check cgroup limits
    # V1: /sys/fs/cgroup/memory/memory.limit_in_bytes
    # V2: /sys/fs/cgroup/memory.max
    # Or rely on Docker inspect on the container object
    sandbox.reload()
    mem_limit = sandbox.attrs["HostConfig"]["Memory"]
    assert mem_limit == 512 * 1024 * 1024 # 512MB

@pytest.mark.asyncio
async def test_security_cpu_limit(sandbox):
    sandbox.reload()
    nano_cpus = sandbox.attrs["HostConfig"]["NanoCpus"]
    assert nano_cpus == 500_000_000 # 0.5 CPUs

@pytest.mark.asyncio
async def test_security_pids_limit(sandbox):
    sandbox.reload()
    pids_limit = sandbox.attrs["HostConfig"]["PidsLimit"]
    assert pids_limit == 64

@pytest.mark.asyncio
async def test_security_read_only_rootfs(sandbox):
    # Attempt to create a file in the root filesystem
    # Specifically in the user's home so it doesn't fail with Permission Denied first
    exit_code, output = sandbox.exec_run("touch /home/sandbox/test.txt")
    assert exit_code != 0
    assert "Read-only file system" in output.decode()

@pytest.mark.asyncio
async def test_security_writable_tmpfs(sandbox):
    # Attempt to create a file in /tmp which should be a tmpfs
    exit_code, output = sandbox.exec_run("touch /tmp/test.txt")
    assert exit_code == 0

@pytest.mark.asyncio
async def test_security_writable_workspace(sandbox):
    # Attempt to create a file in /workspace
    exit_code, output = sandbox.exec_run("touch /workspace/test.txt")
    assert exit_code == 0
    
    # Cleanup
    sandbox.exec_run("rm /workspace/test.txt")
