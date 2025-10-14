cmake_minimum_required(VERSION 3.18)
project(GPULightmassKernel LANGUAGES CXX CUDA)

# Set C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# CUDA setup
set(CMAKE_CUDA_STANDARD 17)
set(CMAKE_CUDA_STANDARD_REQUIRED ON)
enable_language(CUDA)
find_package(CUDAToolkit REQUIRED)

# Platform-specific configurations
if(WIN32)
    add_definitions(-DWIN32 -DWIN64)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")
endif()

# Release build configuration
add_definitions(-DTASKING_TBB -DNDEBUG)

# Source files
set(CUDA_SOURCES
    GPULightmassKernel/rt/rtKernel.cu
)

set(CPP_SOURCES
    GPULightmassKernel/BVH/EmbreeBVHBuilder.cpp
    GPULightmassKernel/GPULightmassKernel.cpp
    GPULightmassKernel/HDRWriter.cpp
    GPULightmassKernel/HostFunc.cpp
    GPULightmassKernel/ProgressReport.cpp
    GPULightmassKernel/Radiosity.cpp
    GPULightmassKernel/StringUtils.cpp
)

set(HEADER_FILES
    GPULightmassKernel/BVH/EmbreeBVHBuilder.h
    GPULightmassKernel/GPULightmassKernel.h
    GPULightmassKernel/HostFunc.h
    GPULightmassKernel/linear_math.h
    GPULightmassKernel/ProgressReport.h
    GPULightmassKernel/Radiosity.h
    GPULightmassKernel/rt/CWBVHCommon.h
    GPULightmassKernel/rt/rtDebugFunc.h
    GPULightmassKernel/rt/rtHelperFunc.h
    GPULightmassKernel/rt/rtHostConfig.h
    GPULightmassKernel/rt/rtKernelDefs.h
    GPULightmassKernel/rt/rtRadiosity.h
    GPULightmassKernel/rt/rtRayBufferKernel.h
    GPULightmassKernel/rt/rtSurfelSorting.h
    GPULightmassKernel/rt/rtTraceCWBVHDynamicFetch.h
    GPULightmassKernel/rt/rtTraceCWBVHFunc.h
    GPULightmassKernel/rt/rtTraceDynamicFetch.h
    GPULightmassKernel/rt/rtTraceFunc.h
    GPULightmassKernel/rt/rtVolumetric.h
    GPULightmassKernel/rt/rtWarpAggregation.h
    GPULightmassKernel/sh_warpreduction.h
    GPULightmassKernel/StringUtils.h
)



# Create shared library
add_library(${PROJECT_NAME} SHARED ${CPP_SOURCES} ${CUDA_SOURCES} ${HEADER_FILES})

# Define that we're building the library
target_compile_definitions(${PROJECT_NAME} PRIVATE GPULIGHTMASSKERNEL_LIB)

# Include directories
target_include_directories(${PROJECT_NAME} PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/GPULightmassKernel
    ${CUDAToolkit_INCLUDE_DIRS}
)

# Search for dependencies in local directories first
list(APPEND CMAKE_PREFIX_PATH 
    ${CMAKE_CURRENT_SOURCE_DIR}/embree_static
    ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss
)

# Find Embree (try local first, then system)
find_path(EMBREE_INCLUDE_DIR embree4/rtcore.h
    PATHS ${CMAKE_CURRENT_SOURCE_DIR}/embree_static/include
    NO_DEFAULT_PATH
)

if(EMBREE_INCLUDE_DIR)
    message(STATUS "Found local Embree: ${EMBREE_INCLUDE_DIR}")
    target_include_directories(${PROJECT_NAME} PRIVATE ${EMBREE_INCLUDE_DIR})
    
    # Find Embree static library
    find_library(EMBREE_LIBRARY 
        NAMES libembree4.a embree4 libembree.a embree
        PATHS ${CMAKE_CURRENT_SOURCE_DIR}/embree_static/lib
        NO_DEFAULT_PATH
    )
    
    if(EMBREE_LIBRARY)
        # Force static linking by using -Wl,--whole-archive
        if(UNIX AND NOT APPLE)
            target_link_libraries(${PROJECT_NAME} PRIVATE 
                -Wl,--whole-archive ${EMBREE_LIBRARY} -Wl,--no-whole-archive
            )
        else()
            target_link_libraries(${PROJECT_NAME} PRIVATE ${EMBREE_LIBRARY})
        endif()
        message(STATUS "Statically linking Embree: ${EMBREE_LIBRARY}")
    else()
        message(FATAL_ERROR "Embree library not found in ${CMAKE_CURRENT_SOURCE_DIR}/embree_static/lib")
    endif()
else()
    # Fallback to system Embree
    find_package(embree 4.0 REQUIRED)
    target_link_libraries(${PROJECT_NAME} PRIVATE embree)
endif()

# Find TBB (try local first, then system)
find_path(TBB_INCLUDE_DIR tbb/tbb.h
    PATHS ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss/include
    NO_DEFAULT_PATH
)

