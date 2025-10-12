mkdir build && cd build

cmake .. \
    -DCMAKE_BUILD_TYPE=Release \
    -DCMAKE_CUDA_ARCHITECTURES=75;86;89 \
    -Dembree_DIR=./embree_static \
    -DTBB_ROOT=./tbb2019_20190605oss \
    -DINSTALL_TO_UE5=OFF

cmake --build . -j$(nproc)