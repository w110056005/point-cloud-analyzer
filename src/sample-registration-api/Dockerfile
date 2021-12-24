FROM python:3.7-slim

WORKDIR /app

ADD . /app

RUN pip install -r requirements.txt

# Install Open3D system dependencies and pip
RUN apt-get update && apt-get install --install-recommends -y \
    gcc \
    libgl1 \
    libgomp1 \
    libusb-1.0-0 \
    python3-pip \
    && rm -rf /var/lib/apt/lists/*
RUN python3 -m pip install --upgrade pip
RUN python3 -m pip install --upgrade setuptools

# Install Open3D from the pypi repositories
RUN python3 -m pip install open3d

CMD python main.py