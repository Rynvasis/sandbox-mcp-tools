"""Unit tests for validator components."""
import pytest
from sandbox.validator import validate_command

def test_validate_command_allowed():
    """Test commands that should be permitted."""
    allowed = [
        "ls /workspace",
        "echo hello",
        "python script.py",
        "cat /tmp/test.txt",
        "grep pattern file.txt"
    ]
    for cmd in allowed:
        # Should not raise any exception
        validate_command(cmd)

def test_validate_command_empty():
    """Test empty command string rejection."""
    with pytest.raises(ValueError, match="Empty command"):
        validate_command("")
        
    with pytest.raises(ValueError, match="Empty command"):
        validate_command("   ")

def test_validate_command_destructive():
    """Test destructive command patterns."""
    blocked = [
        "rm -rf /",
        "rm  -rf  /",
        "mkfs.ext4 /dev/sda",
        "dd if=/dev/zero of=/dev/sda"
    ]
    for cmd in blocked:
        with pytest.raises(ValueError, match=r"destructive pattern"):
            validate_command(cmd)

def test_validate_command_system():
    """Test system boundary commands."""
    blocked = [
        "shutdown -h now",
        "reboot",
        "mount /dev/sda /mnt",
        "umount /mnt"
    ]
    for cmd in blocked:
        with pytest.raises(ValueError, match=r"system boundary"):
            validate_command(cmd)

def test_validate_command_privilege():
    """Test privilege escalation commands."""
    blocked = [
        "sudo ls /workspace",
        "su sandbox",
        "chown root:root /workspace/file"
    ]
    for cmd in blocked:
        with pytest.raises(ValueError, match=r"privilege escalation"):
            validate_command(cmd)

def test_validate_command_permissions():
    """Test dangerous permission changes and fork bombs."""
    with pytest.raises(ValueError, match=r"dangerous permission change"):
        validate_command("chmod 777 script.py")
        
    with pytest.raises(ValueError, match=r"fork bomb pattern"):
        validate_command(":(){ :|:& };:")

def test_validate_command_network():
    """Test blocked network tools."""
    blocked = [
        "curl http://example.com",
        "wget http://example.com",
        "nc -l 8080",
        "ssh user@host",
        "scp file user@host:/"
    ]
    for cmd in blocked:
        with pytest.raises(ValueError, match=r"blocked network tool"):
            validate_command(cmd)

def test_validate_command_pipes():
    """Test blocked commands embedded in pipes."""
    blocked = [
        "echo hello | rm -rf /",
        "ls && sudo rm /etc/passwd"
    ]
    for cmd in blocked:
        with pytest.raises(ValueError):
            validate_command(cmd)

from sandbox.validator import validate_path

def test_validate_path_allowed():
    """Test valid paths resolve correctly."""
    assert validate_path("file.txt") == "/workspace/file.txt"
    assert validate_path("./file.txt") == "/workspace/file.txt"
    assert validate_path("dir/file.txt") == "/workspace/dir/file.txt"
    assert validate_path("/workspace/file.txt") == "/workspace/file.txt"
    
def test_validate_path_empty():
    """Test empty path rejection."""
    with pytest.raises(ValueError, match="Empty path"):
        validate_path("")
    with pytest.raises(ValueError, match="Empty path"):
        validate_path("   ")

def test_validate_path_absolute_traversal():
    """Test absolute paths outside workspace."""
    blocked = [
        "/tmp/file.txt",
        "/etc/passwd",
        "/roo/file.txt"
    ]
    for path in blocked:
        with pytest.raises(ValueError, match="Absolute path must be within"):
            validate_path(path)

def test_validate_path_relative_traversal():
    """Test relative paths that attempt to escape workspace."""
    blocked = [
        "..",
        "../file.txt",
        "dir/../../file.txt",
        "../../etc/passwd"
    ]
    for path in blocked:
        with pytest.raises(ValueError, match="Path traversal attempted outside"):
            validate_path(path)

def test_validate_path_embedded_traversal():
    """Test paths that use .. but stay within workspace."""
    # These are technically safe, but our logic resolves them cleanly
    assert validate_path("dir/../file.txt") == "/workspace/file.txt"
    assert validate_path("dir1/dir2/../../file.txt") == "/workspace/file.txt"


