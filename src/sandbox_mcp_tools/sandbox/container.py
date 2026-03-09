"""Container lifecycle manager for the Docker sandbox."""

import asyncio
import logging
import time
from typing import Optional

import docker
from docker.errors import DockerException, ImageNotFound
from docker.models.containers import Container

from sandbox_mcp_tools.sandbox import config

logger = logging.getLogger("sandbox_mcp_tools.sandbox.container")

class ContainerManager:
    """Manages the full lifecycle of the sandbox container."""

    def __init__(self) -> None:
        """Initialize the container manager and Docker client."""
        try:
            self._client = docker.from_env()
        except DockerException as e:
            logger.error(f"Failed to connect to Docker daemon: {e}")
            raise RuntimeError(f"Docker daemon must be running: {e}") from e
        
        self._container: Optional[Container] = None

    async def start(self) -> Container:
        """Start a new container or reuse an existing one.
        
        Returns:
            The running Docker container instance.
            
        Raises:
            RuntimeError: If Docker daemon is unavailable or image missing.
        """
        def _check_existing() -> bool:
            if self._container is not None:
                try:
                    self._container.reload()
                    if self._container.status == "running":
                        return True
                except docker.errors.NotFound:
                    pass
            return False

        if await asyncio.to_thread(_check_existing):
            logger.debug("Reusing existing container instance.")
            return self._container

        def _do_start() -> Container:
            try:
                # Check for existing stale container
                try:
                    existing = self._client.containers.get(config.CONTAINER_NAME)
                    if existing.status == "running":
                        logger.info(f"Attached to running container {config.CONTAINER_NAME}.")
                        return existing
                    logger.info(f"Removing stale stopped container {config.CONTAINER_NAME}.")
                    existing.remove(force=True)
                except docker.errors.NotFound:
                    pass

                # Verify image exists
                try:
                    self._client.images.get(config.CONTAINER_IMAGE)
                except ImageNotFound:
                    logger.error(f"Sandbox image {config.CONTAINER_IMAGE} not found.")
                    raise RuntimeError(
                        f"Docker image {config.CONTAINER_IMAGE} not found. "
                        f"Please build it first: cd docker && docker build -t {config.CONTAINER_IMAGE} ."
                    )

                logger.info(f"Starting new container {config.CONTAINER_NAME}.")
                
                # Configure volume mount
                volumes = {
                    config.WORKSPACE_HOST_PATH: {
                        "bind": config.WORKSPACE_CONTAINER_PATH,
                        "mode": "rw"
                    }
                }

                # Start container with security limits
                cont = self._client.containers.run(
                    image=config.CONTAINER_IMAGE,
                    name=config.CONTAINER_NAME,
                    detach=True,
                    network_mode=config.NETWORK_MODE,
                    mem_limit=config.MEMORY_LIMIT,
                    nano_cpus=int(config.CPU_LIMIT * 1_000_000_000),
                    pids_limit=config.PIDS_LIMIT,
                    read_only=config.READ_ONLY,
                    tmpfs={config.TMPFS_CONTAINER_PATH: f"size={config.TMPFS_SIZE}"} if hasattr(config, 'TMPFS_CONTAINER_PATH') else {"/tmp": f"size={config.TMPFS_SIZE}"},
                    volumes=volumes,
                )
                
                # Wait for container to reach running state
                for _ in range(20):
                    cont.reload()
                    if cont.status == "running":
                        break
                    time.sleep(0.1)
                
                return cont
            except DockerException as e:
                logger.error(f"Failed to start container: {e}")
                raise RuntimeError(f"Failed to start container: {e}") from e

        self._container = await asyncio.to_thread(_do_start)
        return self._container

    async def health_check(self) -> bool:
        """Verify the container is responsive.
        
        Returns:
            True if health check succeeds, False otherwise.
        """
        if self._container is None:
            logger.debug("Health check failed: no container instance.")
            return False

        def _do_check() -> bool:
            try:
                # Reload container state implicitly via get to catch external deletion
                cont = self._client.containers.get(config.CONTAINER_NAME)
                if cont.status != "running":
                    logger.warning(f"Container status is {cont.status}, not running.")
                    return False
                
                exit_code, _ = self._container.exec_run("echo ok")
                return exit_code == 0
            except Exception as e:
                logger.warning(f"Health check failed due to error: {e}")
                return False

        try:
            return await asyncio.wait_for(
                asyncio.to_thread(_do_check), 
                timeout=config.HEALTH_CHECK_TIMEOUT
            )
        except asyncio.TimeoutError:
            logger.warning(f"Health check timed out after {config.HEALTH_CHECK_TIMEOUT}s.")
            return False

    async def restart(self) -> Container:
        """Remove and recreate the container.
        
        Returns:
            The new running Docker container instance.
        """
        logger.info("Restarting container.")
        await self.cleanup()
        return await self.start()

    async def cleanup(self) -> None:
        """Stop and remove the container."""
        if self._container is None:
            # Also try to clean up by name in case of lingering state
            def _clean_by_name() -> None:
                try:
                    cont = self._client.containers.get(config.CONTAINER_NAME)
                    logger.info(f"Removing named container {config.CONTAINER_NAME}.")
                    cont.remove(force=True)
                except Exception:
                    pass
            await asyncio.to_thread(_clean_by_name)
            return

        def _do_cleanup() -> None:
            try:
                logger.info(f"Stopping and removing container {config.CONTAINER_NAME}.")
                self._container.remove(force=True)
            except Exception as e:
                logger.error(f"Error during container cleanup: {e}")

        await asyncio.to_thread(_do_cleanup)
        self._container = None
