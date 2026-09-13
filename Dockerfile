FROM mcr.microsoft.com/dotnet/sdk:10.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    software-properties-common \
    && add-apt-repository ppa:deadsnakes/ppa -y \
    && apt-get update && apt-get install -y --no-install-recommends \
    python3.11 \
    python3.11-venv \
    python3-pip \
    libgl1 \
    libglib2.0-0 \
    libfontconfig1 \
    libx11-xcb1 \
    libdbus-1-3 \
    libxrender1 \
    libxext6 \
    libsm6 \
    libxcb-cursor0 \
    libxcb-xinerama0 \
    libxcb-icccm4 \
    libxcb-image0 \
    libxcb-keysyms1 \
    libxcb-randr0 \
    libxcb-render-util0 \
    libxcb-shape0 \
    libxkbcommon-x11-0 \
    && rm -rf /var/lib/apt-get/lists/*
    
ENV QT_QPA_PLATFORM=offscreen
WORKDIR /app

RUN python3.11 -m venv /app/venv
ENV PATH="/app/venv/bin:$PATH"

COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

WORKDIR /app/phylogenetic-project
RUN dotnet build

ENTRYPOINT ["dotnet", "run", "--no-build", "--"]