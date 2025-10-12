GPULightmassKernel - CMake Build Instructions
Cross-platform CMake build system for GPULightmassKernel supporting both Windows and Linux.

Prerequisites
Common Requirements
CMake 3.18 or higher
CUDA Toolkit 12.6 (or compatible version)
C++17 compatible compiler
Dependencies
Embree 3.x - Ray tracing kernels library
Intel TBB - Threading Building Blocks
CUDA Runtime - NVIDIA CUDA Toolkit
Windows Specific
Visual Studio 2022 (v143 toolset) or Visual Studio 2019 (v142)
CUDA installed with Visual Studio integration
Linux Specific
GCC 9+ or Clang 10+
CUDA Toolkit for Linux
Make or Ninja build system
Installation of Dependencies
Windows
Embree
batch
# Download from: https://github.com/embree/embree/releases
# Install to D:\embree_static or customize path in build script
Intel TBB
batch
# Download from: https://github.com/oneapi-src/oneTBB/releases
# Install to D:\tbb2019_20190605oss or customize path
CUDA
batch
# Download from: https://developer.nvidia.com/cuda-downloads
# Default installation path: C:\Program Files\NVIDIA GPU Computing Toolkit\CUDA\v12.6
Linux
Ubuntu/Debian
bash
# Install dependencies
sudo apt update
sudo apt install build-essential cmake

# Install CUDA (follow NVIDIA official guide)
# https://developer.nvidia.com/cuda-downloads

# Install Embree
sudo apt install libembree-dev

# Install TBB
sudo apt install libtbb-dev
Fedora/RHEL
bash
sudo dnf install cmake gcc-c++
sudo dnf install embree-devel tbb-devel
# Install CUDA following NVIDIA's guide
Building the Project
Quick Start - Windows
Edit build_windows.bat and customize paths:
EMBREE_ROOT
TBB_ROOT
UE5_ROOT (if using INSTALL_TO_UE5)
CUDA_TOOLKIT_ROOT_DIR
Run the build script:
batch
   build_windows.bat
Quick Start - Linux
Edit build_linux.sh and customize paths if needed
Make script executable and run:
bash
   chmod +x build_linux.sh
   ./build_linux.sh
Manual Build
Windows
batch
mkdir build
cd build

cmake .. -G "Visual Studio 17 2022" -A x64 ^
    -DCMAKE_BUILD_TYPE=Release ^
    -DCMAKE_CUDA_ARCHITECTURES=75;86;89 ^
    -Dembree_DIR="D:/embree_static/lib/cmake/embree-3.0" ^
    -DTBB_ROOT="D:/tbb2019_20190605oss" ^
    -DINSTALL_TO_UE5=ON ^
    -DUE5_ROOT="D:/UE5"

cmake --build . --config Release -j
cmake --install . --config Release
Linux
bash
mkdir build && cd build

cmake .. \
    -DCMAKE_BUILD_TYPE=Release \
    -DCMAKE_CUDA_ARCHITECTURES=75;86;89 \
    -Dembree_DIR=/usr/local/lib/cmake/embree-3.0 \
    -DTBB_ROOT=/usr/local \
    -DINSTALL_TO_UE5=OFF

cmake --build . -j$(nproc)
CMake Options
Option	Description	Default
CMAKE_BUILD_TYPE	Build configuration (Release/Debug)	Release
CMAKE_CUDA_ARCHITECTURES	CUDA compute capabilities	75;86;89
INSTALL_TO_UE5	Install to UE5 directories	OFF
UE5_ROOT	Unreal Engine 5 installation path	D:/UE5 (Windows)
embree_DIR	Path to Embree CMake config	(auto-detect)
TBB_ROOT	Path to TBB installation	(auto-detect)
CUDA Architectures
Common CUDA compute capabilities:

75 - RTX 20 series (Turing)
86 - RTX 30 series (Ampere)
89 - RTX 40 series (Ada Lovelace)
90 - RTX 40 series (Hopper)
Customize via: -DCMAKE_CUDA_ARCHITECTURES="75;86;89;90"

Output
Compiled files will be placed in:

build/
├── bin/                          # Runtime binaries (DLL/SO)
├── lib/                          # Link libraries
│   ├── GPULightmassKernel.dll    # Windows
│   ├── GPULightmassKernel.lib    # Windows import library
│   └── libGPULightmassKernel.so  # Linux
If INSTALL_TO_UE5=ON, files will be copied to:

<UE5_ROOT>/
├── Engine/
│   ├── Binaries/
│   │   └── Win64/GPULightmassKernel.dll
│   └── Source/ThirdParty/GPULightmass/
│       └── Win64/
│           ├── include/GPULightmassKernel.h
│           └── lib/GPULightmassKernel.{dll,lib}
Troubleshooting
CUDA not found
bash
# Set CUDA path manually
export CUDA_TOOLKIT_ROOT_DIR=/usr/local/cuda-12.6
Embree not found
bash
# Specify Embree path
-Dembree_DIR=/path/to/embree/lib/cmake/embree-3.0
TBB not found
bash
# Specify TBB root
-DTBB_ROOT=/path/to/tbb
Wrong CUDA architecture
Edit the CMake command and adjust architectures for your GPU:

bash
-DCMAKE_CUDA_ARCHITECTURES="86"  # For RTX 3080 only
Linux: Missing OpenMP
bash
sudo apt install libomp-dev  # Ubuntu/Debian
sudo dnf install libomp-devel  # Fedora
Integration with Unreal Engine
To use with your custom UE5 build:

Set INSTALL_TO_UE5=ON
Set UE5_ROOT to your engine path
Build and install
Rebuild your UE5 project
Or set environment variable:

bash
export UE5_ROOT=/path/to/UnrealEngine
License
Check the original project license for terms and conditions.

