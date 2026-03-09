"""Sandbox execution engine for running commands inside the container."""

from dataclasses import dataclass

@dataclass(frozen=True)
class ExecutionResult:
    """Structured result from a sandbox command execution."""
    stdout: str
    stderr: str
    exit_code: int
    timed_out: bool
    execution_time_ms: int

import asyncio
import time
import logging
from .container import ContainerManager
from .config import MAX_OUTPUT_SIZE

logger = logging.getLogger(__name__)

class Executor:
    """Executes commands inside the isolated Docker sandbox."""
    
    def __init__(self):
        """Initialize the executor with a container manager instance."""
        self.container_manager = ContainerManager()
        
    async def execute(self, command: str, timeout: int = 30) -> ExecutionResult:
        """Run a command inside the sandbox and capture its output.
        
        Args:
            command: The bash command to execute. Must not contain blocked patterns.
            timeout: Maximum execution time in seconds (default 30).
            
        Returns:
            ExecutionResult mapping stdout, stderr, and exit status.
        """
        # Ensure container is running (FR-015)
        await self.container_manager.start()
        
        # Get the underlying docker.models.containers.Container object
        container = self.container_manager._container
        if not container:
            logger.error("Executor failed: container is not running")
            raise RuntimeError("Sandbox container is not running")

        start_time = time.monotonic()
        logger.info(
            "Executing command in sandbox", 
            extra={
                "command": command,
                "timeout": timeout
            }
        )
        
        # We need both exit code and output, demuxing stdout/stderr separate
        # Docker SDK exec_run is blocking, so we wrap it in an async thread
        def _blocking_exec():
            return container.exec_run(
                ['bash', '-c', command],
                demux=True,
                workdir='/workspace'
            )
            
        try:
            # Run the command with timeout wrapper
            exit_code, (out_bytes, err_bytes) = await asyncio.wait_for(
                asyncio.to_thread(_blocking_exec),
                timeout=timeout
            )
            
            # None typically means empty output in SDK
            out_bytes = out_bytes or b""
            err_bytes = err_bytes or b""
            
            stdout = out_bytes.decode('utf-8', errors='replace')
            stderr = err_bytes.decode('utf-8', errors='replace')
            timed_out = False
            
            # Truncate output if necessary (FR-006)
            if len(stdout) > MAX_OUTPUT_SIZE:
                truncated_count = len(stdout) - MAX_OUTPUT_SIZE
                stdout = stdout[:MAX_OUTPUT_SIZE] + f"\n... [truncated, {truncated_count} characters omitted]"
                
            if len(stderr) > MAX_OUTPUT_SIZE:
                truncated_count = len(stderr) - MAX_OUTPUT_SIZE
                stderr = stderr[:MAX_OUTPUT_SIZE] + f"\n... [truncated, {truncated_count} characters omitted]"
            
            logger.debug(
                "Command trace completed",
                extra={
                    "exit_code": exit_code,
                    "stdout_len": len(stdout),
                    "stderr_len": len(stderr)
                }
            )
            
        except asyncio.TimeoutError:
            # On timeout, we don't get partial output from SDK easily,
            # so we just mark timeout and return empty outputs.
            stdout = ""
            stderr = f"Command execution timed out after {timeout} seconds"
            exit_code = 124  # Standard timeout exit code
            timed_out = True
            
            logger.warning(
                "Command execution timed out",
                extra={
                    "command": command,
                    "timeout": timeout
                }
            )
            
            # We must kill the runaway process inside the container
            # Since the SDK doesn't give us the exec PID easily, we can use pkill
            # on the container. We run it in the background daemon thread.
            def _kill_runaway():
                try:
                     # This is a blunt instrument, but safe since the sandbox
                     # is isolated and only runs one command at a time per session
                     container.exec_run(['pkill', '-9', '-f', 'bash -c'])
                except Exception as e:
                     logger.debug(f"Error during timeout cleanup (can be ignored): {e}")
                     pass # Ignore errors during cleanup
                     
            asyncio.create_task(asyncio.to_thread(_kill_runaway))
            
        execution_time_ms = int((time.monotonic() - start_time) * 1000)
        
        logger.info(
            "Sandbox execution finished",
            extra={
                "execution_time_ms": execution_time_ms,
                "exit_code": exit_code,
                "timed_out": timed_out
            }
        )
        
        return ExecutionResult(
            stdout=stdout,
            stderr=stderr,
            exit_code=exit_code,
            timed_out=timed_out,
            execution_time_ms=execution_time_ms
        )


