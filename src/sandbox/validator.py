"""Command and path validation for sandbox security controls."""

import re

# Blocklist patterns (FR-008, FR-009)
# T008: Destructive commands
BLOCKED_DESTRUCTIVE = re.compile(r'\b(rm\s+-rf\s+/|mkfs\b|dd\s+if=)', re.IGNORECASE)

# T009: System commands
BLOCKED_SYSTEM = re.compile(r'\b(shutdown|reboot|mount|umount)\b', re.IGNORECASE)

# T010: Privilege escalation
BLOCKED_PRIVILEGE = re.compile(r'\b(sudo\b|su\s|chown\b)', re.IGNORECASE)

# T011: Permissions and fork bombs
BLOCKED_PERMISSIONS = re.compile(r'\bchmod\s+777\b', re.IGNORECASE)
BLOCKED_FORK_BOMB = re.compile(r':\(\)\{\s*:\|:&\s*\};:')

# T012: Network tools
BLOCKED_NETWORK = re.compile(r'\b(curl|wget|nc|ncat|ssh|scp)\b', re.IGNORECASE)

def validate_command(command: str) -> None:
    """Validate a bash command against security blocklists.
    
    Args:
        command: The raw bash command string to validate.
        
    Raises:
        ValueError: If the command matches a blocked pattern or is empty.
    """
    if not command or not command.strip():
        raise ValueError("Empty command")
        
    if BLOCKED_DESTRUCTIVE.search(command):
        raise ValueError("Command rejected: contains destructive pattern (rm -rf /, mkfs, dd)")
        
    if BLOCKED_SYSTEM.search(command):
        raise ValueError("Command rejected: contains system boundary command (shutdown, reboot, mount)")
        
    if BLOCKED_PRIVILEGE.search(command):
        raise ValueError("Command rejected: contains privilege escalation (sudo, su, chown)")
        
    if BLOCKED_PERMISSIONS.search(command):
        raise ValueError("Command rejected: contains dangerous permission change (chmod 777)")
        
    if BLOCKED_FORK_BOMB.search(command):
        raise ValueError("Command rejected: contains fork bomb pattern")
        
    if BLOCKED_NETWORK.search(command):
        raise ValueError("Command rejected: contains blocked network tool (curl, wget, ssh, etc.)")

from pathlib import PurePosixPath

def validate_path(path: str) -> str:
    """Validate and resolve a file path to ensure it stays within /workspace.
    
    Resolves relative paths against /workspace and prevents directory
    traversal attacks using '..'. All paths are treated as POSIX paths
    since they are evaluated inside the Docker container.
    
    Args:
        path: The path provided by the user/tool.
        
    Returns:
        The resolved absolute path as a string (always starts with /workspace).
        
    Raises:
        ValueError: If the path is empty or attempts to escape the workspace.
    """
    if not path or not path.strip():
        raise ValueError("Empty path provided")
        
    posix_path = PurePosixPath(path.strip())
    workspace = PurePosixPath("/workspace")
    
    # If absolute path, it must start with /workspace
    if posix_path.is_absolute():
        if not str(posix_path).startswith(str(workspace)):
            raise ValueError(f"Absolute path must be within {workspace}")
        resolved = posix_path
    else:
        # If relative, resolve against /workspace
        # PurePosixPath doesn't have resolve(), so we combine and normalize via string
        # To normalize '..', we can construct a list of parts
        parts = []
        for part in (workspace.parts + posix_path.parts):
            if part == '..':
                if len(parts) > 1: # Don't pop the root '/'
                    parts.pop()
                else:
                    raise ValueError(f"Path traversal attempted outside {workspace}")
            elif part != '.':
                if part and (not parts or part != '/'):
                    parts.append(part)
        
        # Reconstruct path
        resolved_str = '/' + '/'.join(p for p in parts if p and p != '/')
        resolved = PurePosixPath(resolved_str)
        
    # Final sanity check to ensure it's still in workspace
    if not str(resolved).startswith(str(workspace)):
        raise ValueError(f"Path traversal attempted outside {workspace}")
        
    return str(resolved)

