FROM python:3.11-slim

# Create non-root user
RUN useradd -m -u 1000 sandbox

# Install system dependencies for common data-science stacks
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        build-essential \
        libblas-dev \
        liblapack-dev \
        gfortran \
        libopenblas-dev \
        libffi-dev \
        libssl-dev \
        git \
    && rm -rf /var/lib/apt/lists/*

# Install Python libraries
RUN pip install --no-cache-dir \
    pandas \
    numpy \
    matplotlib \
    seaborn \
    scikit-learn \
    openpyxl

# Workspace directory
WORKDIR /workspace

# Use non-root user
USER sandbox

# Default command keeps container alive for exec-based workloads
CMD ["sleep", "infinity"]