if(TBB_INCLUDE_DIR)
    message(STATUS "Found local TBB: ${TBB_INCLUDE_DIR}")
    target_include_directories(${PROJECT_NAME} PRIVATE ${TBB_INCLUDE_DIR})
    
    # Find TBB static library
    find_library(TBB_LIBRARY 
        NAMES libtbb.a tbb
        PATHS 
            ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss/lib/intel64/gcc4.7
            ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss/lib/intel64/gcc4.4
            ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss/lib/intel64/gcc4.1
            ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss/lib
        NO_DEFAULT_PATH
    )
    
    if(TBB_LIBRARY)
        # Force static linking by using -Wl,--whole-archive
        if(UNIX AND NOT APPLE)
            target_link_libraries(${PROJECT_NAME} PRIVATE 
                -Wl,--whole-archive ${TBB_LIBRARY} -Wl,--no-whole-archive
            )
        else()
            target_link_libraries(${PROJECT_NAME} PRIVATE ${TBB_LIBRARY})
        endif()
        message(STATUS "Statically linking TBB: ${TBB_LIBRARY}")
    else()
        message(FATAL_ERROR "TBB library not found in ${CMAKE_CURRENT_SOURCE_DIR}/tbb2019_20190605oss/lib")
    endif()
else()
    # Fallback to system TBB
    find_package(TBB REQUIRED)
    target_link_libraries(${PROJECT_NAME} PRIVATE TBB::tbb)
endif()

if(UNIX AND NOT APPLE)
    target_link_libraries(${PROJECT_NAME} PRIVATE pthread dl)
endif()

# Link CUDA libraries
target_link_libraries(${PROJECT_NAME} PRIVATE CUDA::cudart_static)

# OpenMP support
find_package(OpenMP)
if(OpenMP_CXX_FOUND)
    target_link_libraries(${PROJECT_NAME} PRIVATE OpenMP::OpenMP_CXX)
endif()

# CUDA compilation flags
set_target_properties(${PROJECT_NAME} PROPERTIES
    CUDA_SEPARABLE_COMPILATION ON
    CUDA_RESOLVE_DEVICE_SYMBOLS ON
)

# CUDA compiler options
target_compile_options(${PROJECT_NAME} PRIVATE
    $<$<COMPILE_LANGUAGE:CUDA>:
        --use_fast_math
        -Xcompiler=-w
        $<$<PLATFORM_ID:Windows>:-Xcompiler=/wd4819,/wd4267,/wd4996>
    >
)

# C++ compiler options
if(MSVC)
    target_compile_options(${PROJECT_NAME} PRIVATE
        /W3
        /O2
        /Oi  # Intrinsic functions
        /GL  # Whole program optimization
    )
    set_target_properties(${PROJECT_NAME} PROPERTIES
        LINK_FLAGS "/LTCG"  # Link-time code generation
    )
elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
    target_compile_options(${PROJECT_NAME} PRIVATE
        -O3
        -march=native
        -Wno-changes-meaning  # Fix for old TBB with modern GCC
        -Wno-deprecated-declarations  # Suppress wstring_convert deprecation warnings
    )
endif()

# Set output directory
set_target_properties(${PROJECT_NAME} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin
    LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
    ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib
)

# Optional: Install rules for UE5 integration
option(INSTALL_TO_UE5 "Install to Unreal Engine 5 directories" OFF)

if(INSTALL_TO_UE5)
    if(DEFINED ENV{UE5_ROOT})
        set(UE5_ROOT $ENV{UE5_ROOT})
    else()
        set(UE5_ROOT "D:/UE5" CACHE PATH "Path to Unreal Engine 5 installation")
    endif()

    # Install header
    install(FILES GPULightmassKernel/GPULightmassKernel.h
            DESTINATION ${UE5_ROOT}/Engine/Source/ThirdParty/GPULightmass/${CMAKE_SYSTEM_NAME}/include/)

    # Install library files
    install(TARGETS ${PROJECT_NAME}
            RUNTIME DESTINATION ${UE5_ROOT}/Engine/Binaries/${CMAKE_SYSTEM_NAME}
            LIBRARY DESTINATION ${UE5_ROOT}/Engine/Source/ThirdParty/GPULightmass/${CMAKE_SYSTEM_NAME}/lib/
            ARCHIVE DESTINATION ${UE5_ROOT}/Engine/Source/ThirdParty/GPULightmass/${CMAKE_SYSTEM_NAME}/lib/)
endif()

# Print configuration summary
message(STATUS "=== GPULightmassKernel Configuration ===")
message(STATUS "CUDA Version: ${CUDAToolkit_VERSION}")
message(STATUS "CUDA Architectures: ${CMAKE_CUDA_ARCHITECTURES}")
message(STATUS "Build Type: ${CMAKE_BUILD_TYPE}")
message(STATUS "Install to UE5: ${INSTALL_TO_UE5}")
if(INSTALL_TO_UE5)
    message(STATUS "UE5 Root: ${UE5_ROOT}")
endif()
message(STATUS "=======================================")