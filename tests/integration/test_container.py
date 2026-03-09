import pytest
import asyncio
from sandbox.container import ContainerManager

@pytest.mark.asyncio
async def test_container_lifecycle():
    manager = ContainerManager()
    
    # 1. Start creates container
    container = await manager.start()
    assert container is not None
    assert container.status == "running"
    
    # 2. Exec returns correct output
    exit_code, output = container.exec_run("echo hello")
    assert exit_code == 0
    assert output.decode().strip() == "hello"
    
    # 3. Second start reuses container
    container2 = await manager.start()
    assert container.id == container2.id
    
    # 4. Health check passes
    is_healthy = await manager.health_check()
    assert is_healthy is True
    
    # 5. Restart creates a new container
    container3 = await manager.restart()
    assert container.id != container3.id
    assert container3.status == "running"
    
    # 6. Cleanup removes container
    await manager.cleanup()
    
    # Verify container is gone
    is_healthy_after = await manager.health_check()
    assert is_healthy_after is False

@pytest.mark.asyncio
async def test_stale_container_handling():
    manager = ContainerManager()
    
    # Start and then kill the container outside the manager
    container = await manager.start()
    container.kill()
    
    # The manager should clean it up and start a new one
    new_container = await manager.start()
    assert new_container.id != container.id
    assert new_container.status == "running"
    
    await manager.cleanup()
