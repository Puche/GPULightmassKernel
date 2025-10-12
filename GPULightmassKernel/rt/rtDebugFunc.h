namespace GPULightmass
{
	void LOG(const char* format, ...);
}
#undef NDEBUG
#include <cassert>
#include <cstdio>
#ifdef _WIN32
#include <Windows.h>
#endif

#ifdef _WIN32
#define cudaCheck(x) \
	{ \
		cudaError_t err = (x); \
		if (err != cudaSuccess) { \
			GPULightmass::LOG("Line %d: cudaCheckError: %s", __LINE__, cudaGetErrorString(err)); \
			MessageBoxA(0, cudaGetErrorString(err), "Error", 0); \
			assert(0); \
		} \
	}
#else
#define cudaCheck(x) \
	{ \
		cudaError_t err = (x); \
		if (err != cudaSuccess) { \
			GPULightmass::LOG("Line %d: cudaCheckError: %s", __LINE__, cudaGetErrorString(err)); \
			fprintf(stderr, "CUDA Error at line %d: %s\n", __LINE__, cudaGetErrorString(err)); \
			assert(0); \
		} \
	}
#endif

#ifdef _WIN32
#define cudaPostKernelLaunchCheck \
{ \
	cudaError_t err = cudaGetLastError(); \
	if (err != cudaSuccess) \
	{ \
		GPULightmass::LOG("PostKernelLaunchError: %s", cudaGetErrorString(err)); \
		MessageBoxA(0, cudaGetErrorString(err), "Error", 0); \
		assert(0); \
	} \
}
#else
#define cudaPostKernelLaunchCheck \
{ \
	cudaError_t err = cudaGetLastError(); \
	if (err != cudaSuccess) \
	{ \
		GPULightmass::LOG("PostKernelLaunchError: %s", cudaGetErrorString(err)); \
		fprintf(stderr, "CUDA Post Kernel Launch Error: %s\n", cudaGetErrorString(err)); \
		assert(0); \
	} \
}
#endif